using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Tiny.Formats.Yaml
{
    public class YamlFormat : Format<YamlTokenType>
    {
        public const string BooleanTrue = "Yes";
        public const string BooleanFalse = "No";

        private static readonly List<RegexTokenizer<YamlTokenType>.Definition> definitions = new List<RegexTokenizer<YamlTokenType>.Definition>()
        {
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Indent, "^(  )+", captureGroup: 0),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.PropertyQuoted, @"""((?:[^""\\]|\\.)*)"" *:"),
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.WordQuoted, @"""((?:[^""\\]|\\.)*)"""),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.ArrayIndicator, "- "),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Property, "([^\\s:-][^\\s:]*) *:"),
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Word, "[^\\s:]+"),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.EndLine, "\n"),
        };

        protected override Tokenizer<YamlTokenType> Tokenizer { get; } = new RegexTokenizer<YamlTokenType>(definitions, YamlTokenType.EndLine);
        protected override TokenParser<YamlTokenType> TokenParser { get; } = new YamlTokenParser();

        public override void Write(TextWriter writer, TinyToken value)
            => write(writer, value, null, 0);

        private void write(TextWriter writer, TinyToken token, TinyToken parent, int indentLevel)
        {
            switch (token.Type)
            {
                case TinyTokenType.Object:
                    writeObject(writer, (TinyObject)token, parent, indentLevel);
                    break;
                case TinyTokenType.Array:
                    writeArray(writer, (TinyArray)token, parent, indentLevel);
                    break;
                default:
                    writeValue(writer, (TinyValue)token, parent, indentLevel);
                    break;
            }
        }

        private void writeObject(TextWriter writer, TinyObject obj, TinyToken parent, int indentLevel)
        {
            var parentIsArray = parent != null && parent.Type == TinyTokenType.Array;

            var first = true;
            foreach (var property in obj)
            {
                if (!first || !parentIsArray)
                    writeIndent(writer, indentLevel);

                var key = property.Key;
                if (key.Contains(" ") || key.Contains(":") || key.StartsWith("-"))
                    key = "\"" + YamlUtil.EscapeString(key) + "\"";

                var value = property.Value;
                if (value.IsEmpty)
                    writer.WriteLine(key + ":");
                else if (value.IsInline)
                {
                    writer.Write(key + ": ");
                    write(writer, value, obj, 0);
                }
                else
                {
                    writer.WriteLine(key + ":");
                    write(writer, value, obj, indentLevel + 1);
                }
                first = false;
            }
        }

        private void writeArray(TextWriter writer, TinyArray array, TinyToken parent, int indentLevel)
        {
            var parentIsArray = parent != null && parent.Type == TinyTokenType.Array;

            var first = true;
            foreach (var token in array)
            {
                if (!first || !parentIsArray)
                    writeIndent(writer, indentLevel);

                if (token.IsEmpty)
                    writer.WriteLine("- ");
                else if (token.IsInline)
                {
                    writer.Write("- ");
                    write(writer, token, array, 0);
                }
                else
                {
                    writer.Write("- ");
                    write(writer, token, array, indentLevel + 1);
                }
                first = false;
            }
        }

        private void writeValue(TextWriter writer, TinyValue valueToken, TinyToken parent, int indentLevel)
        {
            if (indentLevel != 0)
                throw new InvalidOperationException();

            var type = valueToken.Type;
            var value = valueToken.Value<object>();

            switch (type)
            {
                case TinyTokenType.Null:
                    writer.WriteLine();
                    break;
                case TinyTokenType.String:
                    writer.WriteLine("\"" + YamlUtil.EscapeString((string)value) + "\"");
                    break;
                case TinyTokenType.Integer:
                    writer.WriteLine(value?.ToString());
                    break;
                case TinyTokenType.Float:
                    if (value is float floatFloat)
                        writer.WriteLine(floatFloat.ToString(CultureInfo.InvariantCulture));
                    else if (value is double floatDouble)
                        writer.WriteLine(floatDouble.ToString(CultureInfo.InvariantCulture));
                    else if (value is decimal floatDecimal)
                        writer.WriteLine(floatDecimal.ToString(CultureInfo.InvariantCulture));
                    else if (value is string floatString)
                        writer.WriteLine(floatString);
                    else throw new InvalidDataException(value?.ToString());
                    break;
                case TinyTokenType.Boolean:
                    writer.WriteLine(((bool)value) ? BooleanTrue : BooleanFalse);
                    break;
                case TinyTokenType.Array:
                case TinyTokenType.Object:
                case TinyTokenType.Invalid:
                    // Should never happen :)
                    throw new InvalidDataException(type.ToString());
                default:
                    throw new NotImplementedException(type.ToString());
            }
        }

        private static void writeIndent(TextWriter writer, int indentLevel)
        {
            if (indentLevel <= 0)
                return;

            writer.Write(new string(' ', indentLevel * 2));
        }
    }
}
