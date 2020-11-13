using System;

namespace Tiny.Formats
{
    public abstract class Parser<TokenType>
    {
        protected readonly Action<TinyToken> Callback;
        protected readonly int VirtualIndent;

        public Parser(Action<TinyToken> callback, int virtualIndent)
        {
            Callback = callback;
            VirtualIndent = virtualIndent;
        }

        public abstract void Parse(ParseContext<TokenType> context);
        public abstract void End();
    }
}