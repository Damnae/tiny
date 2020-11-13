using System.Collections.Generic;
using System.IO;
using System.Text;

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

    public interface Format
    {
        TinyToken Read(string path);
        TinyToken ReadString(string data);
        TinyToken Read(TextReader reader);
        void Write(string path, TinyToken value);
        void Write(TextWriter writer, TinyToken value);
    }

    public abstract class Format<TokenType> : Format
    {
        protected abstract Tokenizer<TokenType> Tokenizer { get; }
        protected abstract TokenParser<TokenType> TokenParser { get; }

        public TinyToken Read(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return Read(reader);
        }

        public TinyToken ReadString(string data)
        {
            using (var reader = new StringReader(data))
                return Read(reader);
        }

        public TinyToken Read(TextReader reader)
        {
            var tokens = Tokenizer.Tokenize(reader);
            return TokenParser.Parse(tokens);
        }

        public void Write(string path, TinyToken value)
        {
            using (var stream = new FileStream(path, FileMode.Create))
            using (var writer = new StreamWriter(stream, Encoding.UTF8) { NewLine = "\n", })
                Write(writer, value);
        }

        public abstract void Write(TextWriter writer, TinyToken value);
    }
}
