using RefactorThis.Persistence;
using System.Collections.Generic;

namespace RefactorThis.Domain.PaymentProcessors
{
    /// <summary>
    /// Process standard invoice payment.
    /// </summary>
    public class StandardInvoicePaymentProcessor : IPaymentProcessor
    {
        public void ConfirmPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.Payments.Add(payment);
        }
    }
}
