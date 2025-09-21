namespace ImplementationScannerDemo.Models;

internal class UserCreatedEvent : BaseEvent
{
    public Guid Uuid { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
