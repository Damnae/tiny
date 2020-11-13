using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Tiny.Formats.Yaml
{
    public partial class YamlTokenParser : TokenParser<YamlTokenType>
    {
        public TinyToken Parse(IEnumerable<Token<YamlTokenType>> tokens)
        {
            TinyToken result = null;
            var context = new ParseContext<YamlTokenType>(tokens, c => new AnyParser(c), r => result = r);

            Token<YamlTokenType> previousToken = null;
            while (context.CurrentToken != null)
            {
                if (context.CurrentToken != previousToken)
                {
                    //Debug.Print($"{context.CurrentToken}");
                    previousToken = context.CurrentToken;
                }

                switch (context.CurrentToken.Type)
                {
                    case YamlTokenType.Indent:
                        context.Indent(context.CurrentToken.Value.Length / 2);
                        context.ConsumeToken();
                        continue;

                    case YamlTokenType.EndLine:
                        context.NewLine();
                        context.ConsumeToken();
                        continue;
                }
                //Debug.Print($"  - {context.Parser.GetType().Name} ({context.ParserCount})");
                context.Parser.Parse(context);
            }

            while (context.Parser != null)
            {
                context.Parser.End();
                context.PopParser();
            }

            return result;
        }

        private abstract class MultilineParser : Parser<YamlTokenType>
        {
            private int? indent = null;

            protected abstract int ResultCount { get; }

            public MultilineParser(Action<TinyToken> callback, int virtualIndent) : base(callback, virtualIndent)
            {
            }

            protected bool CheckIndent(ParseContext<YamlTokenType> context)
            {
                indent = indent ?? context.IndentLevel + VirtualIndent;
                var lineIndent = ResultCount == 0 ? context.IndentLevel + VirtualIndent : context.IndentLevel;
                if (lineIndent != indent)
                {
                    if (lineIndent > indent)
                        throw new InvalidDataException($"Unexpected indent: {lineIndent}, expected: {indent}, token: {context.CurrentToken}");

                    context.PopParser();
                    return true;
                }
                return false;
            }
        }

        private class ObjectParser : MultilineParser
        {
            private readonly TinyObject result = new TinyObject();

            protected override int ResultCount => result.Count;

            public ObjectParser(Action<TinyToken> callback, int virtualIndent = 0) : base(callback, virtualIndent)
            {
                callback(result);
            }

            public override void Parse(ParseContext<YamlTokenType> context)
            {
                if (CheckIndent(context))
                    return;

                switch (context.LookaheadToken.Type)
                {
                    case YamlTokenType.ArrayIndicator:
                    case YamlTokenType.Property:
                    case YamlTokenType.PropertyQuoted:
                        throw new InvalidDataException("Unexpected token: " + context.LookaheadToken + ", after: " + context.CurrentToken);
                }

                switch (context.CurrentToken.Type)
                {
                    case YamlTokenType.Property:
                    case YamlTokenType.PropertyQuoted:

                        var key = context.CurrentToken.Value;
                        if (context.CurrentToken.Type == YamlTokenType.PropertyQuoted)
                            key = TinyUtil.UnescapeString(key);

                        switch (context.LookaheadToken.Type)
                        {
                            case YamlTokenType.Word:
                            case YamlTokenType.WordQuoted:
                                context.PushParser(new ValueParser(r => result.Add(key, r)));
                                break;
                            case YamlTokenType.EndLine:
                                context.PushParser(new EmptyProperyParser(r => result.Add(key, r), context.IndentLevel + 1));
                                break;
                            default:
                                throw new InvalidDataException("Unexpected token: " + context.LookaheadToken + ", after: " + context.CurrentToken);
                        }
                        context.ConsumeToken();
                        return;
                }
                throw new InvalidDataException("Unexpected token: " + context.CurrentToken);
            }

            public override void End()
            {
            }
        }

        private class ArrayParser : MultilineParser
        {
            private readonly TinyArray result = new TinyArray();

            protected override int ResultCount => result.Count;

            public ArrayParser(Action<TinyToken> callback, int virtualIndent = 0) : base(callback, virtualIndent)
            {
                callback(result);
            }

            public override void Parse(ParseContext<YamlTokenType> context)
            {
                if (CheckIndent(context))
                    return;

                switch (context.CurrentToken.Type)
                {
                    case YamlTokenType.ArrayIndicator:
                        context.PushParser(new AnyParser(r => result.Add(r), result.Count == 0 ? VirtualIndent + 1 : 1));
                        context.ConsumeToken();
                        return;
                }

                throw new InvalidDataException("Unexpected token: " + context.CurrentToken);
            }

            public override void End()
            {
            }
        }

        private class ValueParser : Parser<YamlTokenType>
        {
            private static readonly Regex floatRegex = new Regex("^[-+]?[0-9]*\\.[0-9]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            private static readonly Regex integerRegex = new Regex("^[-+]?\\d+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            private static readonly Regex boolRegex = new Regex($"^{TinyValue.BooleanTrue}|{TinyValue.BooleanFalse}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            public ValueParser(Action<TinyToken> callback) : base(callback, 0)
            {
            }

            public override void Parse(ParseContext<YamlTokenType> context)
            {
                switch (context.LookaheadToken.Type)
                {
                    case YamlTokenType.EndLine:
                        break;
                    default:
                        throw new InvalidDataException("Unexpected token: " + context.LookaheadToken + ", after: " + context.CurrentToken);
                }

                switch (context.CurrentToken.Type)
                {
                    case YamlTokenType.Word:
                        {
                            var value = context.CurrentToken.Value;
                            Match match;
                            if ((match = floatRegex.Match(value)).Success)
                                Callback(new TinyValue(value, TinyTokenType.Float));
                            else if ((match = integerRegex.Match(value)).Success)
                                Callback(new TinyValue(value, TinyTokenType.Integer));
                            else if ((match = boolRegex.Match(value)).Success)
                                Callback(new TinyValue(value == TinyValue.BooleanTrue));
                            else
                                Callback(new TinyValue(value));
                            context.ConsumeToken();
                            context.PopParser();
                        }
                        return;

                    case YamlTokenType.WordQuoted:
                        {
                            var value = TinyUtil.UnescapeString(context.CurrentToken.Value);
                            Callback(new TinyValue(value));
                            context.ConsumeToken();
                            context.PopParser();
                        }
                        return;
                }

                throw new InvalidDataException("Unexpected token: " + context.CurrentToken);
            }

            public override void End()
            {
            }
        }

        private class AnyParser : Parser<YamlTokenType>
        {
            public AnyParser(Action<TinyToken> callback, int virtualIndent = 0) : base(callback, virtualIndent)
            {
            }

            public override void Parse(ParseContext<YamlTokenType> context)
            {
                switch (context.CurrentToken.Type)
                {
                    case YamlTokenType.Property:
                    case YamlTokenType.PropertyQuoted:
                        context.ReplaceParser(new ObjectParser(Callback, VirtualIndent));
                        return;
                    case YamlTokenType.ArrayIndicator:
                        context.ReplaceParser(new ArrayParser(Callback, VirtualIndent));
                        return;
                    case YamlTokenType.Word:
                    case YamlTokenType.WordQuoted:
                        context.ReplaceParser(new ValueParser(Callback));
                        return;
                }
                throw new InvalidDataException("Unexpected token: " + context.CurrentToken);
            }

            public override void End()
            {
            }
        }

        private class EmptyProperyParser : Parser<YamlTokenType>
        {
            private readonly int expectedIndent;

            public EmptyProperyParser(Action<TinyToken> callback, int expectedIndent, int virtualIndent = 0) : base(callback, virtualIndent)
            {
                this.expectedIndent = expectedIndent;
            }

            public override void Parse(ParseContext<YamlTokenType> context)
            {
                if (context.IndentLevel < expectedIndent)
                {
                    Callback(new TinyValue(null, TinyTokenType.Null));
                    context.PopParser();
                    return;
                }

                if (context.IndentLevel == expectedIndent)
                {
                    context.ReplaceParser(new AnyParser(Callback));
                    return;
                }

                throw new InvalidDataException($"Unexpected indent: {context.IndentLevel}, expected: {expectedIndent}, token: {context.CurrentToken}");
            }

            public override void End()
            {
                Callback(new TinyValue(null, TinyTokenType.Null));
            }
        }
    }
}