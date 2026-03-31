using System;
using System.Diagnostics;

namespace Ofcas.Lk.Api.Client.Demo.Mvvm
{
    public static class Throw
    {
        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given value is null
        /// </summary>
        [DebuggerStepThrough]
        public static void IfNull<T>(T value, string parameterName)
        {
            Throw<ArgumentNullException>.If(value == null, parameterName);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the given condition is true
        /// </summary>
        [DebuggerStepThrough]
        public static void If(bool condition, string message)
        {
            Throw<ArgumentException>.If(condition, message);
        }
    }

    public static class Throw<TException> where TException : Exception
    {
        /// <summary>
        /// Throws an exception of type <see cref="TException"/> if the condition is true
        /// </summary>
        [DebuggerStepThrough]
        public static void If(bool condition, string message)
        {
            if (condition)
                throw Create(message);
        }

        [DebuggerStepThrough]
        private static TException Create(string message)
        {
            return (TException)Activator.CreateInstance(typeof(TException), message);
        }
    }
}