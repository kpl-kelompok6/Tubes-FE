using KPL_FE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class PaymentApiController
{
    public async Task<List<PaymentResponse>> GetAllPaymentsAsync()
        => await App.Api.GetAsync<List<PaymentResponse>>("payments") ?? [];

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        => await App.Api.PostAsync<PaymentResponse>("payments", request);
}
