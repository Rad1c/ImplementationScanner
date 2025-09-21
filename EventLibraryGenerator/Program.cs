using System.Transactions;
using EventLibraryGenerator.Demo;

namespace EventLibraryGenerator;

class Program
{
    static void Main()
    {
        // Search and instantiate all implementations of BaseEvent
        var events = ImplementationScanner<BaseEvent>.GenerateEvents();
        Console.WriteLine($"Generated {events.Count} events.");

        // Generate JSON string
        string json = ImplementationScanner<BaseEvent>.GenerateEventsJsonString();
        Console.WriteLine(json);

        // Filtered example: just types that end with "Event"
        ImplementationScanner<BaseEvent>.SetTypeFilter(t =>
            typeof(BaseEvent).IsAssignableFrom(t) &&
            t.Name.EndsWith("Event") &&
            !t.IsAbstract);

        var filteredEvents = ImplementationScanner<BaseEvent>.GenerateEvents();
        Console.WriteLine($"Generated {filteredEvents.Count} filtered events.");

        // Search and instantiate all implementations of System.Transaction
        var transactions = ImplementationScanner<Transaction>.GenerateEvents();
        Console.WriteLine($"Generated {transactions.Count} transactions.");
    }
}