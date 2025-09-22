using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dorbit.Framework.Utils.Json;

public class PolymorphismOptions(string propertyName)
{
    public string PropertyName { get; } = propertyName;
    public List<JsonDerivedTypeAttribute> DriveTypes { get; set; } = new();
}

public static class ModelBuilderExtensions
{
    private static bool HasJsonAttribute(PropertyInfo propertyInfo)
    {
        return propertyInfo != null && propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(JsonFieldAttribute));
    }

    public static void AddJsonFields(this ModelBuilder modelBuilder, bool skipConventionalEntities = true)
    {
        if (modelBuilder == null)
            throw new ArgumentNullException(nameof(modelBuilder));

        var typeBase = typeof(TypeBase);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (skipConventionalEntities)
            {
                var typeConfigurationSource = typeBase.GetField("_configurationSource", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(entityType)?.ToString();
                if (Enum.TryParse(typeConfigurationSource, out ConfigurationSource typeSource) && typeSource == ConfigurationSource.Convention) continue;
            }

            var ignoredMembers = typeBase.GetField("_ignoredMembers", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(entityType) as Dictionary<string, ConfigurationSource>;

            bool NotIgnored(PropertyInfo property)
            {
                return property != null &&
                       !ignoredMembers.ContainsKey(property.Name) &&
                       property.CustomAttributes.All(a => a.AttributeType != typeof(NotMappedAttribute));
            }

            foreach (var clrProperty in entityType.ClrType.GetProperties().Where(x => NotIgnored(x) && HasJsonAttribute(x)))
            {
                var property = modelBuilder.Entity(entityType.ClrType).Property(clrProperty.PropertyType, clrProperty.Name);
                var modelType = clrProperty.PropertyType;

                if (clrProperty.PropertyType.IsGenericType)
                {
                    modelType = clrProperty.PropertyType.GetGenericArguments().First();
                }
                
                PolymorphismOptions polymorphismOptions = null;
                
                var jsonDerivedTypeAttributes = modelType.GetCustomAttributes<JsonDerivedTypeAttribute>().ToList();
                if (jsonDerivedTypeAttributes.Any())
                {
                    var jsonPolymorphicAttribute = modelType.GetCustomAttribute<JsonPolymorphicAttribute>();
                    polymorphismOptions = new PolymorphismOptions(jsonPolymorphicAttribute?.TypeDiscriminatorPropertyName ?? "$type")
                    {
                        DriveTypes = jsonDerivedTypeAttributes.ToList()
                    };
                }

                var converterType = typeof(JsonValueConverter<>).MakeGenericType(clrProperty.PropertyType);
                var converter = (ValueConverter)Activator.CreateInstance(converterType, [polymorphismOptions, null]);
                property.Metadata.SetValueConverter(converter);

                var valueComparer = typeof(JsonValueComparer<>).MakeGenericType(clrProperty.PropertyType);
                property.Metadata.SetValueComparer((ValueComparer)Activator.CreateInstance(valueComparer, []));
            }
        }
    }

    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder) where T : class
    {
        propertyBuilder
            .HasConversion(new JsonValueConverter<T>())
            .Metadata.SetValueComparer(new JsonValueComparer<T>());

        return propertyBuilder;
    }
}

public class JsonValueConverter<T>(PolymorphismOptions polymorphismOptions = null, ConverterMappingHints hints = null)
    : ValueConverter<T, string>(v => JsonHelper.Serialize(v), v => JsonHelper.Deserialize<T>(v, polymorphismOptions), hints)
    where T : class;

internal class JsonValueComparer<T>() : ValueComparer<T>((t1, t2) => DoEquals(t1, t2),
    t => DoGetHashCode(t),
    t => DoGetSnapshot(t))
{
    private static string Json(T instance)
    {
        return JsonSerializer.Serialize(instance);
    }

    private static T DoGetSnapshot(T instance)
    {
        if (instance is ICloneable cloneable)
            return (T)cloneable.Clone();

        var result = (T)JsonSerializer.Deserialize(Json(instance), typeof(T));
        return result;
    }

    private static int DoGetHashCode(T instance)
    {
        if (instance is IEquatable<T>)
            return instance.GetHashCode();

        return Json(instance).GetHashCode();
    }

    private static bool DoEquals(T left, T right)
    {
        if (left is IEquatable<T> equatable)
            return equatable.Equals(right);

        var result = Json(left).Equals(Json(right));
        return result;
    }
}

internal static class JsonHelper
{
    public static T Deserialize<T>(string json, PolymorphismOptions polymorphismOptions = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        if (polymorphismOptions != null)
        {
            foreach (var driveTypeAttribute in polymorphismOptions.DriveTypes)
            {
                if(!json.Contains($"\"{polymorphismOptions.PropertyName}\": \"{driveTypeAttribute.TypeDiscriminator}\"")) continue;
                return JsonSerializer.Deserialize(json, driveTypeAttribute.DerivedType) as T;
            }
        }
        return  JsonSerializer.Deserialize<T>(json);
    }

    public static string Serialize<T>(T obj) where T : class
    {
        return obj == null ? null : JsonSerializer.Serialize(obj);
    }
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class JsonFieldAttribute : Attribute;