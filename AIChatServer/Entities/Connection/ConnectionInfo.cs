namespace AIChatServer.Entities.Connection
{
    public class ConnectionInfo(Guid id, Guid userId, string device, DateTime? lastOnline)
    {
        public Guid Id { get { return id; } }
        public Guid UserId { get { return userId; } }
        public string Device { get { return device; } }
        public DateTime? LastOnline { get { return lastOnline; } }

        public override string ToString()
        {
            return $"ConnectionInfo {id}/{userId}:\n{device} in {lastOnline}";
        }
    }
}
