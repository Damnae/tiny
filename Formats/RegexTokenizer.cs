using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tiny.Formats
{
    public class RegexTokenizer<TokenType> : Tokenizer<TokenType>
        where TokenType : struct
    {
        private readonly List<Definition> definitions;
        private readonly TokenType? endLineToken;

        public RegexTokenizer(List<Definition> definitions, TokenType? endLineToken)
        {
            this.definitions = definitions;
            this.endLineToken = endLineToken;
        }

        public IEnumerable<Token<TokenType>> Tokenize(TextReader reader)
        {
            string line;
            int lineNumber = 1;
            while ((line = reader.ReadLine()) != null)
            {
                foreach (var token in Tokenize(line))
                {
                    token.LineNumber = lineNumber;
                    yield return token;
                }

                lineNumber++;
            }
        }

        public IEnumerable<Token<TokenType>> Tokenize(string content)
        {
            var matches = definitions
                .SelectMany((d, i) => d.FindMatches(content, i));

            var byStartGroups = matches
                .GroupBy(m => m.StartIndex)
                .OrderBy(g => g.Key);

            Definition.Match previousMatch = null;
            foreach (var byStartGroup in byStartGroups)
            {
                var bestMatch = byStartGroup
                    .OrderBy(m => m.Priority)
                    .First();

                if (previousMatch != null && bestMatch.StartIndex < previousMatch.EndIndex)
                    continue;

                yield return new Token<TokenType>(bestMatch.Type, bestMatch.Value)
                {
                    CharNumber = bestMatch.StartIndex,
                };
                previousMatch = bestMatch;
            }

            if (endLineToken.HasValue)
                yield return new Token<TokenType>(endLineToken.Value);
        }

        public class Definition
        {
            private readonly Regex regex;
            private readonly TokenType matchType;
            private readonly int captureGroup;

            public Definition(TokenType matchType, string regexPattern, int captureGroup = 1)
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                this.matchType = matchType;
                this.captureGroup = captureGroup;
            }

            public IEnumerable<Match> FindMatches(string input, int priority)
            {
                var matches = regex.Matches(input);
                foreach (System.Text.RegularExpressions.Match match in matches)
                    yield return new Match()
                    {
                        StartIndex = match.Index,
                        EndIndex = match.Index + match.Length,
                        Priority = priority,
                        Type = matchType,
                        Value = match.Groups.Count > captureGroup ?
                            match.Groups[captureGroup].Value :
                            match.Value,
                    };
            }

            public override string ToString() => $"regex:{regex}, matchType:{matchType}, captureGroup:{captureGroup}";

            public class Match
            {
                public int StartIndex;
                public int EndIndex;
                public int Priority;
                public TokenType Type;
                public string Value;

                public override string ToString() => $"{Type} <{Value}> from {StartIndex} to {EndIndex}, priority:{Priority}";
            }
        }
    }
}
