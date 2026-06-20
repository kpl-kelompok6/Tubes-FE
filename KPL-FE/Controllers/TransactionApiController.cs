using KPL_FE.Models;
using System.Threading;

namespace KPL_FE.Controllers;

public sealed class TransactionApiController
{
    public async Task<List<TransactionDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await App.Api.GetAsync<List<TransactionDto>>("transactions", cancellationToken) ?? [];

    public async Task<TransactionDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await App.Api.GetAsync<TransactionDto>($"transactions/{id}", cancellationToken);

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<TransactionDto>("transactions", request, cancellationToken);

    public async Task<TransactionDto> AddItemAsync(int transactionId, AddItemRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PostAsync<TransactionDto>($"transactions/{transactionId}/items", request, cancellationToken);

    public async Task<TransactionDto> RemoveItemAsync(int transactionId, int itemId, CancellationToken cancellationToken = default)
        => await App.Api.DeleteAsync<TransactionDto>($"transactions/{transactionId}/items/{itemId}", cancellationToken);

    public async Task<TransactionDto> UpdateItemQuantityAsync(int transactionId, int itemId, UpdateItemRequest request, CancellationToken cancellationToken = default)
        => await App.Api.PatchAsync<TransactionDto>($"transactions/{transactionId}/items/{itemId}", request, cancellationToken);

    public async Task CancelAsync(int transactionId, CancellationToken cancellationToken = default)
        => await App.Api.DeleteAsync($"transactions/{transactionId}", cancellationToken);
}
