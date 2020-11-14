using System;
using System.Collections.Generic;

namespace Tiny.Formats
{
    public class ParseContext<TokenType>
    {
        private readonly IEnumerator<Token<TokenType>> tokenEnumerator;

        public Token<TokenType> CurrentToken;
        public Token<TokenType> LookaheadToken;

        private readonly Stack<Parser<TokenType>> parserStack = new Stack<Parser<TokenType>>();
        public Parser<TokenType> Parser => parserStack.Count > 0 ? parserStack.Peek() : null;
        public int ParserCount => parserStack.Count;

        public int IndentLevel { get; private set; }

        public ParseContext(IEnumerable<Token<TokenType>> tokens, Parser<TokenType> initialParser)
        {
            tokenEnumerator = tokens.GetEnumerator();
            initializeCurrentAndLookahead();

            parserStack.Push(initialParser);
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

        public void End()
        {
            while (Parser != null)
            {
                Parser.End();
                PopParser();
            }
        }

        private void initializeCurrentAndLookahead()
        {
            ConsumeToken();
            ConsumeToken();
        }
    }
}