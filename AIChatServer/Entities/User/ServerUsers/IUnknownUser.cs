namespace AIChatServer.Entities.User.ServerUsers
{
    interface IUnknownUser
    {
        Connection.Connection GetCurrentConnection();
    }
}
