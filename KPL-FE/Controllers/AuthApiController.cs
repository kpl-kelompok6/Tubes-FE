using KPL_FE.Models;
using System.Threading;

namespace KPL_FE.Controllers;

public sealed class AuthApiController
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<LoginResponse>("auth/login", request, cancellationToken);

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<LoginResponse>("auth/register", request, cancellationToken);
}
