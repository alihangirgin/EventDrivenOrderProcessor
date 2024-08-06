namespace EventDrivenOrderProcessor.SagaChoreography.Stock.Api.Model
{
    public class Stock
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        public int Count { get; set; }
    }
}
