namespace Ethik.Utility.Messaging;

public interface IConsumer : IAsyncDisposable
{
    Task StartConsumingAsync();
    Task StopConsumingAsync();
}
