using System;

namespace Monads.Maybe
{
    [Serializable]
    public sealed class Maybe<T> : IEquatable<Maybe<T>>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static readonly Maybe<T> Empty = new Maybe<T>(default(T), false);


        private Maybe(T item, bool hasValue)
        {
            _value = item;
            _hasValue = hasValue;
        }

        internal Maybe(T value)
            : this(value, true)
        {
            if (value == null) throw new ArgumentNullException("value");
        }

        /// <summary> 
        /// Gets the underlying value, if it is available
        /// </summary>
        public T Value
        {
            get
            {
                if (_hasValue == false)
                    throw new InvalidOperationException("Accessed Maybe<T>.Value when HasValue is false. Use Maybe<T>.GetValueOrDefault instead of Maybe<T>.Value");
                return _value;
            }
        }

        /// <summary> 
        /// Gets a value indicating whether this instance has value. 
        /// </summary>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Get the stored value, or the default value for it's type
        /// </summary>
        /// <returns></returns>
        public T GetValueOrDefault()
        {
            return _hasValue ? _value : default(T);
        }

        /// <summary>
        /// Get the stored value, or return the provided default value
        /// </summary>
        public T GetValueOrDefault(T @default)
        {
            return _hasValue ? _value : @default;
        }

        /// <summary>
        /// Get the stored value, or the provided default if the Maybe[T] is Empty
        /// </summary>
        public T GetValueOrDefault(Func<T> @default)
        {
            return _hasValue ? _value : @default();
        }

        /// <summary>
        /// Apply an action to the value, if present
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public Maybe<T> Apply(Action<T> action)
        {
            if (_hasValue)
                action(_value);
            return this;
        }

        /// <summary>
        /// Select a new value from the value of the Maybe, if it exists
        /// otherwise provides an instance of Maybe.Empty
        /// </summary>
        public Maybe<U> Select<U>(Func<T, U> selector)
        {
            if (_hasValue == false)
                return Maybe<U>.Empty;
            else
            {
                var selected = selector(_value);
                if (selected == null)
                    return Maybe<U>.Empty;
                else
                    return new Maybe<U>(selected);
            }
        }

        /// <summary>
        /// Determines whether the provided Maybe is equal to the current Maybe
        /// </summary>
        public bool Equals(Maybe<T> maybe)
        {
            if (ReferenceEquals(null, maybe)) return false;
            if (ReferenceEquals(this, maybe)) return true;

            if (_hasValue != maybe._hasValue) return false;
            if (_hasValue == false) return true;
            return _value.Equals(maybe._value);
        }

        /// <summary>
        /// Determines whether the provided object is equal to the current Maybe.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var maybe = obj as Maybe<T>;
            if (maybe == null) return false;
            return Equals(maybe);
        }

        public override int GetHashCode()
        {
            if (_hasValue)
            {
                // 41 is just an odd prime, the likelihood of encountering it is not high in comparison to
                // 0 for example. We just want a good seed value, and 41 is my choice :)
                return 41 * _value.GetHashCode();
            }
            else
            {
                return 0;
            }
        }

        public static bool operator ==(Maybe<T> left, Maybe<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Maybe<T> left, Maybe<T> right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Performs an implicit conversion from T to Maybe[T]
        /// </summary>
        public static implicit operator Maybe<T>(T item)
        {
            if (item == null)
                return Maybe<T>.Empty;
            else
                return new Maybe<T>(item);
        }

        /// <summary>
        /// Performs an explicit conversion from Maybe[T] to T
        /// </summary>
        public static explicit operator T(Maybe<T> item)
        {
            if (item == null) throw new ArgumentNullException("item");

            return item.HasValue ? item.Value : default(T);
        }

        /// <summary>
        /// Returns a string representing the Maybe's value
        /// </summary>
        public override string ToString()
        {
            if (_hasValue)
                return "<" + _value + ">";

            return "<Empty>";
        }
    }
}
