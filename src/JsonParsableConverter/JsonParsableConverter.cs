using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModelingEvolution.JsonParsableConverter;

/// <summary>
/// A JSON converter for types that implement <see cref="IParsable{TSelf}"/>.
/// </summary>
/// <typeparam name="T">The type to convert, which must implement <see cref="IParsable{TSelf}"/>.</typeparam>
/// <remarks>
/// <para>
/// This converter enables seamless JSON serialization and deserialization for types that follow
/// the .NET IParsable pattern. It's particularly useful for custom value types, domain primitives,
/// and strongly-typed identifiers in Domain-Driven Design (DDD) applications.
/// </para>
/// <para>
/// During serialization, the converter calls <see cref="object.ToString"/> on the value.
/// During deserialization, it uses the static <c>Parse</c> method defined by <see cref="IParsable{TSelf}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Define a custom value type
/// public readonly record struct ProductId(Guid Value) : IParsable&lt;ProductId&gt;
/// {
///     public static ProductId Parse(string s, IFormatProvider? provider) 
///         => new(Guid.Parse(s));
///     
///     public static bool TryParse(string? s, IFormatProvider? provider, out ProductId result)
///     {
///         if (Guid.TryParse(s, out var guid))
///         {
///             result = new ProductId(guid);
///             return true;
///         }
///         result = default;
///         return false;
///     }
///     
///     public override string ToString() => Value.ToString();
/// }
/// 
/// // Configure the converter
/// var options = new JsonSerializerOptions
/// {
///     Converters = { new JsonParsableConverter&lt;ProductId&gt;() }
/// };
/// 
/// // Use it
/// var id = new ProductId(Guid.NewGuid());
/// string json = JsonSerializer.Serialize(id, options);
/// ProductId deserialized = JsonSerializer.Deserialize&lt;ProductId&gt;(json, options);
/// </code>
/// </example>
public class JsonParsableConverter<T> : JsonConverter<T> where T : IParsable<T>
{
    /// <summary>
    /// Reads and converts JSON to type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>The converted value, or <c>null</c> if the JSON value is null.</returns>
    /// <exception cref="JsonException">Thrown when the JSON value cannot be parsed as type <typeparamref name="T"/>.</exception>
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        var stringValue = reader.GetString();
        if (stringValue == null)
        {
            return default;
        }

        try
        {
            return T.Parse(stringValue, provider: null);
        }
        catch (Exception ex) when (ex is not JsonException)
        {
            throw new JsonException($"Failed to parse '{stringValue}' as {typeof(T).Name}.", ex);
        }
    }

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> as JSON.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="options">The serializer options.</param>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.ToString());
    }

    /// <summary>
    /// Determines whether the specified type can be handled by this converter.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><c>true</c> if the type can be handled; otherwise, <c>false</c>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(T).IsAssignableFrom(typeToConvert);
    }
}