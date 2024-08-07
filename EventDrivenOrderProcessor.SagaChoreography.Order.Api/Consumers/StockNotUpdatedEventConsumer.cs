using EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model;
using EventDrivenOrderProcessor.Shared;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api.Consumers
{
    public class StockNotUpdatedEventConsumer : IConsumer<StockNotUpdatedEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<StockNotUpdatedEventConsumer> _logger;

        public StockNotUpdatedEventConsumer(AppDbContext appDbContext, ILogger<StockNotUpdatedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockNotUpdatedEvent> context)
        {
            var order = await _appDbContext.Orders.FirstOrDefaultAsync(x => x.Id == context.Message.OrderId);
            if (order == null)
            {
                _logger.LogCritical($"Order OrderId:{context.Message.OrderId} not found");
                return;
            }

            order.Status = OrderStatus.Failed;
            order.FailMessage = context.Message.FailMessage;
            _appDbContext.Orders.Update(order);
            await _appDbContext.SaveChangesAsync();

            _logger.LogInformation($"Order OrderId:{context.Message.OrderId} failed");
        }
    }
}
