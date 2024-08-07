using EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model;
using EventDrivenOrderProcessor.Shared;
using EventDrivenOrderProcessor.Shared.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<IPaymentFailedToAllRequestEvent>
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext appDbContext, ILogger<PaymentFailedEventConsumer> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<IPaymentFailedToAllRequestEvent> context)
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
