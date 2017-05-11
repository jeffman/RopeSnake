using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;
using System.Threading;

namespace RopeSnake.Gba
{
    public sealed class Lz77Compressor : ICompressor
    {
        private const int _compressBufferSize = 128 * 1024;
        private static readonly ThreadLocal<byte[]> _compressBuffers;

        public bool CompressToVram { get; }

        static Lz77Compressor()
        {
            _compressBuffers = new ThreadLocal<byte[]>(() => new byte[_compressBufferSize]);
        }

        public Lz77Compressor(bool compressToVram = true)
        {
            CompressToVram = compressToVram;
        }

        /// <summary>
        /// Compresses an input sequence to a GBA-compatible LZ77 compressed sequence.
        /// </summary>
        /// <param name="source">source data</param>
        /// <param name="offset">source data offset</param>
        /// <param name="length">source data length</param>
        /// <returns>compressed sequence</returns>
        /// <remarks>
        /// An LZ77 stream is fairly straightforward. The first four bytes are a header:
        ///   [00]: Always 0x10
        ///   [01-03]: Length of uncompressed data in bytes (little endian)
        /// 
        /// What follows is a sequence of blocks. Each block has an 8-bit mode header,
        /// ordered from MSB to LSB. Immediately following this header is a sequence of 8 chunks,
        /// whose meaning depends on the corresponding bit in the mode header.
        /// 
        /// If the mode bit is 0, then the chunk is a single byte that should be copied directly
        /// to the uncompressed output.
        /// 
        /// If the mode bit is 1, then we will be looking up a string from the partially-uncompressed
        /// output buffer and copying it to the end of the buffer. This string is described by
        /// a distance value and a length value, where the distance is how far backwards from the
        /// current output position the string begins and the length is how many bytes to copy.
        /// The chunk is a 16-bit value, where the lowest 12 bits is (distance - 1) and the
        /// highest 4 bits is (length - 3);
        /// 
        /// Lookup strings are allowed to overlap the current input position.
        /// 
        /// There is a hardware bug when uncompressing to VRAM where garbage data will be written if
        /// the distance is only 1. This has something to do with the VRAM bus being 16 bits wide
        /// or whatever. Anyway, for any data that is to be uncompressed directly to VRAM, then
        /// the CompressToVram flag should be true (and it is by default since this is by far the
        /// most common use case), and consequently the minimum distance should be 2. This is why
        /// compressed streams tend to have a lot of [F0 01] chunks rather than [F0 00].
        /// 
        /// The specific algorithm being used is an optimized brute-force, since the encoding is
        /// so straightforward and the lookup window is fixed to a 12-bit distance and 4-bit length
        /// -- we can only search 0x1000 bytes backwards for strings of at most 18 bytes in length,
        /// which makes brute-forcing O(n) with the input size; albeit still a slow algorithm if
        /// using naive searching.
        /// 
        /// The main optimization comes from using linked lists to store the starting position of
        /// each of the 256 possible starting bytes of the current input string.
        /// 
        /// This implementation is thread-safe.
        /// </remarks>
        public byte[] Compress(byte[] source, int offset, int length)
        {
            if (length < 0)
                throw new ArgumentException(nameof(length));

            if (offset + length > source.Length)
                throw new ArgumentException(nameof(length));

            LinkedList<int>[] lookup = new LinkedList<int>[256];
            for (int i = 0; i < 256; i++)
                lookup[i] = new LinkedList<int>();

            // Overall compressed buffer
            byte[] compBuffer = _compressBuffers.Value;
            int compPosition = 0;

            // Current mode byte (we'll be using 8 bits at a time)
            int modeByte = 0;
            int modeIndex = 0;

            // tempBuffer contains the current sequence of chunks corresponding to the current
            // mode byte. Since the mode byte as 8 bits and each chunk has at most 16 bits, then
            // the largest possible chunk sequence is 8*16 bits = 16 bytes
            byte[] tempBuffer = new byte[16];
            int tempIndex = 0;

            int minimumDistance = CompressToVram ? 2 : 1;

            // Start by writing the header
            compBuffer[compPosition++] = 0x10;
            compBuffer[compPosition++] = (byte)(length & 0xFF);
            compBuffer[compPosition++] = (byte)((length >> 8) & 0xFF);
            compBuffer[compPosition++] = (byte)((length >> 16) & 0xFF);

            // First byte needs to be a direct copy (we don't have any strings yet in the
            // uncompressed buffer)
            byte firstValue = source[offset];
            addDirectCopyChunk(firstValue);
            lookup[firstValue].AddFirst(0);

            // Start the algorithm from the second byte
            for (int sourceIndex = 1; sourceIndex < length; )
            {
                byte sourceValue = source[offset + sourceIndex];

                // Find the longest string already in the uncompressed buffer
                // which starts with sourceValue
                var pastStringPositions = lookup[sourceValue];

                if (pastStringPositions.Count == 0)
                {
                    // No strings exist which start with sourceValue; use direct copy mode
                    addDirectCopyChunk(sourceValue);
                    pastStringPositions.AddFirst(sourceIndex);
                    sourceIndex++;
                }
                else
                {
                    // Search all past positions for longest string
                    int longestMatchPosition = 0;
                    int longestMatchLength = 0;

                    int longestPossibleMatchLength = Math.Min(18, length - sourceIndex);

                    if (longestPossibleMatchLength >= 3)
                    {
                        foreach (int pastStringPosition in pastStringPositions)
                        {
                            // Don't bother looking past 0x1000 bytes away, we can't use them
                            if (sourceIndex - pastStringPosition > 0x1000)
                                break;

                            // String must be at least 1 byte away (or 2 for VRAM)
                            if (sourceIndex - pastStringPosition < minimumDistance)
                                continue;

                            // See how long this string match is
                            int matchLength = 1;
                            for (int i = 1; i < longestPossibleMatchLength; i++)
                            {
                                if (source[offset + sourceIndex + i] != source[offset + pastStringPosition + i])
                                    break;

                                matchLength++;
                            }

                            if (matchLength < 3)
                                continue;

                            if (matchLength > longestMatchLength)
                            {
                                // Record new best match
                                longestMatchPosition = pastStringPosition;
                                longestMatchLength = matchLength;

                                // We can stop looking if we've hit the longest possible length
                                if (matchLength == longestPossibleMatchLength)
                                    break;
                            }
                        }
                    }

                    // Whether or not we found a string, the current byte is going to the uncompressed
                    // buffer, so add it to the lookup table
                    pastStringPositions.AddFirst(sourceIndex);

                    if (longestMatchLength > 0)
                    {
                        addLookupChunk(sourceIndex - longestMatchPosition, longestMatchLength);

                        // Add the rest of the string to the lookup table
                        for (int i = 1; i < longestMatchLength; i++)
                        {
                            byte match = source[offset + sourceIndex + i];
                            lookup[match].AddFirst(sourceIndex + i);
                        }

                        sourceIndex += longestMatchLength;
                    }
                    else
                    {
                        // Couldn't find a long enough string; fall back to direct copy
                        addDirectCopyChunk(sourceValue);
                        sourceIndex++;
                    }
                }
            }

            // Flush the mode byte since we're done
            if (modeIndex > 0)
                flush();

            Array.Resize(ref compBuffer, compPosition);
            return compBuffer;

            void addDirectCopyChunk(byte directCopyValue)
            {
                tempBuffer[tempIndex++] = directCopyValue;
                modeIndex++;

                if (modeIndex == 8)
                    flush();
            }

            void addLookupChunk(int lookupDistance, int lookupLength)
            {
                int lookupChunk = (lookupDistance - 1) | ((lookupLength - 3) << 12);
                tempBuffer[tempIndex++] = (byte)((lookupChunk >> 8) & 0xFF);
                tempBuffer[tempIndex++] = (byte)(lookupChunk & 0xFF);

                modeByte |= (1 << (7 - modeIndex));
                modeIndex++;

                if (modeIndex == 8)
                    flush();
            }

            void flush()
            {
                compBuffer[compPosition++] = (byte)modeByte;
                Array.Copy(tempBuffer, 0, compBuffer, compPosition, tempIndex);
                compPosition += tempIndex;

                modeByte = 0;
                modeIndex = 0;
                tempIndex = 0;
            }
        }

        public byte[] Decompress(byte[] source, int offset)
        {
            // Check for LZ77 signature
            if (source[offset++] != 0x10)
                throw new Exception($"Expected LZ77 header");

            // Read the block length
            int length = source[offset++];
            length += (source[offset++] << 8);
            length += (source[offset++] << 16);

            byte[] decompressed = new byte[length];
            int decompPosition = 0;

            while (decompPosition < length)
            {
                byte mode = source[offset++];
                for (int i = 0; i < 8; i++)
                {
                    switch ((mode >> (7 - i)) & 1)
                    {
                        case 0:

                            // Direct copy
                            if (decompPosition >= length)
                                break;

                            decompressed[decompPosition++] = source[offset++];
                            break;

                        case 1:

                            // String lookup
                            int lookup = (source[offset++] << 8);
                            lookup += source[offset++];

                            int numBytes = ((lookup >> 12) & 0xF) + 3;
                            int distance = (lookup & 0xFFF);

                            for (int j = 0; j < numBytes; j++)
                            {
                                if (decompPosition >= length)
                                    break;

                                decompressed[decompPosition] = decompressed[decompPosition - distance - 1];
                                decompPosition++;
                            }

                            break;

                        default:
                            break;
                    }
                }
            }

            return decompressed;
        }
    }

    public sealed class Lz77Exception : Exception
    {
        public Lz77Exception(string message) : base(message) { }
    }
}
