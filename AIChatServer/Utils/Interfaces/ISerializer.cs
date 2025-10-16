namespace AIChatServer.Utils.Interfaces
{
    public interface ISerializer
    {
        public T Deserialize<T>(byte[] bytes);
        public T Deserialize<T>(string str);
        public byte[] SerializeToBytes<T>(T obj);
        public string Serialize<T>(T obj);
    }
}
