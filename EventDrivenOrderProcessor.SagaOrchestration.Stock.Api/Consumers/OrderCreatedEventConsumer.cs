using EventDrivenOrderProcessor.Shared.Constants;
using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Events;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;
using MassTransit.Transports;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaOrchestration.Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedStockRequestEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext appDbContext, ISendEndpointProvider sendEndpointProvider, ILogger<OrderCreatedEventConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _appDbContext = appDbContext;
            _sendEndpointProvider = sendEndpointProvider;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedStockRequestEvent> context)
        {
            var stocks = await _appDbContext.Stocks
                .Where(x => context.Message.OrderedItems.Select(y => y.ProductId).ToList().Contains(x.ProductId))
                .ToListAsync();

            foreach (var orderedItem in context.Message.OrderedItems)
            {
                var stock = stocks.FirstOrDefault(x => x.ProductId == orderedItem.ProductId);
                if (stock == null)
                {
                    await SendStockNotUpdatedEvent(context, $"Product not found for ProductId:{orderedItem.ProductId}");
                    return;
                }

                stock.Count -= orderedItem.Count;
                if (stock.Count < 0)
                {
                    await SendStockNotUpdatedEvent(context, $"Insufficient Stock Amount for ProductId:{orderedItem.ProductId}");
                    return;
                }
            }

            _appDbContext.UpdateRange(stocks);
            await _appDbContext.SaveChangesAsync();

            await SendStockUpdatedEvent(context);
        }
        private async Task SendStockNotUpdatedEvent(ConsumeContext<IOrderCreatedStockRequestEvent> context, string failMessage)
        {
            var sendFailedEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderQueue}"));
            await _publishEndpoint.Publish(new StockNotUpdatedStateEvent()
            {
                CorrelationId = context.CorrelationId ?? Guid.Empty,
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                FailMessage = failMessage
            });
            _logger.LogError(failMessage);
        }
        private async Task SendStockUpdatedEvent(ConsumeContext<IOrderCreatedStockRequestEvent> context)
        {
            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderQueue}"));
            await _publishEndpoint.Publish(new StockUpdatedStateEvent()
            {
                CorrelationId = context.CorrelationId ?? Guid.Empty,
                PaymentInput = context.Message.PaymentInput,
                OrderedItems = context.Message.OrderedItems,
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
            });
            _logger.LogInformation("Stock was reserved for BuyerId:{buyerId}", context.Message.BuyerId);
        }
    }
}
