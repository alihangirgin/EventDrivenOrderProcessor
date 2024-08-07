using EventDrivenOrderProcessor.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaChoreography.Stock.Api.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<PaymentFailedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            var stocks = await _appDbContext.Stocks.Where(x=> context.Message.OrderedItems.Select(y=>y.ProductId).ToList().Contains(x.ProductId)).ToListAsync();
            foreach (var orderedItem in context.Message.OrderedItems)
            {
                var stock = stocks.FirstOrDefault(x => x.ProductId == orderedItem.ProductId);
                if (stock == null)
                {
                    _logger.LogCritical($"Stock not found for ProductId:{orderedItem.ProductId}");
                }
                stock.Count += orderedItem.Count;
            }
            _appDbContext.UpdateRange(stocks);
            await _appDbContext.SaveChangesAsync();

            _logger.LogInformation($"Stock updated after payment fail for OrderId:{context.Message.OrderId}");
        }
    }
}
