using RefactorThis.Persistence;
using System.Collections.Generic;


namespace RefactorThis.Domain.PaymentProcessors
{
    /// <summary>
    /// Process comercial invoice payment, including tax calculation.
    /// </summary>
    public class CommercialInvoicePaymentProcessor : IPaymentProcessor
    {
        private const decimal TaxRate = 0.14m;

        public void ConfirmPayment(Invoice invoice, Payment payment)
        {
            invoice.AmountPaid += payment.Amount;
            invoice.TaxAmount += payment.Amount * TaxRate;
            invoice.Payments.Add(payment);
        }
    }
}
