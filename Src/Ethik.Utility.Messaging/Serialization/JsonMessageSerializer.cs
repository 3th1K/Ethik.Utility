namespace Ethik.Utility.Messaging.Serialization;

public class JsonMessageSerializer : IMessageSerializer
{
    public object Deserialize(byte[] data, Type messageType)
    {
        return System.Text.Json.JsonSerializer.Deserialize(data, messageType);
    }

    public T Deserialize<T>(byte[] data)
    {
        var a = System.Text.Json.JsonSerializer.Deserialize(data, typeof(T));
        return (T)a;
    }


    public byte[] Serialize(object message)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(message);
    }
}
