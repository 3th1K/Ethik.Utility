public interface IPublisher
{
    Task PublishAsync<T>(T message, string topic, IDictionary<string, string?>? headers = null, CancellationToken cancellationToken = default);
}
