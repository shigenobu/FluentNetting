using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest
{
    public class FnEventTimeResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new FnEventTimeResolver();

        private FnEventTimeResolver()
        {
        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            public static readonly IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                Formatter = (IMessagePackFormatter<T>) FnEventTimeResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class FnEventTimeResolverGetFormatterHelper
    {
        static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
        {
            { typeof(object), new FnEventTimeFormatter() }
        };

        internal static object GetFormatter(Type t)
        {
            object formatter;
            if (formatterMap.TryGetValue(t, out formatter))
            {
                return formatter;
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                return Activator.CreateInstance(typeof(ValueTupleFormatter<,>).MakeGenericType(t.GenericTypeArguments));
            }

            return null;
        }
    }
}