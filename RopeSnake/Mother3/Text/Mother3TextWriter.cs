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
        internal BlockTokenWriter _blockWriter;
        internal StringTokenReader _stringReader;

        internal Mother3TextWriter(BlockTokenWriter writer, StringTokenReader reader)
        {
            _blockWriter = writer;
            _stringReader = reader;
        }

        public static Mother3TextWriter Create(Block destination, RomType type, bool isCompressed = false, bool isEncoded = false)
        {
            if (type.Game != "Mother 3")
                throw new NotSupportedException(type.Game);

            BlockTokenWriter writer;
            var charMap = Mother3Config.CreateCharacterMap(type);
            var codes = Mother3Config.Configs[type].ControlCodes;

            switch (type.Version)
            {
                case "jp":
                    writer = new BlockTokenWriter(destination, charMap);
                    break;

                case "en-v10":
                    writer = new EnglishBlockTokenWriter(
                        destination,
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

        public int WriteString(int offset, string str) => WriteString(offset, str, -1);

        public int WriteString(int offset, string str, int bytesToWrite)
        {
            _blockWriter.Reset();
            _blockWriter.Position = offset;
            _stringReader.Reset();
            _stringReader.BaseString = str;

            while ((bytesToWrite == -1) || (_blockWriter.Position - offset < bytesToWrite))
            {
                var token = _stringReader.Read();

                if (token == null)
                {
                    _blockWriter.Write(new RawToken(-1));
                    break;
                }

                _blockWriter.Write(token);

                if (token.IsTerminator)
                    break;
            }

            if (bytesToWrite >= 0)
                _blockWriter.Position = offset + bytesToWrite;

            return _blockWriter.Position - offset;
        }
    }
}
