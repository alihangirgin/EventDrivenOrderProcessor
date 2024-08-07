using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Constants;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaChoreography.Stock.Api.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider _sendEndpointProvider;


        public OrderCreatedEventConsumer(AppDbContext appDbContext, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider)
        {
            _appDbContext = appDbContext;
            _logger = logger;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
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

        private async Task SendStockNotUpdatedEvent(ConsumeContext<OrderCreatedEvent> context, string failMessage)
        {
            var sendFailedEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.OrderStockNotUpdatedEventQueueName}"));
            await sendFailedEndpoint.Send(new StockNotUpdatedEvent()
            {
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
                FailMessage = failMessage
            });
            _logger.LogError(failMessage);
        }

        private async Task SendStockUpdatedEvent(ConsumeContext<OrderCreatedEvent> context)
        {

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMqConstants.PaymentStockUpdatedEventQueueName}" ));
            await sendEndpoint.Send(new StockUpdatedEvent()
            {
                PaymentInput = context.Message.PaymentInput,
                OrderedItems = context.Message.OrderedItems,
                BuyerId = context.Message.BuyerId,
                OrderId = context.Message.OrderId,
            });
            _logger.LogInformation("Stock was reserved for BuyerId:{buyerId}", context.Message.BuyerId);
        }
    }
}
