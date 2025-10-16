using AIChatServer.Entities.Connection.Interfaces;
using Newtonsoft.Json.Linq;

namespace AIChatServer.Entities.Connection
{
    public class Command(string operation, IConnection sender, JObject? data)
    {
        private readonly JObject? _data = data;
        public string Operation { get; } = operation;
        public IConnection Sender { get; } = sender ?? throw new ArgumentNullException(nameof(sender));

        public T? GetData<T>()
        {
            if (_data == null) return default;
            return _data.ToObject<T>();
        }

        public override string ToString()
        {
            return _data == null ? $"Command {{{Operation}}} Without data" : $"Command {{{Operation}}} With some data";
        }
    }
}
