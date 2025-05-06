using RefactorThis.Persistence;

namespace RefactorThis.Domain.PaymentProcessors
{
    public interface IPaymentProcessor
    {
        void ConfirmPayment(Invoice invoice, Payment payment);
    }
}
