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
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.PropertyQuoted, @"""((?:[^""\\]|\\.)*)"" *:"),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.WordQuoted, @"""((?:[^""\\]|\\.)*)"""),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ObjectStart, "{"),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ObjectEnd, "}"),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ArrayStart, "\\["),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ArrayEnd, "]"),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.ValueSeparator, ","),

            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.Property, "([^\\s:,{}\\[\\]]*) *:"),
            new RegexTokenizer<JsonTokenType>.Definition(JsonTokenType.Word, "[^\\s:,{}\\[\\]]+"),
        };

        protected override Tokenizer<JsonTokenType> Tokenizer { get; } = new RegexTokenizer<JsonTokenType>(definitions, null);
        protected override TokenParser<JsonTokenType> TokenParser { get; } = new JsonTokenParser();

        public override void Write(TextWriter writer, TinyToken value)
            => throw new NotImplementedException();
    }
}
