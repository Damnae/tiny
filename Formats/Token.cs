namespace Tiny.Formats
{
    public class Token<TokenType>
    {
        public TokenType Type;
        public string Value;
        public int LineNumber;

        public Token(TokenType type, string value = null)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"{Type} <{Value}> (line {LineNumber})";
    }
}
