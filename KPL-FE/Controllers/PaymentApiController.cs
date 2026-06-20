using KPL_FE.Models;
using System.Threading;
using System.Threading.Tasks;

namespace KPL_FE.Controllers;

public sealed class PaymentApiController
{
    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<PaymentResponse>("payments", request, cancellationToken);
}
