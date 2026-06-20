using KPL_FE.Models;
using System.Threading;

namespace KPL_FE.Controllers;

public sealed class MenuApiController
{
    public async Task<List<MenuDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await App.Api.GetAsync<List<MenuDto>>("menus", cancellationToken) ?? [];

    public async Task<MenuDto> AddAsync(MenuRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<MenuDto>("menus", request, cancellationToken);

    public async Task<MenuDto> UpdateAsync(int id, MenuRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PutAsync<MenuDto>($"menus/{id}", request, cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        => await App.Api.DeleteAsync($"menus/{id}", cancellationToken);
}
