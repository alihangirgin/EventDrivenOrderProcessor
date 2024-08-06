using System.Collections;

namespace EventDrivenOrderProcessor.SagaChoreography.Order.Api.Model
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public string BuyerId { get; set; }
        public Address Address { get; set; }
        public virtual ICollection<OrderItem> OrderedItems { get; set; }
        public string? FailMessage { get; set; }
    }

    public enum OrderStatus : byte
    {
        Pending,
        Completed,
        Failed
    }
}
