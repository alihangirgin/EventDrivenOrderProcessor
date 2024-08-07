using System.ComponentModel.DataAnnotations.Schema;

namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public int ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Count { get; set; }
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}
