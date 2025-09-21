namespace EventLibraryGenerator.Demo;

public class UserCreatedEvent : BaseEvent
{
    public string Username { get; set; }
    public string Email { get; set; }
}
