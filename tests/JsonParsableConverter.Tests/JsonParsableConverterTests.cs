using System;
using System.Text.Json;
using ModelingEvolution.JsonParsableConverter;
using Xunit;

namespace JsonParsableConverter.Tests;

public class JsonParsableConverterTests
{
    #region Test Types

    public readonly record struct ProductId(Guid Value) : IParsable<ProductId>
    {
        public static ProductId Parse(string s, IFormatProvider? provider) 
            => new(Guid.Parse(s));
        
        public static bool TryParse(string? s, IFormatProvider? provider, out ProductId result)
        {
            if (Guid.TryParse(s, out var guid))
            {
                result = new ProductId(guid);
                return true;
            }
            result = default;
            return false;
        }
        
        public override string ToString() => Value.ToString();
    }

    public readonly record struct CustomerId(int Value) : IParsable<CustomerId>
    {
        public static CustomerId Parse(string s, IFormatProvider? provider)
        {
            if (!int.TryParse(s, out var value))
                throw new FormatException($"Unable to parse '{s}' as CustomerId");
            return new CustomerId(value);
        }

        public static bool TryParse(string? s, IFormatProvider? provider, out CustomerId result)
        {
            if (int.TryParse(s, out var value))
            {
                result = new CustomerId(value);
                return true;
            }
            result = default;
            return false;
        }

        public override string ToString() => Value.ToString();
    }

    public class ComplexObject
    {
        public ProductId ProductId { get; set; }
        public CustomerId CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    private readonly JsonSerializerOptions _options;

    public JsonParsableConverterTests()
    {
        _options = new JsonSerializerOptions
        {
            Converters = { 
                new JsonParsableConverter<ProductId>(),
                new JsonParsableConverter<CustomerId>()
            }
        };
    }

    [Fact]
    public void Should_Serialize_Guid_Based_Type()
    {
        // Arrange
        var id = new ProductId(Guid.Parse("12345678-1234-1234-1234-123456789012"));

        // Act
        var json = JsonSerializer.Serialize(id, _options);

        // Assert
        Assert.Equal("\"12345678-1234-1234-1234-123456789012\"", json);
    }

    [Fact]
    public void Should_Deserialize_Guid_Based_Type()
    {
        // Arrange
        var json = "\"12345678-1234-1234-1234-123456789012\"";
        var expected = new ProductId(Guid.Parse("12345678-1234-1234-1234-123456789012"));

        // Act
        var result = JsonSerializer.Deserialize<ProductId>(json, _options);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_Serialize_Int_Based_Type()
    {
        // Arrange
        var id = new CustomerId(42);

        // Act
        var json = JsonSerializer.Serialize(id, _options);

        // Assert
        Assert.Equal("\"42\"", json);
    }

    [Fact]
    public void Should_Deserialize_Int_Based_Type()
    {
        // Arrange
        var json = "\"42\"";
        var expected = new CustomerId(42);

        // Act
        var result = JsonSerializer.Deserialize<CustomerId>(json, _options);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Should_Handle_Null_When_Deserializing()
    {
        // Arrange
        var json = "null";

        // Act
        var result = JsonSerializer.Deserialize<ProductId?>(json, _options);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Should_Handle_Null_When_Serializing()
    {
        // Arrange
        ProductId? id = null;

        // Act
        var json = JsonSerializer.Serialize(id, _options);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Should_Throw_JsonException_For_Invalid_Format()
    {
        // Arrange
        var json = "\"not-a-guid\"";

        // Act & Assert
        var exception = Assert.Throws<JsonException>(() => 
            JsonSerializer.Deserialize<ProductId>(json, _options));
        
        Assert.Contains("Failed to parse", exception.Message);
        Assert.Contains("ProductId", exception.Message);
    }

    [Fact]
    public void Should_Work_With_Complex_Objects()
    {
        // Arrange
        var obj = new ComplexObject
        {
            ProductId = new ProductId(Guid.Parse("12345678-1234-1234-1234-123456789012")),
            CustomerId = new CustomerId(42),
            Name = "Test Product"
        };

        // Act
        var json = JsonSerializer.Serialize(obj, _options);
        var deserialized = JsonSerializer.Deserialize<ComplexObject>(json, _options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(obj.ProductId, deserialized.ProductId);
        Assert.Equal(obj.CustomerId, deserialized.CustomerId);
        Assert.Equal(obj.Name, deserialized.Name);
    }

    [Fact]
    public void Should_Work_In_Arrays()
    {
        // Arrange
        var ids = new[]
        {
            new ProductId(Guid.NewGuid()),
            new ProductId(Guid.NewGuid()),
            new ProductId(Guid.NewGuid())
        };

        // Act
        var json = JsonSerializer.Serialize(ids, _options);
        var deserialized = JsonSerializer.Deserialize<ProductId[]>(json, _options);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(ids.Length, deserialized.Length);
        for (int i = 0; i < ids.Length; i++)
        {
            Assert.Equal(ids[i], deserialized[i]);
        }
    }

}