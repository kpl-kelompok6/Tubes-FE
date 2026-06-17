using KPL_FE.Models;

namespace KPL_FE.Controllers;

public sealed class PaymentApiController
{
    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        => await App.Api.PostAsync<PaymentResponse>("payments", request);
}
