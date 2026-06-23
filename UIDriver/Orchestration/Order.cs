using FlaUI.Core.AutomationElements;

namespace UIDriver;

public enum OrderStatus
{
    Pending,
    Completed
}

public sealed class Order
{
    public required BY By { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Task<AutomationElement>? Task { get; set; }
}
