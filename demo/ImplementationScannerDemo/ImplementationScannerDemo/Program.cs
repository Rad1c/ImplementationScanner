

using ImplementationScanner;
using ImplementationScannerDemo.Models;

namespace ImplementationScannerDemo;

public class Program
{
    public static void Main(string[] args)
    {
        var implementations = Scanner<BaseEvent>.GenerateEvents();

        var implementationJson = Scanner<BaseEvent>.GenerateEventsJsonString();

        Console.WriteLine("Implementations:", implementationJson);

    }
}