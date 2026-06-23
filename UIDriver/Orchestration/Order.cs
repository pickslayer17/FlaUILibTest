using FlaUI.Core.AutomationElements;

namespace UIDriver;

public enum OrderStatus
{
    Pending,
    Completed
}

// Заявка на уровне менеджера: что попросили (BY) и её результат. AppManager и WindowManager
// держат свои списки Orders, чтобы знать, что у них сейчас в работе.
public sealed class Order
{
    public required BY By { get; init; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Task<AutomationElement>? Task { get; set; }
}
