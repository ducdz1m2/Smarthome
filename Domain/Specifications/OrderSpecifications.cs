using Domain.Entities.Sales;
using Domain.Enums;

namespace Domain.Specifications;

/// <summary>
/// Specification for orders by user.
/// </summary>
public class OrdersByUserSpecification : BaseSpecification<Order>
{
    public OrdersByUserSpecification(int userId)
    {
        Criteria = o => o.UserId == userId;
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders by status.
/// </summary>
public class OrdersByStatusSpecification : BaseSpecification<Order>
{
    public OrdersByStatusSpecification(OrderStatus status)
    {
        Criteria = o => o.Status == status;
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for pending orders.
/// </summary>
public class PendingOrdersSpecification : BaseSpecification<Order>
{
    public PendingOrdersSpecification()
    {
        Criteria = o => o.Status == OrderStatus.Pending;
        AddInclude(o => o.Items);
        ApplyOrderBy(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for orders requiring installation.
/// </summary>
public class OrdersRequiringInstallationSpecification : BaseSpecification<Order>
{
    public OrdersRequiringInstallationSpecification()
    {
        Criteria = o => o.Status == OrderStatus.AwaitingSchedule ||
                        o.Status == OrderStatus.Installing;
        AddInclude(o => o.Items);
        AddInclude("Items.Product");
    }
}

/// <summary>
/// Specification for orders in date range.
/// </summary>
public class OrdersByDateRangeSpecification : BaseSpecification<Order>
{
    public OrdersByDateRangeSpecification(DateTime fromDate, DateTime toDate)
    {
        Criteria = o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate;
        AddInclude(o => o.Items);
        ApplyOrderByDescending(o => o.CreatedAt);
    }
}

/// <summary>
/// Specification for overdue orders.
/// </summary>
public class OverdueOrdersSpecification : BaseSpecification<Order>
{
    public OverdueOrdersSpecification(TimeSpan threshold)
    {
        var cutoff = DateTime.UtcNow - threshold;
        Criteria = o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoff;
        ApplyOrderBy(o => o.CreatedAt);
    }
}
