using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using Bogus;

namespace ImplementationScanner;

public static class Scanner<TBase>
{
    private static JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static Func<Type, bool> _typeFilter = t =>
            typeof(TBase).IsAssignableFrom(t) &&
            !t.IsAbstract &&
            !t.IsInterface &&
            t != typeof(TBase);

    /// <summary>
    /// Allows setting a custom type filter.
    /// </summary>
    public static void SetTypeFilter(Func<Type, bool> filter)
    {
        _typeFilter = filter ?? throw new ArgumentNullException(nameof(filter));
    }

    public static List<TBase> GetImplementations(string language = "en")
    {
        var faker = new Faker(language);
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var eventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(_typeFilter)
            .ToList();

        var results = new List<TBase>();

        foreach (var type in eventTypes)
        {
            var instance = (TBase?)CreateAndPopulate(type, faker);

            if (instance != null)
                results.Add(instance);
        }

        return results;
    }

    public static string GetImplementationsJson(JsonSerializerOptions? options = null)
    {
        var events = GetImplementations();

        var jsonItems = new List<string>();

        foreach (var ev in events)
        {
            string json = JsonSerializer.Serialize(ev, ev.GetType(), options ?? _jsonOptions);
            jsonItems.Add(json);
        }

        string finalJson = "[" + string.Join(",", jsonItems) + "]";
        return finalJson;
    }

    public static object? CreateAndPopulate(Type eventType, Faker faker)
    {
        try
        {
            var instance = TryCreateInstance(eventType);

            FillObject(instance, faker);

            return instance!;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private static void FillObject(object obj, Faker faker, HashSet<Type> visited = null)
    {
        if (obj == null) return;

        visited ??= [];

        var type = obj.GetType();
        if (visited.Contains(type)) return; // Skip if already visited

        visited.Add(type);

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propType = prop.PropertyType;

            if (propType == typeof(string))
                TrySetPropValue(prop, obj, faker.Lorem.Word());
            else if (propType == typeof(int))
                TrySetPropValue(prop, obj, faker.Random.Int(1, 1000));
            else if (propType == typeof(decimal))
                TrySetPropValue(prop, obj, faker.Random.Decimal(10, 1000));
            else if (propType == typeof(Guid))
                TrySetPropValue(prop, obj, Guid.NewGuid());
            else if (propType == typeof(DateTime))
                TrySetPropValue(prop, obj, faker.Date.Recent());
            else if (propType.IsEnum)
            {
                var values = Enum.GetValues(propType);
                TrySetPropValue(prop, obj, values.GetValue(faker.Random.Int(0, values.Length - 1)));
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
            {
                var elementType = propType.IsGenericType
                    ? propType.GetGenericArguments().FirstOrDefault()
                    : propType.GetElementType();

                if (elementType != null)
                {
                    var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                    for (int i = 0; i < faker.Random.Int(1, 3); i++)
                        list.Add(CreateAndFill(elementType, faker, new HashSet<Type>(visited)));

                    if (propType.IsArray)
                    {
                        var array = Array.CreateInstance(elementType, list.Count);
                        list.CopyTo(array, 0);
                        TrySetPropValue(prop, obj, array);
                    }
                    else
                        TrySetPropValue(prop, obj, list);
                }
            }
            else if (propType.IsClass && propType != typeof(string))
            {
                var nestedInstance = TryCreateInstance(propType);
                FillObject(nestedInstance, faker, new HashSet<Type>(visited));
                TrySetPropValue(prop, obj, nestedInstance);
            }
        }
    }

    private static object CreateAndFill(Type type, Faker faker, HashSet<Type> visited)
    {
        if (type == typeof(string)) return faker.Lorem.Word();
        if (type == typeof(int)) return faker.Random.Int(1, 1000);
        if (type == typeof(decimal)) return faker.Random.Decimal(10, 1000);
        if (type == typeof(double)) return faker.Random.Double(1, 1000);
        if (type == typeof(Guid)) return Guid.NewGuid();
        if (type == typeof(DateTime)) return faker.Date.Recent();

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.GetValue(faker.Random.Int(0, values.Length - 1));
        }

        // Complex type
        if (type.IsClass && type != typeof(string))
        {
            if (visited.Contains(type)) return null; // Stop circular references

            visited.Add(type);

            var instance = TryCreateInstance(type);
            FillObject(instance, faker, new HashSet<Type>(visited)); // Pass a copy of visited set
            return instance;
        }

        return null;
    }

    // Create an instance without invoking constructor
    private static object? TryCreateInstance(Type type)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var eventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        Type? typeToInstantiate = eventTypes.FirstOrDefault();

        if (typeToInstantiate == null) return null;

        try
        {
            return FormatterServices.GetUninitializedObject(typeToInstantiate);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private static void TrySetPropValue(PropertyInfo prop, object? obj, object? value)
    {
        if (value == null) return;
        if (prop.CanWrite)
        {
            prop.SetValue(obj, value);
        }
        else
        {
            var setter = prop.SetMethod;
            setter?.Invoke(obj, [value]);
        }
    }
}