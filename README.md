# JsonParsableConverter

[![NuGet](https://img.shields.io/nuget/v/ModelingEvolution.JsonParsableConverter.svg)](https://www.nuget.org/packages/ModelingEvolution.JsonParsableConverter/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)

A high-performance System.Text.Json converter for types implementing `IParsable<T>`. This library enables seamless JSON serialization/deserialization for custom value types, records, and structs using the standard .NET `IParsable` pattern.

## Features

- 🚀 **High Performance**: Leverages System.Text.Json for optimal performance
- 🎯 **Type Safe**: Full compile-time type safety with generic constraints
- 🔧 **Easy Integration**: Simple registration with JsonSerializerOptions
- 📦 **Minimal Dependencies**: Only depends on System.Text.Json
- 🧩 **DDD Friendly**: Perfect for Domain-Driven Design value objects and strongly-typed IDs

## Installation

```bash
dotnet add package ModelingEvolution.JsonParsableConverter
```

## Quick Start

### Define a Parsable Type

```csharp
using System.Text.Json.Serialization;
using ModelingEvolution.JsonParsableConverter;

[JsonConverter(typeof(JsonParsableConverter<ProductId>))]
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
```

### Usage

With the attribute applied, the type automatically uses the converter - no additional configuration needed:

```csharp
var productId = new ProductId(Guid.NewGuid());

// Just works - no special JsonSerializerOptions needed!
string json = JsonSerializer.Serialize(productId);
ProductId deserialized = JsonSerializer.Deserialize<ProductId>(json);
```

## Advanced Usage

### More Examples

String-based value type:
```csharp
[JsonConverter(typeof(JsonParsableConverter<CustomerName>))]
public readonly record struct CustomerName(string Value) : IParsable<CustomerName>
{
    public static CustomerName Parse(string s, IFormatProvider? provider) 
        => new(s);
    
    public static bool TryParse(string? s, IFormatProvider? provider, out CustomerName result)
    {
        result = new CustomerName(s ?? string.Empty);
        return s != null;
    }
    
    public override string ToString() => Value;
}
```

Complex value type with validation:
```csharp
[JsonConverter(typeof(JsonParsableConverter<Email>))]
public readonly record struct Email : IParsable<Email>
{
    private readonly string _value;
    
    public Email(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Invalid email format", nameof(value));
        _value = value;
    }
    
    public static Email Parse(string s, IFormatProvider? provider)
    {
        if (!IsValid(s))
            throw new FormatException($"Invalid email format: {s}");
        return new Email(s);
    }
    
    public static bool TryParse(string? s, IFormatProvider? provider, out Email result)
    {
        if (s != null && IsValid(s))
        {
            result = new Email(s);
            return true;
        }
        result = default;
        return false;
    }
    
    private static bool IsValid(string email) 
        => !string.IsNullOrWhiteSpace(email) && email.Contains('@');
    
    public override string ToString() => _value;
}
```

### Using in Complex Types

Since the converter is applied via attributes, it works seamlessly in complex objects:

```csharp
public class Order
{
    public OrderId Id { get; set; }
    public CustomerId CustomerId { get; set; }
    public Money TotalAmount { get; set; }
    public DateOnly OrderDate { get; set; }
}

public class Customer
{
    public CustomerId Id { get; set; }
    public CustomerName Name { get; set; }
    public Email Email { get; set; }
}

// No special configuration needed - just serialize!
var customer = new Customer 
{ 
    Id = CustomerId.Parse("12345", null),
    Name = new CustomerName("John Doe"),
    Email = Email.Parse("john@example.com", null)
};

string json = JsonSerializer.Serialize(customer);
Customer deserialized = JsonSerializer.Deserialize<Customer>(json);
```

## Why IParsable?

The `IParsable<T>` interface was introduced in .NET 7 as a standard way to define parsing behavior for custom types. Using this pattern with JSON serialization provides:

1. **Consistency**: Same parsing logic for JSON, string manipulation, and user input
2. **Maintainability**: Single source of truth for parsing logic
3. **Framework Support**: Leverages built-in .NET abstractions
4. **Type Safety**: Compile-time guarantees with generic constraints

## Performance

This converter is designed for high performance:
- Zero allocations for value type conversions
- Minimal overhead compared to manual serialization
- Efficient string handling using `Utf8JsonReader/Writer`

## Requirements

- .NET 9.0 or higher
- System.Text.Json

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

This converter is inspired by the need for better value object support in System.Text.Json and is based on patterns from the [MicroPlumberd](https://github.com/modelingevolution/micro-plumberd) project.