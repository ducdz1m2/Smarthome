namespace Domain.Enums;

/// <summary>
/// Type of stock issue/dispatch operation.
/// </summary>
public enum StockIssueType
{
    Installation = 1,       // Issue for installation
    Warranty = 2,          // Issue for warranty repair
    Return = 3,            // Return to supplier
    Adjustment = 4,        // Stock adjustment
    Other = 99             // Other purposes
}
