using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using O2Html.Converters;
using O2Html.Dom;

namespace O2Html;

public sealed class HtmlSerializer
{
    private static readonly Dictionary<Type, HtmlConverter?> _typeConverterCache = new();
    private static readonly Dictionary<Type, TypeCategory> _typeCategoryCache = new();
    private static readonly Dictionary<Type, PropertyInfo[]> _typePropertyCache = new();
    private static readonly Dictionary<Type, Type?> _collectionElementTypeCache = new();

    public HtmlSerializer(HtmlSerializerSettings? serializerSettings = null)
    {
        SerializerSettings = serializerSettings ?? new HtmlSerializerSettings();
        Converters = new List<HtmlConverter>();

        if (SerializerSettings.Converters?.Any() == true)
        {
            // Insert settings converters at the beginning so they take precedence
            foreach (var converter in SerializerSettings.Converters)
            {
                Converters.Add(converter);
            }
        }

        // Add default converters in this order, first converter in list
        // that can convert object takes precedence
        Converters.Add(new TwoDimensionalArrayHtmlConverter());
        Converters.Add(new DataSetHtmlConverter());
        Converters.Add(new DataTableHtmlConverter());
        Converters.Add(new XNodeHtmlConverter());
        Converters.Add(new XmlNodeHtmlConverter());
        Converters.Add(new DotNetTypeWithStringRepresentationHtmlConverter());
#if NETSTANDARD2_1 || NETCOREAPP3_0_OR_GREATER
        Converters.Add(new TupleHtmlConverter());
#endif
        Converters.Add(new CollectionHtmlConverter());
        Converters.Add(new ObjectHtmlConverter());
    }

    public HtmlSerializerSettings SerializerSettings { get; }

    public List<HtmlConverter> Converters { get; }

    public Node Serialize<T>(T? obj, Type type, SerializationScope? serializationScope = null)
    {
        var converter = GetConverter(type);
        if (converter == null)
            throw new HtmlSerializationException($"Could not find a {nameof(HtmlConverter)} for type: {type}");

        serializationScope = GetSerializationScope(type, obj, serializationScope);

        return converter.WriteHtml(obj, type, serializationScope, this);
    }

    public void SerializeWithinTableRow<T>(Element tr, T? obj, Type type, SerializationScope? serializationScope = null)
    {
        var converter = GetConverter(type);
        if (converter == null)
            throw new HtmlSerializationException($"Could not find a {nameof(HtmlConverter)} for type: {type}");

        serializationScope = GetSerializationScope(type, obj, serializationScope);

        converter.WriteHtmlWithinTableRow(tr, obj, type, serializationScope, this);
    }

    public TypeCategory GetTypeCategory(Type type)
    {
        if (_typeCategoryCache.TryGetValue(type, out var category))
            return category;

        if (IsDotNetTypeWithStringRepresentation(type))
            category = TypeCategory.DotNetTypeWithStringRepresentation;
        else if (IsCollectionType(type))
            category = TypeCategory.Collection;
        else
            category = TypeCategory.SingleObject;

        _typeCategoryCache.Add(type, category);
        return category;
    }

    public PropertyInfo[] GetReadableProperties(Type type)
    {
        if (_typePropertyCache.TryGetValue(type, out var propertyInfos))
            return propertyInfos;

        propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();

        _typePropertyCache.Add(type, propertyInfos);
        return propertyInfos;
    }

    public Type? GetCollectionElementType(Type collectionType)
    {
        if (_collectionElementTypeCache.TryGetValue(collectionType, out var elementType))
            return elementType;

        elementType = collectionType.GetCollectionElementType();

        _collectionElementTypeCache.Add(collectionType, elementType);

        return elementType;
    }

    private HtmlConverter? GetConverter(Type type)
    {
        if (_typeConverterCache.TryGetValue(type, out var match))
            return match;

        foreach (var converter in Converters)
        {
            if (!converter.CanConvert(this, type)) continue;
            _typeConverterCache.Add(type, converter);
            return converter;
        }

        _typeConverterCache.Add(type, null);
        return null;
    }

    private SerializationScope GetSerializationScope<T>(Type type, T? obj, SerializationScope? serializationScope)
    {
        if (serializationScope == null)
            serializationScope = new SerializationScope();
        else
        {
            bool createNewScope = obj != null && GetTypeCategory(type) == TypeCategory.SingleObject;

            if (createNewScope)
                serializationScope = new SerializationScope(serializationScope);
        }

        return serializationScope;
    }

    public static bool IsDotNetTypeWithStringRepresentation(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || typeof(IFormattable).IsAssignableFrom(type)
               || typeof(Exception).IsAssignableFrom(type)
               || typeof(Type).IsAssignableFrom(type)
               || Nullable.GetUnderlyingType(type) != null
               || typeof(XNode).IsAssignableFrom(type)
               || typeof(XmlNode).IsAssignableFrom(type)
            ;
    }

    public static bool IsObjectType(Type type)
    {
        return !IsDotNetTypeWithStringRepresentation(type) && !IsCollectionType(type);
    }

    public static bool IsCollectionType(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    public static HtmlSerializer Create(HtmlSerializerSettings? serializerSettings = null)
    {
        return new HtmlSerializer(serializerSettings);
    }
}
