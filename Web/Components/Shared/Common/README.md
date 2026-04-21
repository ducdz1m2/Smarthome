# Shared Common Components

This directory contains reusable UI components to improve maintainability and reduce code duplication across the Smarthome Web application.

## Components

### DataTable.razor
A reusable table component that wraps MudBlazor's MudTable with common features like:
- Built-in search functionality
- Configurable toolbar with title and filters
- Standardized pagination
- Consistent styling (dense, hover, striped, bordered)

**Usage:**
```razor
<DataTable Items="_items" Title="Danh sách" Loading="_loading" FilterFunc="@FilterItems">
    <HeaderContent>
        <!-- Column headers -->
    </HeaderContent>
    <RowTemplate Context="item">
        <!-- Row content -->
    </RowTemplate>
</DataTable>
```

### StatusSwitch.razor
A reusable status toggle switch with:
- Automatic status toggle handling
- Color coding (success/error)
- Event callbacks for status changes

**Usage:**
```razor
<StatusSwitch IsActive="@item.IsActive" ItemId="@item.Id" OnToggle="@ToggleStatus" />
```

### ActionButtons.razor
A reusable action button component that provides:
- Edit, Add, View, Delete, Cancel buttons
- Configurable visibility for each button
- Disabled state support
- Custom button support via ChildContent

**Usage:**
```razor
<ActionButtons ShowEdit="true" ShowDelete="true" DeleteDisabled="@condition"
              OnEdit="@(() => OpenEditDialog(item))" 
              OnDelete="@(() => DeleteItem(item))" />
```

### FilterBar.razor
A reusable filter bar container using MudGrid for:
- Consistent filter layout
- Responsive design
- Easy filter component placement

**Usage:**
```razor
<FilterBar>
    <MudItem xs="12" sm="6" md="3">
        <!-- Filter controls -->
    </MudItem>
</FilterBar>
```

### PageHeader.razor
A reusable page header component with:
- Title display
- Optional "Add" button
- Consistent styling

**Usage:**
```razor
<PageHeader Title="Quản lý Sản phẩm" AddButtonText="Thêm sản phẩm" OnAddClick="@OpenAddDialog" />
```

### CrudPageBase.cs
A base class for CRUD pages that provides:
- Common data loading logic
- Dialog opening helpers
- Delete confirmation helpers
- Standardized error handling with Snackbar

**Usage:**
```csharp
public class BrandsIndex : CrudPageBase<BrandResponse, BrandDialog>
{
    protected override async Task LoadDataAsync()
    {
        Items = await BrandService.GetAllAsync();
    }
}
```

## Benefits

1. **Reduced Code Duplication**: Common patterns extracted into reusable components
2. **Consistent UI**: All pages use the same component patterns
3. **Easier Maintenance**: Changes to common patterns only need to be made in one place
4. **Faster Development**: New pages can be built quickly using existing components
5. **Better Testability**: Individual components can be tested independently

## Migration Guide

When creating new admin pages:

1. Use `PageHeader` for the page title and add button
2. Use `FilterBar` for filter controls (if needed)
3. Use `DataTable` for the main data table
4. Use `StatusSwitch` for status toggles
5. Use `ActionButtons` for action buttons
6. Consider inheriting from `CrudPageBase` for common CRUD logic

## Example: Refactored Brands Page

**Before (182 lines):**
- Repeated MudTable markup
- Inline status switch logic
- Inline action button markup
- Redundant OnToggleStatus method

**After (~130 lines):**
- Uses PageHeader, DataTable, StatusSwitch, ActionButtons
- Cleaner, more readable code
- Reduced by ~30% lines
- Easier to maintain

## Future Enhancements

- Add more specialized table column components (ImageColumn, DateColumn, etc.)
- Create form input components for common patterns
- Add pagination state management
- Create specialized dialog base classes
