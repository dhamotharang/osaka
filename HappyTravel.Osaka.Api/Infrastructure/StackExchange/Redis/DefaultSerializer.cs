using System.Text.Json;
using StackExchange.Redis.Extensions.Core;

namespace HappyTravel.Osaka.Api.Infrastructure.StackExchange.Redis
{
    public class DefaultSerializer : ISerializer
    {
        public byte[] Serialize(object item)
            => JsonSerializer.SerializeToUtf8Bytes(item);
        

        public T? Deserialize<T>(byte[] serializedObject)
            => JsonSerializer.Deserialize<T>(serializedObject);
    }
}