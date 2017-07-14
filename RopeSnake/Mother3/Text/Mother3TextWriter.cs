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

        public static Mother3TextWriter Create(Rom rom, bool isCompressed, bool isEncoded)
        {
            var type = rom.Type;

            if (type.Game != "Mother 3")
                throw new NotSupportedException(type.Game);

            StreamTokenWriter writer;
            var charMap = Mother3Config.CreateCharacterMap(rom.Type);
            var codes = Mother3Config.Configs[rom.Type].ControlCodes;

            switch (type.Version)
            {
                case "jp":
                    writer = new StreamTokenWriter(rom.ToStream(), charMap);
                    break;

                case "en-v10":
                    writer = new EnglishStreamTokenWriter(rom.ToStream(),
                        charMap,
                        isCompressed,
                        isEncoded ? Mother3Config.Configs[type].ScriptEncodingParameters : null);
                    break;

                default:
                    RLog.Warn($"Unrecognized ROM version: {type.Version}. Defaulting to Japanese character map.");
                    goto case "jp";
            }

            var reader = new StringTokenReader(codes);

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
