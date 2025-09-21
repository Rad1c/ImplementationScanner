# ImplementationScanner

A .NET library that scans assemblies for types implementing a specified base type, creates instances using reflection, and populates them with realistic fake data using the Bogus library.

## Features

- **Assembly Scanning**: Automatically discovers all types that implement or inherit from a specified base type
- **Instance Creation**: Creates instances using reflection without invoking constructors
- **Fake Data Population**: Populates object properties with realistic fake data using Bogus
- **JSON Serialization**: Generates JSON representations of created instances
- **Custom Filtering**: Supports custom type filters for fine-grained control over which types to include
- **Generic Design**: Works with any base type, class, or interface

## Installation

### Prerequisites

- .NET 8.0 or higher

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/Rad1c/ImplementationScanner.git
   cd ImplementationScanner
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run the demo:
   ```bash
   dotnet run --project EventLibraryGenerator
   ```

## Usage

### Basic Example

```csharp
using EventLibraryGenerator;
using EventLibraryGenerator.Demo;

// Generate instances of all types that implement BaseEvent
var events = ImplementationScanner<BaseEvent>.GenerateEvents();
Console.WriteLine($"Generated {events.Count} events.");

// Generate JSON representation
string json = ImplementationScanner<BaseEvent>.GenerateEventsJsonString();
Console.WriteLine(json);
```

### Custom Type Filtering

```csharp
// Filter to only include types whose names end with "Event"
ImplementationScanner<BaseEvent>.SetTypeFilter(t =>
    typeof(BaseEvent).IsAssignableFrom(t) &&
    t.Name.EndsWith("Event") &&
    !t.IsAbstract);

var filteredEvents = ImplementationScanner<BaseEvent>.GenerateEvents();
```

### Working with System Types

```csharp
using System.Transactions;

// Works with built-in .NET types too
var transactions = ImplementationScanner<Transaction>.GenerateEvents();
Console.WriteLine($"Generated {transactions.Count} transactions.");
```

## API Reference

### `ImplementationScanner<TBase>` Class

#### Methods

##### `GenerateEvents()`
Returns a list of instances of all types that implement `TBase`.

**Returns:** `List<TBase>` - List of populated instances

##### `GenerateEventsJsonString()`
Generates a JSON string representation of all instances.

**Returns:** `string` - JSON array containing all instances

##### `SetTypeFilter(Func<Type, bool> filter)`
Sets a custom filter to control which types are included.

**Parameters:**
- `filter` - A function that takes a `Type` and returns `bool`

##### `CreateAndPopulate(Type eventType, Faker faker)`
Creates and populates a single instance of the specified type.

**Parameters:**
- `eventType` - The type to instantiate
- `faker` - Bogus faker instance for generating fake data

**Returns:** `object?` - Populated instance or null if creation failed

## Supported Data Types

The library automatically generates appropriate fake data for common .NET types:

- `string` - Random words
- `int` - Random integers (1-1000)
- `decimal` - Random decimals (10-1000)
- `Guid` - New GUIDs
- `DateTime` - Random dates
- Complex objects - Recursively populated
- Collections - Populated with fake items

## Example Domain Model

```csharp
namespace EventLibraryGenerator.Demo;

public abstract class BaseEvent
{
    // Base event class
}

public class UserCreatedEvent : BaseEvent
{
    public string Username { get; set; }
    public string Email { get; set; }
}
```

## Output Example

Running the demo application produces output like:

```
Generated 1 events.
[{
  "Username": "sed",
  "Email": "et"
}]
Generated 1 filtered events.
Generated 3 transactions.
```

## How It Works

1. **Assembly Scanning**: Uses reflection to scan all loaded assemblies for types
2. **Type Filtering**: Applies built-in and custom filters to select relevant types
3. **Instance Creation**: Uses `FormatterServices.GetUninitializedObject()` to create instances without calling constructors
4. **Property Population**: Iterates through public properties and sets them with fake data
5. **Circular Reference Handling**: Tracks visited types to prevent infinite recursion
6. **JSON Serialization**: Uses `System.Text.Json` for JSON output

## Dependencies

- [Bogus](https://github.com/bchavez/Bogus) (v35.6.3) - For generating fake data

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is licensed under the MIT License.