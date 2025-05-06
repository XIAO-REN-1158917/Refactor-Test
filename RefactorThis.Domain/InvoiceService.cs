using System;
using System.Linq;
using RefactorThis.Domain.PaymentProcessors;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
	{
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

        /// <summary>
        /// Processes a payment against its invoice.
        /// </summary>
        /// <param name="payment">The payment to process.</param>
        /// <returns>A string indicating the result of the payment processing.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if no invoice is found for the given payment reference.
        /// </exception>
        public string ProcessPayment(Payment payment)
        {
            var invoice = _invoiceRepository.GetInvoice(payment.Reference);
            if (invoice == null)
            {
                throw new InvalidOperationException("There is no invoice matching this payment");
            }

            var message = HandleInvoicePayment(invoice, payment);
            invoice.Save();
            return message;
        }

        private string HandleInvoicePayment(Invoice invoice, Payment payment)
        {
            if (invoice.Amount == 0)
            {
                return HandleZeroChargeInvoice(invoice);
            }

            return invoice.Payments != null && invoice.Payments.Any()
                ? HandleRemainingPayments(invoice, payment)
                : HandleFirstTimePayment(invoice, payment);
        }

        private string HandleZeroChargeInvoice(Invoice invoice)
        {
            if (invoice.Payments == null || !invoice.Payments.Any())
            {
                return "no payment needed";
            }

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        private string HandleRemainingPayments(Invoice invoice, Payment payment)
        {
            var totalPaid = invoice.Payments.Sum(x => x.Amount);
            var remaining = invoice.Amount - totalPaid;

            if (totalPaid != 0 && invoice.Amount == totalPaid)
            {
                return "invoice was already fully paid";
            }

            if (payment.Amount > remaining)
            {
                return "the payment is greater than the partial amount remaining";
            }

            GetPaymentProcessor(invoice.Type).ConfirmPayment(invoice, payment);

            return remaining == payment.Amount
                ? "final partial payment received, invoice is now fully paid"
                : "another partial payment received, still not fully paid";
        }

        private string HandleFirstTimePayment(Invoice invoice, Payment payment)
        {
            if (payment.Amount > invoice.Amount)
            {
                return "the payment is greater than the invoice amount";
            }

            GetPaymentProcessor(invoice.Type).ConfirmPayment(invoice, payment);

            return payment.Amount == invoice.Amount
                ? "invoice is now fully paid"
                : "invoice is now partially paid";
        }

        /// <summary>
        /// Get the appropriate payment processor bases on invoice type.
        /// </summary>
        /// <param name="type">Invoice type</param>
        /// <returns>An PaymentProcessor for handling payments.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if an unsupported invoice type is provided.
        /// </exception>
        private IPaymentProcessor GetPaymentProcessor(InvoiceType type)
        {
            switch (type)
            {
                case InvoiceType.Standard:
                    return new StandardInvoicePaymentProcessor();
                case InvoiceType.Commercial:
                    return new CommercialInvoicePaymentProcessor();
                default:
                    // TODO: Confirm exception is handled else where.
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }

}
