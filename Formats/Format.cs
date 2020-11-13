using System.Collections.Generic;
using System.IO;

namespace Tiny.Formats
{
    public interface Tokenizer<TokenType>
    {
        IEnumerable<Token<TokenType>> Tokenize(TextReader reader);
    }

    public interface TokenParser<TokenType>
    {
        TinyToken Parse(IEnumerable<Token<TokenType>> tokens);
    }

    public abstract class Format<TokenType>
    {
        protected abstract Tokenizer<TokenType> Tokenizer { get; }
        protected abstract TokenParser<TokenType> TokenParser { get; }

        public TinyToken Read(TextReader reader)
        {
            var tokens = Tokenizer.Tokenize(reader);
            return TokenParser.Parse(tokens);
        }
    }
}
