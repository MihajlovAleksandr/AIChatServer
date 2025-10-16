namespace AIChatServer.Entities.DTO.Response
{
    public record CommandResponse(string operation, object? data = null)
    {
        private readonly string _operation = operation;
        private readonly object? _data = data;

        public override string ToString()
        {
            return $"Command {{{_operation}}} with data {{{_data}}}";
        }
    }
}
