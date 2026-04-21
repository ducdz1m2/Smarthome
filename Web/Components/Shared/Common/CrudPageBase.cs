using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Web.Components.Shared.Common;

public class CrudPageBase<TItem, TDialog> : ComponentBase where TDialog : ComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IDialogService DialogService { get; set; } = null!;

    protected List<TItem> Items { get; set; } = new();
    protected bool Loading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadItemsAsync();
    }

    protected virtual async Task LoadItemsAsync()
    {
        Loading = true;
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Lỗi khi tải dữ liệu: {ex.Message}", Severity.Error);
        }
        finally
        {
            Loading = false;
        }
    }

    protected virtual Task LoadDataAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task OpenDialogAsync(string title, DialogParameters<TDialog> parameters, DialogOptions? options = null)
    {
        var dialogOptions = options ?? new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.Small, FullWidth = true };
        var dialog = await DialogService.ShowAsync<TDialog>(title, parameters, dialogOptions);
        var result = await dialog.Result;

        if (!result.Canceled)
        {
            await LoadItemsAsync();
        }
    }

    protected async Task ConfirmDeleteAsync(string itemName, Func<Task> deleteAction)
    {
        var parameters = new DialogParameters
        {
            { "Title", "Xác nhận xóa" },
            { "Content", $"Bạn có chắc muốn xóa \"{itemName}\"?" },
            { "ConfirmText", "Xóa" },
            { "CancelText", "Hủy" }
        };
        var options = new DialogOptions { CloseOnEscapeKey = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await DialogService.ShowAsync<ConfirmDialog>("", parameters, options);
        var result = await dialog.Result;

        if (result is not null && !result.Canceled && result.Data is true)
        {
            try
            {
                await deleteAction();
                Snackbar.Add("Đã xóa thành công", Severity.Success);
                await LoadItemsAsync();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Lỗi khi xóa: {ex.Message}", Severity.Error);
            }
        }
    }
}
