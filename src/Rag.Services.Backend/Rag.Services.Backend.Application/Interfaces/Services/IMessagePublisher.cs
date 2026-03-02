namespace Rag.Services.Backend.Application.Interfaces.Services
{
    public interface IMessagePublisher
    {
        Task PublishMessageAsync<TMessage>(string exchange, string routingKey, TMessage message);
    }
}