namespace AIChatServer.Entities.Chats
{
    public class Message
    {
        public Guid? Id { get; set; }
        public Guid Chat { get; set; }
        public Guid Sender { get; set; }
        public string Text { get; set; }
        public DateTime? Time { get; set; }
        public DateTime? LastUpdate { get; set; }

        public override string ToString()
        {
            return $"Message {Id}:\nFrom User{Sender} To Chat{Chat} In {Time}\nUpdate in {LastUpdate}\n{Text}";
        }
    }

}
