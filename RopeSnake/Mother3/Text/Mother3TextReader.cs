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

        public static Mother3TextReader Create(Rom rom)
        {
            var reader = new StreamTokenReader(rom.ToStream(),
                Mother3Config.CreateCharacterMap(rom.Type), Mother3Config.Configs[rom.Type].ControlCodes);

            var writer = new StringTokenWriter(new StringBuilder());

            return new Mother3TextReader(reader, writer);
        }

        public string ReadString() => ReadString(-1);

        public string ReadString(int maxChars)
        {
            _streamReader.Reset();
            _stringWriter.Reset();
            _stringWriter.Output.Clear();

            for (int i = 0; (maxChars == -1) || (i < maxChars); i++)
            {
                var token = _streamReader.Read();

                if (token.IsTerminator)
                    break;

                _stringWriter.Write(token);
            }

            return _stringWriter.Output.ToString();
        }
    }
}
