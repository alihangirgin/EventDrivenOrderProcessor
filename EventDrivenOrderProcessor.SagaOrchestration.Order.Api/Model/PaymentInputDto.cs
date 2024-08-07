namespace EventDrivenOrderProcessor.SagaOrchestration.Order.Api.Model
{
    public class PaymentInputDto
    {
        public string CardName { get; set; }
        public string CardNumber { get; set; }
        public string Expiration { get; set; }
        public string CVV { get; set; }
    }
}
