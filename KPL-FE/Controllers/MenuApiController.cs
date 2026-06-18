using KPL_FE.Models;

namespace KPL_FE.Controllers;

public sealed class MenuApiController
{
    public async Task<List<MenuDto>> GetAllAsync()
        => await App.Api.GetAsync<List<MenuDto>>("menus") ?? [];

    public async Task<MenuDto> AddAsync(MenuRequest request)
        => await App.Api.PostAsync<MenuDto>("menus", request);

    public async Task<MenuDto> UpdateAsync(int id, MenuRequest request)
        => await App.Api.PutAsync<MenuDto>($"menus/{id}", request);

    public async Task DeleteAsync(int id)
        => await App.Api.DeleteAsync($"menus/{id}");
}
