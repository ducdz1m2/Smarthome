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
        [Parameter] public List<WarehouseStockDetailResponse> WarehouseStocks { get; set; } = new();

        private void Close() => MudDialog.Close();
    }
}
