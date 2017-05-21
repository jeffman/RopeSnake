using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    public class Mother3TextWriter
    {
        public Stream BaseStream => _streamWriter.BaseStream;
        internal StreamTokenWriter _streamWriter;
        internal StringTokenReader _stringReader;

        internal Mother3TextWriter(StreamTokenWriter writer, StringTokenReader reader)
        {
            _streamWriter = writer;
            _stringReader = reader;
        }

        public static Mother3TextWriter Create(Rom rom)
        {
            var writer = new StreamTokenWriter(rom.ToStream(), Mother3Config.CreateCharacterMap(rom.Type));
            var reader = new StringTokenReader(Mother3Config.Configs[rom.Type].ControlCodes);

            return new Mother3TextWriter(writer, reader);
        }

        public void WriteString(string str) => WriteString(str, -1);

        public void WriteString(string str, int bytesToWrite)
        {
            _streamWriter.Reset();
            _stringReader.Reset();
            _stringReader.BaseString = str;

            long oldPosition = BaseStream.Position;

            while ((bytesToWrite == -1) || (BaseStream.Position - oldPosition < bytesToWrite))
            {
                var token = _stringReader.Read();

                if (token == null)
                {
                    _streamWriter.Write(new RawToken(-1));
                    break;
                }

                _streamWriter.Write(token);

                if (token.IsTerminator)
                    break;
            }

            if ((bytesToWrite >= 0) && BaseStream.Position != oldPosition + bytesToWrite)
                BaseStream.Position = oldPosition + bytesToWrite;
        }
    }
}
