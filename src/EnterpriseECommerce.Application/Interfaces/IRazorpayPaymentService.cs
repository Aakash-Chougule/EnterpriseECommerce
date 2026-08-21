using EnterpriseECommerce.Application.DTOs;

namespace EnterpriseECommerce.Application.Interfaces;

public interface IRazorpayPaymentService
{
    Task<RazorpayOrderDto>
        CreateOrderAsync(
            Guid userId,
            Guid paymentId,
            CancellationToken cancellationToken = default);

    Task<PaymentDto>
        VerifyPaymentAsync(
            Guid userId,
            Guid paymentId,
            VerifyRazorpayPaymentRequest request,
            CancellationToken cancellationToken = default);
}