namespace AIChatServer.Entities.Logger
{
    public record LogEntry
    {
        public DateTime Timestamp { get; init; }
        public string Level { get; init; } = default!;
        public string Message { get; init; } = default!;
        public string Source { get; init; } = default!;
    }
}
