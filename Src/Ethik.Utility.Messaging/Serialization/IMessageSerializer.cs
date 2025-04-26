namespace Ethik.Utility.Messaging.Serialization;

public interface IMessageSerializer
{
    object? Deserialize(byte[] data, Type messageType);
    T? Deserialize<T>(byte[] data);
    byte[] Serialize(object message);
}
