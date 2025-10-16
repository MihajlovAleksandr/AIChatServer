using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils
{
    public static class CommandSender
    {
        public static async Task SendCommandAsync(IConnection? connection, CommandResponse command, ISerializer serializer)
        {
            if (connection != null)
                await connection.SendCommandAsync(serializer.SerializeToBytes(command));
        }

        public static async Task SendCommandAsync(IEnumerable<IConnection> connections, CommandResponse command, ISerializer serializer)
        {
            foreach (var connection in connections)
                await SendCommandAsync(connection, command, serializer);
        }
    }
}