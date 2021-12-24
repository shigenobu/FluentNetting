using System;
using System.Collections.Generic;
using FluentNest.Formatters;
using MessagePack;
using MessagePack.Formatters;

namespace FluentNest
{
    public class FnEventModeResolver : IFormatterResolver
    {
        public static readonly IFormatterResolver Instance = new FnEventModeResolver();

        private FnEventModeResolver()
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
                Formatter = (IMessagePackFormatter<T>) FnEventModeResolverGetFormatterHelper.GetFormatter(typeof(T));
            }
        }
    }

    internal static class FnEventModeResolverGetFormatterHelper
    {
        static readonly Dictionary<Type, object> formatterMap = new Dictionary<Type, object>()
        {
            { typeof(BaseFnEventMode), new FnEventModeFormatter() },
            { typeof(FnMessageMode), new FnMessageModeFormatter() },
            { typeof(FnForwardMode), new FnForwardModeFormatter() },
            { typeof(FnPackedForwardMode), new FnPackedForwardModeFormatter() },
            { typeof(FnCompressedPackedForwardMode), new FnCompressedPackedForwardFormatter() }
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