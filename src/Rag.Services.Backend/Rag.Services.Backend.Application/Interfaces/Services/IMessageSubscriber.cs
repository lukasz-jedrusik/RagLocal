using Rag.Services.Backend.Application.Interfaces.Messages;

namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IMessageSubscriber
    {
        IMessageSubscriber SubscribeMessage<TMessage>(
            string queue,
            string exchange,
            string routingKey,
            Func<TMessage, Task> handle) where TMessage : class, IMessage;

        IMessageSubscriber RespondToRequest<TRequest, TResponse>(
            string queue,
            string exchange,
            string routingKey,
            Func<TRequest, Task<TResponse>> handleRequest)
            where TRequest : class, IMessageRequest
            where TResponse : class, IMessageResponse;
    }
}