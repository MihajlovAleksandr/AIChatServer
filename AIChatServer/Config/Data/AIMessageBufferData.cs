namespace AIChatServer.Config.Data
{
    public record AIMessageBufferData(
        AIMessageBufferSize MessageBufferSize,
        AIMessageBufferSize CompressedMessageBufferSize
    );
}
