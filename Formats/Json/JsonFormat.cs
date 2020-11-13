using System;
using System.Collections.Generic;
using System.IO;

namespace Tiny.Formats.Json
{
    public class JsonFormat : Format<JsonTokenType>
    {
        public const string BooleanTrue = "true";
        public const string BooleanFalse = "false";

        private static readonly List<RegexTokenizer<JsonTokenType>.Definition> definitions = new List<RegexTokenizer<JsonTokenType>.Definition>()
        {
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.PropertyQuoted, @"""((?:[^""\\]|\\.)*)"" *:", 4),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.WordQuoted, @"""((?:[^""\\]|\\.)*)""", 5),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ObjectStart, "{", 10),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ObjectEnd, "}", 11),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ArrayStart, "\\[", 12),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ArrayEnd, "]", 13),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ValueSeparator, ",", 20),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.Property, "([^\\s:,{}\\[\\]]*) *:", 30),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.Word, "[^\\s:,{}\\[\\]]+", 31),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.EndLine, "\n", 100),
        };

        protected override Tokenizer<JsonTokenType> Tokenizer { get; } = new RegexTokenizer<JsonTokenType>(definitions, JsonTokenType.EndLine);
        protected override TokenParser<JsonTokenType> TokenParser { get; } = new JsonTokenParser();

        public override void Write(TextWriter writer, TinyToken value)
            => throw new NotImplementedException();
    }
}
