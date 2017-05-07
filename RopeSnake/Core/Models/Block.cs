using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RopeSnake.Core
{
    public class Block
    {
        protected byte[] _data;
        public byte[] Data => _data;
        public int Length => Data.Length;

        public byte this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public Block(int length)
        {
            if (length < 0)
                throw new ArgumentException(nameof(length));

            _data = new byte[length];
        }

        public Block(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _data = data.ToArray();
        }

        public Block(Block copyFrom)
        {
            if (copyFrom == null)
                throw new ArgumentNullException(nameof(copyFrom));

            _data = new byte[copyFrom.Length];
            Array.Copy(copyFrom.Data, _data, _data.Length);
        }

        public void Resize(int newLength)
        {
            if (newLength < 0)
                throw new ArgumentException(nameof(newLength));

            Array.Resize(ref _data, newLength);
        }

        public static implicit operator byte[](Block block)
        {
            return block.Data;
        }

        public static implicit operator Block(byte[] data)
        {
            return new Block(data);
        }

        public BlockStream ToStream(int offset = 0)
        {
            var stream = new BlockStream(this);
            stream.Position = offset;
            return stream;
        }
    }
}
