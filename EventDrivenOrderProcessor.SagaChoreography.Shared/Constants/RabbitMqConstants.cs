using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDrivenOrderProcessor.Shared.Constants
{
    public static class RabbitMqConstants
    {
        public const string OrderPaymentFailedEventQueueName = "order-payment-failed-queue";
        public const string OrderPaymentSuccessfulEventQueueName = "order-payment-successful-queue";
        public const string OrderStockNotUpdatedEventQueueName = "order-stock-not-updated-queue";

        public const string PaymentStockUpdatedEventQueueName = "payment-stock-updated-queue";

        public const string StockOrderCreatedEventQueueName = "stock-order-created-queue";
        public const string StockPaymentFailedEventQueueName = "stock-payment-failed-queue";

        public const string OrderQueue = "order-queue";

    }
}
