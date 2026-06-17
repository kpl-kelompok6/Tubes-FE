using KPL_FE.Models;

namespace KPL_FE.Controllers;

public sealed class AuthApiController
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
        => await App.Api.PostAsync<LoginResponse>("auth/login", request);

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        => await App.Api.PostAsync<LoginResponse>("auth/register", request);
}
