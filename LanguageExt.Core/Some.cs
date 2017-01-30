﻿using LanguageExt.ClassInstances;
using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using static LanguageExt.Prelude;

namespace LanguageExt
{
    namespace SomeHelp
    {
        /// <summary>
        /// Extension method for Some T to help with the lack of covariance of generic
        /// parameters in structs (and therefore Some T)
        /// </summary>
        public static class SomeExt
        {
            /// <summary>
            /// Convert value to Some T.  Helps with the lack of covariance of generic
            /// parameters in structs (and therefore Some T)
            /// </summary>
            /// <typeparam name="T">Value type</typeparam>
            /// <param name="value">Value</param>
            /// <returns>Value wrapped in a Some T</returns>
            public static Some<T> ToSome<T>(this T value)
            {
                return new Some<T>(value);
            }
        }
    }

    // TODO: Restore when the type-class work is complete [TypeConverter(typeof(OptionalTypeConverter))]
    [Serializable]
    public struct Some<A> : 
        ISerializable, 
        IOptional
    {
        readonly A value;
        readonly bool initialised;

        public Some(A value)
        {
            if (isnull(value))
            {
                throw new ValueIsNullException("Value is null when expecting Some(x)");
            }
            this.value = value;
            initialised = true;
        }

        /// <summary>
        /// Serialisation support
        /// </summary>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IsSome", initialised, typeof(bool));
            info.AddValue("Value", initialised ? value : default(A), typeof(A));
        }

        /// <summary>
        /// Deserialisation constructor
        /// </summary>
        public Some(SerializationInfo info, StreamingContext context)
        {
            initialised = (bool)info.GetValue("IsSome", typeof(bool));
            value = initialised
                ? (A)info.GetValue("Value", typeof(A))
                : default(A);
        }

        [Pure]
        public A Value => 
            CheckInitialised(value);

        [Pure]
        private U CheckInitialised<U>(U value) =>
            initialised
                ? value
                : raise<U>( new SomeNotInitialisedException(typeof(A)) );

        [Pure]
        public static implicit operator Option<A>(Some<A> value) =>
            default(MOption<A>).Return(value.Value);

        [Pure]
        public static implicit operator Some<A>(A value) => 
            new Some<A>(value);

        [Pure]
        public static implicit operator A(Some<A> value) => 
            value.Value;

        [Pure]
        public override string ToString() =>
            Value.ToString();

        [Pure]
        public override int GetHashCode() =>
            Value.GetHashCode();

        [Pure]
        public override bool Equals(object obj) =>
            Value.Equals(obj);

        [Pure]
        public bool IsSome =>
            initialised;

        [Pure]
        public bool IsNone =>
            !initialised;

        [Pure]
        public R MatchUntyped<R>(Func<object, R> Some, Func<R> None) =>
            IsSome
                ? Some(value)
                : None();

        [Pure]
        public Type GetUnderlyingType() =>
            typeof(A);
    }

    public static class Some
    {
        [Pure]
        public static Some<T> Create<T>(T x) =>
            new Some<T>(x);
    }
}
