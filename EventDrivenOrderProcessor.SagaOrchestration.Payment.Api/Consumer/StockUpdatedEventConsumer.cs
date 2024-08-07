using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Constants;
using EventDrivenOrderProcessor.Shared.Events;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;

namespace EventDrivenOrderProcessor.SagaOrchestration.Payment.Api.Consumer
{
    public class StockUpdatedEventConsumer : IConsumer<IStockUpdatedPaymentRequestEvent>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<StockUpdatedEventConsumer> _logger;
        public StockUpdatedEventConsumer(ILogger<StockUpdatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<IStockUpdatedPaymentRequestEvent> context)
        {
            var balance = 3000m;
            if (context.Message.PaymentInput.TotalPrice <= balance)
            {
                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderQueue}"));
                await sendEndpoint.Send(new PaymentSuccessfulStateEvent()
                {
                    CorrelationId = context.Message.CorrelationId,
                    BuyerId = context.Message.BuyerId,
                    OrderId = context.Message.OrderId
                });
                _logger.LogInformation($"UserId:{context.Message.BuyerId} successfully paid for {context.Message.PaymentInput.TotalPrice} cost");
            }
            else
            {
                var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderQueue}"));
                var failMessage = $"UserId:{context.Message.BuyerId} failed paid for {context.Message.PaymentInput.TotalPrice} cost";
                await sendEndpoint.Send(new PaymentFailedStateEvent()
                {
                    CorrelationId = context.Message.CorrelationId,
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
