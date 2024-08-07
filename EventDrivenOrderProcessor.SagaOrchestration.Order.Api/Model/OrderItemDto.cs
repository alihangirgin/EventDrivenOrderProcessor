namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
