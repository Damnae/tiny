using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tiny.Formats
{
    public class RegexTokenizer<TokenType> : Tokenizer<TokenType>
    {
        private readonly List<Definition> definitions;
        private readonly TokenType endToken;

        public RegexTokenizer(List<Definition> definitions, TokenType endToken)
        {
            this.definitions = definitions;
            this.endToken = endToken;
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
                .SelectMany(d => d.FindMatches(content));

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

            yield return new Token<TokenType>(endToken);
        }

        public class Definition
        {
            private readonly Regex regex;
            private readonly TokenType matchType;
            private readonly int priority;
            private readonly int captureGroup;

            public Definition(TokenType matchType, string regexPattern, int priority, int captureGroup = 1)
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                this.matchType = matchType;
                this.priority = priority;
                this.captureGroup = captureGroup;
            }

            public IEnumerable<Match> FindMatches(string input)
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

            public override string ToString() => $"regex:{regex}, matchType:{matchType}, priority:{priority}, captureGroup:{captureGroup}";

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
