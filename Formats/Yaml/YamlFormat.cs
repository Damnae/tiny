using System.Collections.Generic;

namespace Tiny.Formats.Yaml
{
    public class YamlFormat : Format<YamlTokenType>
    {
        private static readonly List<RegexTokenizer<YamlTokenType>.Definition> definitions = new List<RegexTokenizer<YamlTokenType>.Definition>()
        {
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Indent, "^(  )+", 1, captureGroup: 0),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.PropertyQuoted, @"""((?:[^""\\]|\\.)*)"" *:", 4),
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.WordQuoted, @"""((?:[^""\\]|\\.)*)""", 5),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.ArrayIndicator, "- ", 10),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Property, "([^\\s:-][^\\s:]*) *:", 20),
            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.Word, "[^\\s:]+", 21),

            new RegexTokenizer<YamlTokenType>.Definition(YamlTokenType.EndLine, "\n", 100),
        };

        protected override Tokenizer<YamlTokenType> Tokenizer { get; } = new RegexTokenizer<YamlTokenType>(definitions, YamlTokenType.EndLine);
        protected override TokenParser<YamlTokenType> TokenParser { get; } = new YamlTokenParser();
    }
}
