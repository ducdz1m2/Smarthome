using Application.DTOs.Responses;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Components.Dialogs
{
    public partial class WarehouseStockDetailsDialog : ComponentBase
    {
        [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public string ProductName { get; set; } = string.Empty;
        [Parameter] public string Sku { get; set; } = string.Empty;
        [Parameter] public int ProductId { get; set; }
        [Parameter] public List<WarehouseStockDetailResponse> WarehouseStocks { get; set; } = new();

        [Inject] private IDialogService DialogService { get; set; } = default!;

        private async Task OpenTransferDialog(WarehouseStockDetailResponse warehouseStock)
        {
            var parameters = new DialogParameters<WarehouseTransferDialog>();
            parameters.Add(x => x.FromWarehouseId, warehouseStock.WarehouseId);
            parameters.Add(x => x.SelectedProductId, ProductId);
            
            var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Medium, FullWidth = true };
            var dialog = await DialogService.ShowAsync<WarehouseTransferDialog>("Chuyển kho", parameters, options);
            var result = await dialog.Result;

            if (result is { Canceled: false })
            {
                MudDialog.Close(DialogResult.Ok(true));
            }
        }

        private void Close() => MudDialog.Close();
    }
}
