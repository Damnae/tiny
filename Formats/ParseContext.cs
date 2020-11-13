using System;
using System.Collections.Generic;

namespace Tiny.Formats
{
    public class ParseContext<TokenType>
    {
        private readonly IEnumerator<Token<TokenType>> tokenEnumerator;

        public Token<TokenType> CurrentToken;
        public Token<TokenType> LookaheadToken;

        private readonly Stack<Parser<TokenType>> parserStack;
        public Parser<TokenType> Parser => parserStack.Count > 0 ? parserStack.Peek() : null;
        public int ParserCount => parserStack.Count;

        public int IndentLevel { get; private set; }

        public ParseContext(IEnumerable<Token<TokenType>> tokens, Func<Action<TinyToken>, Parser<TokenType>> createInitialParser, Action<TinyToken> callback)
        {
            tokenEnumerator = tokens.GetEnumerator();
            initializeCurrentAndLookahead();

            parserStack = new Stack<Parser<TokenType>>();
            parserStack.Push(createInitialParser(callback));
        }

        public void PopParser()
        {
            parserStack.Pop();
        }

        public void PushParser(Parser<TokenType> parser)
        {
            parserStack.Push(parser);
        }

        public void ReplaceParser(Parser<TokenType> parser)
        {
            parserStack.Pop();
            parserStack.Push(parser);
        }

        public void Indent(int level)
        {
            IndentLevel = level;
        }

        public void NewLine()
        {
            IndentLevel = 0;
        }

        public void ConsumeToken()
        {
            CurrentToken = LookaheadToken;
            LookaheadToken = tokenEnumerator.MoveNext() ? tokenEnumerator.Current : null;
        }

        private void initializeCurrentAndLookahead()
        {
            ConsumeToken();
            ConsumeToken();
        }
    }
}