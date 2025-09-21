namespace ImplementationScannerDemo.Models;

internal abstract class BaseEvent
{
    public Guid Uuid { get; private set; }

    protected BaseEvent()
    {
        Uuid = Guid.NewGuid();
    }
}
