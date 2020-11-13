namespace Tiny.Formats.Json
{
    public enum JsonTokenType
    {
        PropertyQuoted,
        WordQuoted,
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        ValueSeparator,
        Property,
        Word,
    }
}
