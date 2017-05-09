using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RopeSnake.Core
{
    public class Block
    {
        protected byte[] _data;
        public byte[] Data => _data;
        public int Length => _data.Length;

        public byte this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public Block() : this(0) { }

        public Block(int length)
        {
            if (length < 0)
                throw new ArgumentException(nameof(length));

            _data = new byte[length];
        }

        public Block(byte[] copyFrom)
        {
            if (copyFrom == null)
                throw new ArgumentNullException(nameof(copyFrom));

            _data = copyFrom.ToArray();
        }

        public Block(Block copyFrom) : this(copyFrom._data) { }

        public virtual void Resize(int newLength)
        {
            if (newLength < 0)
                throw new ArgumentException(nameof(newLength));

            Array.Resize(ref _data, newLength);
        }

        public virtual BlockStream ToStream(int offset = 0)
            => new BlockStream(this) { Position = offset };

        public void CopyTo(Block destination)
            => CopyTo(destination, 0);

        public void CopyTo(Block destination, int destinationOffset)
            => CopyTo(destination, destinationOffset, Length);

        public void CopyTo(Block destination, int destinationOffset, int length)
            => CopyTo(destination, 0, destinationOffset, length);

        public virtual void CopyTo(Block destination, int sourceOffset, int destinationOffset, int length)
        {
            RLog.Trace($"Copying {length} bytes from offset 0x{sourceOffset} to offset 0x{destinationOffset}");

            if (sourceOffset + length > destination.Length)
            {
                RLog.Trace($"Destination block too small; resizing to {length}");
                Array.Resize(ref destination._data, sourceOffset + length);
            }

            Array.Copy(_data, sourceOffset, destination._data, destinationOffset, length);
        }

        public virtual void ReadFromFile(string fileName)
        {
            RLog.Debug($"Reading block from file {fileName}");
            _data = File.ReadAllBytes(fileName);
        }

        public virtual void WriteToFile(string fileName)
        {
            RLog.Debug($"Writing block to file {fileName}");
            File.WriteAllBytes(fileName, _data);
        }
    }
}
