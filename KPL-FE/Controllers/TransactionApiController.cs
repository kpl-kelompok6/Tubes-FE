using KPL_FE.Models;

namespace KPL_FE.Controllers;

public sealed class TransactionApiController
{
    public async Task<List<TransactionDto>> GetAllAsync()
        => await App.Api.GetAsync<List<TransactionDto>>("transactions") ?? [];

    public async Task<TransactionDto> GetByIdAsync(int id)
        => await App.Api.GetAsync<TransactionDto>($"transactions/{id}");

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request)
        => await App.Api.PostAsync<TransactionDto>("transactions", request);

    public async Task<TransactionDto> AddItemAsync(int transactionId, AddItemRequest request)
        => await App.Api.PostAsync<TransactionDto>($"transactions/{transactionId}/items", request);

    public async Task<TransactionDto> RemoveItemAsync(int transactionId, int itemId)
        => await App.Api.DeleteAsync<TransactionDto>($"transactions/{transactionId}/items/{itemId}");

    public async Task<TransactionDto> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request)
        => await App.Api.PatchAsync<TransactionDto>($"transactions/{transactionId}/items/{itemId}", request);
}
