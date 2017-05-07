using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RopeSnake.Core
{
    /// <summary>
    /// Represents a range over the integers.
    /// </summary>
    [JsonConverter(typeof(RangeConverter)), Serializable]
    public struct Range
    {
        /// <summary>
        /// Start location of this range. Inclusive.
        /// </summary>
        public readonly int Start;

        /// <summary>
        /// End location of this range. Inclusive.
        /// </summary>
        public readonly int End;

        public int Size { get { return End - Start + 1; } }

        private Range(int start, int end)
        {
            if (start < 0 || end < 0)
                throw new ArgumentException("Locations may not be negative");

            if (end < start)
                throw new ArgumentException($"{nameof(end)} must not be less than {nameof(start)}");

            Start = start;
            End = end;
        }

        public static Range StartEnd(int start, int end) => new Range(start, end);

        public static Range StartSize(int start, int size) => new Range(start, start + size - 1);

        public static Range Combine(Range first, Range second)
        {
            if (!first.CanCombineWith(second))
                throw new Exception("Cannot combine first range with second. Perhaps they don't overlap?"
                    + Environment.NewLine
                    + $"First: {first.ToString()}, second: {second.ToString()}");

            (Range lower, Range upper) = SortRanges(first, second);

            return StartEnd(lower.Start, Math.Max(lower.End, upper.End));
        }

        public Range CombineWith(Range other)
            => Combine(this, other);

        public bool CanCombineWith(Range other)
        {
            (Range lower, Range upper) = SortRanges(this, other);

            return upper.Start <= (lower.End + 1);
        }

        public static Range Parse(string rangeString)
        {
            // Format: [start,end]
            // Where start and end may be hex (prefixed with 0x), and there may be
            // whitespace in between any two tokens
            string trimmed = rangeString.Trim();

            // First and last chars must be []
            if (rangeString[0] != '[')
            {
                throw new Exception("Expected opening square bracket");
            }

            if (rangeString[rangeString.Length - 1] != ']')
            {
                throw new Exception("Expected closing square bracket");
            }

            // Get the insides of the brackets and split by comma
            string[] insideSplit = trimmed.Substring(1, trimmed.Length - 2)
                .Split(',')
                .Select(s => s.Trim())
                .ToArray();

            if (insideSplit.Length != 2)
            {
                throw new Exception("Expected exactly two comma-separated values inside square brackets");
            }

            int[] numbers = insideSplit.Select(s => ParseNumber(s)).ToArray();

            return StartEnd(numbers[0], numbers[1]);
        }

        private static int ParseNumber(string number)
        {
            bool hexMode = number.StartsWith("0x") || number.StartsWith("0X");
            int numberBase = hexMode ? 16 : 10;
            int numberStart = hexMode ? 2 : 0;

            return Convert.ToInt32(number.Substring(numberStart), numberBase);
        }

        /// <summary>
        /// Gets the aligned size of this range.
        /// </summary>
        /// <param name="align">alignment to use</param>
        /// <returns>aligned size</returns>
        public int GetAlignedSize(int align)
            => End - Start.Align(align) + 1;

        internal static (Range lower, Range upper) SortRanges(Range first, Range second)
        {
            Range lower;
            Range upper;

            if (first.Start <= second.Start)
            {
                lower = first;
                upper = second;
            }
            else
            {
                lower = second;
                upper = first;
            }

            return (lower, upper);
        }

        public override string ToString()
        {
            return $"[{Start}, {End}]";
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ End.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Range && this == (Range)obj;
        }

        public static bool operator ==(Range first, Range second)
            => first.Start == second.Start && first.End == second.End;

        public static bool operator !=(Range first, Range second)
            => !(first == second);
    }

    class RangeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Range);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            if (token.Type != JTokenType.String)
            {
                throw new JsonSerializationException("Expected string type");
            }

            string rangeString = token.Value<string>();
            return Range.Parse(rangeString);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType() != typeof(Range))
            {
                throw new InvalidOperationException("Value must be a Range");
            }

            Range range = (Range)value;
            string rangeString = $"[0x{range.Start:X}, 0x{range.End:X}]";
            writer.WriteValue(rangeString);
        }
    }
}
