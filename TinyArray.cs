using System;
using System.Collections;
using System.Collections.Generic;

namespace Tiny
{
    public class TinyArray : TinyToken, IList<TinyToken>
    {
        private readonly List<TinyToken> tokens = new List<TinyToken>();

        public override bool IsInline => false;
        public override bool IsEmpty => tokens.Count == 0;
        public override TinyTokenType Type => TinyTokenType.Array;

        public TinyToken this[int index]
        {
            get => tokens[index];
            set => tokens[index] = value;
        }

        public TinyArray()
        {
        }

        public TinyArray(IEnumerable values)
        {
            foreach (var value in values)
                Add(ToToken(value));
        }

        public int Count => tokens.Count;
        public bool IsReadOnly => false;

        public void Add(TinyToken item) => tokens.Add(item);
        public void Clear() => tokens.Clear();
        public bool Contains(TinyToken item) => tokens.Contains(item);
        public void CopyTo(TinyToken[] array, int arrayIndex) => tokens.CopyTo(array, arrayIndex);
        public int IndexOf(TinyToken item) => tokens.IndexOf(item);
        public void Insert(int index, TinyToken item) => tokens.Insert(index, item);
        public bool Remove(TinyToken item) => tokens.Remove(item);
        public void RemoveAt(int index) => tokens.RemoveAt(index);

        public IEnumerator<TinyToken> GetEnumerator() => tokens.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => tokens.GetEnumerator();

        public override T Value<T>(object key)
        {
            if (key == null)
                return (T)(object)this;

            if (key is int index)
                return this[index].Value<T>();

            throw new ArgumentException($"Key must be an integer, was {key}", "key");
        }

        public override string ToString() => string.Join(", ", tokens);
    }
}
