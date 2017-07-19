using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForwardLookup = System.Collections.Generic.Dictionary<short, char>;

namespace RopeSnake.Mother3.Text
{
    public class EnglishCharacterMap : JapaneseCharacterMap
    {
        internal ForwardLookup _combinedForwardLookup = new ForwardLookup();

        internal EnglishCharacterMap(ForwardLookup normalLookup, ForwardLookup saturnLookup)
            : base(normalLookup, saturnLookup)
        {
            foreach (var kv in _forwardLookups.SelectMany(a => a.Value))
                _combinedForwardLookup.Add(kv.Key, kv.Value);
        }

        public override char Decode(short value, CharacterContext context)
            => _combinedForwardLookup[value];

        public override short Encode(char str, CharacterContext context)
        {
            if (_reverseLookups[CharacterContext.Saturn].TryGetValue(str, out short encoded))
                return encoded;

            return _reverseLookups[CharacterContext.Normal][str];
        }
    }
}
