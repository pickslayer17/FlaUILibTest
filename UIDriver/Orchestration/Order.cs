namespace UIDriver;

public enum OrderStatus
{
    Pending,
    Completed
}

public sealed class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public required UIBy By { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Task<UIAutomationElement>? Task { get; set; }
}
