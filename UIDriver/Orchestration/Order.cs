namespace UIDriver;

public enum OrderStatus
{
    Pending,
    Completed
}

public sealed class Order
{
    public Guid Id { get; } = Guid.NewGuid();
    public required BY By { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Task<AutomationElementObject>? Task { get; set; }
}
