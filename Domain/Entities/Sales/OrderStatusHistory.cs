namespace Domain.Entities.Sales
{
    public record OrderStatusHistory(
        string Status,
        string Note,
        DateTime At
    );
}
