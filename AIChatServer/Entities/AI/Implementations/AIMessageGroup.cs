using AIChatServer.Entities.AI.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.AI.Implementations
{
    public class AIMessageGroup : IAIMessageGroup, IDisposable
    {
        private readonly int _maxBufferSize;
        private readonly int _minBufferSize;
        private readonly List<AIMessage> _messages = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly ILogger<AIMessageGroup> _logger;
        private bool _disposed;

        public event Func<List<AIMessage>, Task>? OnBufferOverflowing;

        public AIMessageGroup(int maxBufferSize, int minBufferSize, ILogger<AIMessageGroup> logger)
        {
            if (minBufferSize >= maxBufferSize)
                throw new ArgumentException("minBufferSize must be less than maxBufferSize");

            _maxBufferSize = maxBufferSize;
            _minBufferSize = minBufferSize;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("AIMessageGroup created with max {Max}, min {Min}", maxBufferSize, minBufferSize);
        }

        public async Task AddMessageAsync(AIMessage message)
        {
            await _semaphore.WaitAsync();
            try
            {
                _messages.Add(message);
                _logger.LogDebug("Message added. Current buffer size: {Count}", _messages.Count);

                if (_messages.Count > _maxBufferSize)
                {
                    int messagesToRemove = _messages.Count - _minBufferSize;
                    var overflowingMessages = _messages.Take(messagesToRemove).ToList();
                    _messages.RemoveRange(0, messagesToRemove);

                    _logger.LogWarning("Buffer overflow: removed {Removed}, remaining {Remaining}",
                        messagesToRemove, _messages.Count);

                    if (OnBufferOverflowing != null)
                    {
                        try
                        {
                            await OnBufferOverflowing.Invoke(overflowingMessages);
                            _logger.LogInformation("Overflow handled by subscriber.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while handling buffer overflow.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Buffer overflow detected, but no handler subscribed.");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetMessagesAsync(IReadOnlyCollection<AIMessage> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            await _semaphore.WaitAsync();
            try
            {
                _messages.Clear();
                _messages.AddRange(messages);
                _logger.LogDebug("Messages set. New buffer size: {Count}", _messages.Count);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public List<AIMessage> GetMessages()
        {
            _semaphore.Wait();
            try
            {
                return [.. _messages];
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            _semaphore.Dispose();
            _disposed = true;
            _logger.LogInformation("AIMessageGroup disposed.");
        }
    }
}
