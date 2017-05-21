using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public sealed class BlockStream : Stream
    {
        public Block Block { get; }

        public BlockStream(Block block)
        {
            Block = block;
        }

        public BlockStream(int size)
        {
            Block = new Block(size);
        }

        #region Stream implementation

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => Block.Length;

        public override long Position { get; set; }

        public override void Flush() { }

        public override int ReadByte()
            => Block[(int)Position++];

        public override int Read(byte[] buffer, int offset, int count)
        {
            Array.Copy(Block.Data, (int)Position, buffer, offset, count);
            Position += count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            if (value > int.MaxValue)
                throw new ArgumentException(nameof(value));

            Block.Resize((int)value);
        }

        public override void WriteByte(byte value)
            => Block[(int)Position++] = value;

        public override void Write(byte[] buffer, int offset, int count)
        {
            Array.Copy(buffer, offset, Block.Data, (int)Position, count);
            Position += count;
        }

        #endregion
    }
}
