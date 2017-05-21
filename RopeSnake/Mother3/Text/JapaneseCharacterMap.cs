using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForwardLookup = System.Collections.Generic.Dictionary<short, char>;
using ReverseLookup = System.Collections.Generic.Dictionary<char, short>;

namespace RopeSnake.Mother3.Text
{
    public sealed class JapaneseCharacterMap : ICharacterMap
    {
        internal Dictionary<CharacterContext, ForwardLookup> _forwardLookups;
        internal Dictionary<CharacterContext, ReverseLookup> _reverseLookups;

        internal JapaneseCharacterMap(ForwardLookup normalLookup, ForwardLookup saturnLookup)
        {
            _forwardLookups = new Dictionary<CharacterContext, ForwardLookup>();
            _reverseLookups = new Dictionary<CharacterContext, ReverseLookup>();

            (var forward, var reverse) = ConfigureLookups(normalLookup);
            _forwardLookups[CharacterContext.Normal] = forward;
            _reverseLookups[CharacterContext.Normal] = reverse;

            (forward, reverse) = ConfigureLookups(saturnLookup);
            _forwardLookups[CharacterContext.Saturn] = forward;
            _reverseLookups[CharacterContext.Saturn] = reverse;
        }

        internal (ForwardLookup forward, ReverseLookup reverse) ConfigureLookups(ForwardLookup lookup)
        {
            var forward = new ForwardLookup();
            var reverse = new ReverseLookup();

            foreach (var kv in lookup)
            {
                forward.Add(kv.Key, kv.Value);
                if (!reverse.ContainsKey(kv.Value))
                    reverse.Add(kv.Value, kv.Key);
            }

            return (forward, reverse);
        }

        public char Decode(short value, CharacterContext context)
        {
            if (_forwardLookups[context].TryGetValue(value, out char decoded))
                return decoded;

            return '?';
        }

        public short Encode(char str, CharacterContext context)
            => _reverseLookups[context][str];
    }
}
