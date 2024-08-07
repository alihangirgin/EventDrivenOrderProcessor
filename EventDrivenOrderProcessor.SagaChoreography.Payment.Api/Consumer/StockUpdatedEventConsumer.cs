using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Constants;
using MassTransit;

namespace EventDrivenOrderProcessor.SagaChoreography.Payment.Api.Consumer
{
    public class StockUpdatedEventConsumer : IConsumer<StockUpdatedEvent>
    {
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<StockUpdatedEventConsumer> _logger;
        public StockUpdatedEventConsumer(IPublishEndpoint publishEndpoint, ILogger<StockUpdatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<StockUpdatedEvent> context)
        {
            var balance = 3000m;
            if (context.Message.PaymentInput.TotalPrice <= balance)
            {
                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderPaymentSuccessfulEventQueueName}"));
                await sendEndpoint.Send(new PaymentSuccessfulEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId
                });
                _logger.LogInformation($"UserId:{context.Message.BuyerId} successfully paid for {context.Message.PaymentInput.TotalPrice} cost");
            }
            else
            {
                var failMessage = $"UserId:{context.Message.BuyerId} failed paid for {context.Message.PaymentInput.TotalPrice} cost";
                await _publishEndpoint.Publish(new PaymentFailedEvent()
                {
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId,
                    FailMessage = failMessage,
                    OrderedItems = context.Message.OrderedItems
                });
                _logger.LogError(failMessage);
            }
        }
    }
}
