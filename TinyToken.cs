using System;
using System.Collections;
using System.IO;
using System.Text;
using Tiny.Formats;
using Tiny.Formats.Json;
using Tiny.Formats.Yaml;

namespace Tiny
{
    public abstract class TinyToken
    {
        public abstract bool IsInline { get; }
        public abstract bool IsEmpty { get; }
        public abstract TinyTokenType Type { get; }

        public static implicit operator TinyToken(string value) => new TinyValue(value);

        public static implicit operator TinyToken(bool value) => new TinyValue(value);
        public static implicit operator TinyToken(bool? value) => new TinyValue(value);

        public static implicit operator TinyToken(sbyte value) => new TinyValue(value);
        public static implicit operator TinyToken(sbyte? value) => new TinyValue(value);
        public static implicit operator TinyToken(byte value) => new TinyValue(value);
        public static implicit operator TinyToken(byte? value) => new TinyValue(value);
        public static implicit operator TinyToken(short value) => new TinyValue(value);
        public static implicit operator TinyToken(short? value) => new TinyValue(value);
        public static implicit operator TinyToken(ushort value) => new TinyValue(value);
        public static implicit operator TinyToken(ushort? value) => new TinyValue(value);
        public static implicit operator TinyToken(int value) => new TinyValue(value);
        public static implicit operator TinyToken(int? value) => new TinyValue(value);
        public static implicit operator TinyToken(uint value) => new TinyValue(value);
        public static implicit operator TinyToken(uint? value) => new TinyValue(value);
        public static implicit operator TinyToken(long? value) => new TinyValue(value);
        public static implicit operator TinyToken(long value) => new TinyValue(value);
        public static implicit operator TinyToken(ulong value) => new TinyValue(value);
        public static implicit operator TinyToken(ulong? value) => new TinyValue(value);

        public static implicit operator TinyToken(float value) => new TinyValue(value);
        public static implicit operator TinyToken(float? value) => new TinyValue(value);
        public static implicit operator TinyToken(double value) => new TinyValue(value);
        public static implicit operator TinyToken(double? value) => new TinyValue(value);
        public static implicit operator TinyToken(decimal value) => new TinyValue(value);
        public static implicit operator TinyToken(decimal? value) => new TinyValue(value);

        public abstract T Value<T>(object key);
        public T Value<T>() => Value<T>(null);

        public static TinyToken ToToken(object value)
        {
            if (value is TinyToken token)
                return token;

            if (TinyValue.FindValueType(value) == TinyTokenType.Invalid)
            {
                if (value is IDictionary dictionary)
                {
                    var o = new TinyObject();
                    foreach (var key in dictionary.Keys)
                        o.Add((string)key, ToToken(dictionary[key]));
                    return o;
                }

                if (value is IEnumerable enumerable)
                    return new TinyArray(enumerable);
            }

            return new TinyValue(value);
        }

        public static TinyToken Read(Stream stream, Format format)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return format.Read(reader);
        }

        public static TinyToken Read(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open))
                return Read(stream, GetFormat(path));
        }

        public static TinyToken ReadString(string data, Format format)
        {
            using (var reader = new StringReader(data))
                return format.Read(reader);
        }

        public static TinyToken ReadString<F>(string data)
            where F : Format, new()
            => ReadString(data, new F());

        public void Write(Stream stream, Format format)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8) { NewLine = "\n", })
                format.Write(writer, this);
        }

        public void Write(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create))
                Write(stream, GetFormat(path));
        }

        public static Format GetFormat(string path)
        {
            var extension = Path.GetExtension(path);
            switch (Path.GetExtension(path))
            {
                case ".yaml": return new YamlFormat();
                case ".json": return new JsonFormat();
            }
            throw new NotImplementedException($"No format matches extension '{extension}'.");
        }
    }
}
