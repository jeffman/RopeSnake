using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using RopeSnake.Core;

namespace RopeSnake.Mother3.Text
{
    public class Mother3TextReader
    {
        public Stream BaseStream => _streamReader.BaseStream;
        internal StreamTokenReader _streamReader;
        internal StringTokenWriter _stringWriter;

        internal Mother3TextReader(StreamTokenReader streamReader, StringTokenWriter stringWriter)
        {
            _streamReader = streamReader;
            _stringWriter = stringWriter;
        }

        public static Mother3TextReader Create(Rom rom, bool isCompressed, bool isEncoded)
        {
            var type = rom.Type;

            if (type.Game != "Mother 3")
                throw new NotSupportedException(type.Game);

            StreamTokenReader reader;
            var charMap = Mother3Config.CreateCharacterMap(type);
            var codes = Mother3Config.Configs[type].ControlCodes;

            switch (type.Version)
            {
                case "jp":
                    reader = new StreamTokenReader(rom.ToStream(), charMap, codes);
                    break;

                case "en-v10":
                    reader = new EnglishStreamTokenReader(rom.ToStream(),
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

        public string ReadString() => ReadString(-1);

        public string ReadString(int bytesToRead)
        {
            _streamReader.Reset();
            _stringWriter.Reset();
            _stringWriter.Output.Clear();

            long oldPosition = BaseStream.Position;

            while ((bytesToRead == -1) || (BaseStream.Position - oldPosition < bytesToRead))
            {
                var token = _streamReader.Read();

                if (token.IsTerminator)
                    break;

                _stringWriter.Write(token);
            }

            if ((bytesToRead >= 0) && BaseStream.Position != oldPosition + bytesToRead)
                BaseStream.Position = oldPosition + bytesToRead;

            return _stringWriter.Output.ToString();
        }
    }
}
