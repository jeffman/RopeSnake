using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    public class Mother3TextReader
    {
        internal BlockTokenReader _blockReader;
        internal StringTokenWriter _stringWriter;

        internal Mother3TextReader(BlockTokenReader streamReader, StringTokenWriter stringWriter)
        {
            _blockReader = streamReader;
            _stringWriter = stringWriter;
        }

        public static Mother3TextReader Create(Rom rom, bool isCompressed, bool isEncoded)
            => Create(rom, rom.Type, isCompressed, isEncoded);

        public static Mother3TextReader Create(Block source, RomType type, bool isCompressed, bool isEncoded)
        {
            if (type.Game != "Mother 3")
                throw new NotSupportedException(type.Game);

            BlockTokenReader reader;
            var charMap = Mother3Config.CreateCharacterMap(type);
            var codes = Mother3Config.Configs[type].ControlCodes;

            switch (type.Version)
            {
                case "jp":
                    reader = new BlockTokenReader(source, charMap, codes);
                    break;

                case "en-v10":
                    reader = new EnglishBlockTokenReader(
                        source,
                        charMap,
                        codes,
                        isCompressed,
                        isEncoded ? Mother3Config.Configs[type].ScriptEncodingParameters : null);
                    break;

                default:
                    RLog.Warn($"Unrecognized ROM version: {type.Version}. Defaulting to Japanese character map.");
                    goto case "jp";
            }

            var writer = new StringTokenWriter(new StringBuilder());

            return new Mother3TextReader(reader, writer);
        }

        public string ReadString(int offset) => ReadString(offset, -1);

        public string ReadString(int offset, int bytesToRead)
        {
            if (bytesToRead < -1)
                throw new ArgumentException(nameof(bytesToRead));

            _blockReader.Reset();
            _stringWriter.Reset();
            _stringWriter.Output.Clear();

            _blockReader.Position = offset;

            while ((bytesToRead == -1) || (_blockReader.Position - offset < bytesToRead))
            {
                var token = _blockReader.Read();

                if (token.IsTerminator)
                    break;

                _stringWriter.Write(token);
            }

            if (bytesToRead >= 0)
                _blockReader.Position = offset + bytesToRead;

            return _stringWriter.Output.ToString();
        }
    }
}
