namespace UIDriver;

public static class LogEventFactory
{
    public static void RaiseOrderCreated(Guid orderId, BY by) => LogEventHandler.Handle(new OrderCreated(orderId, by));

    public static void RaiseElementResolved(Guid orderId) => LogEventHandler.Handle(new ElementResolved(orderId));

    public static void RaiseWindowOpened(RunTimeId window) => LogEventHandler.Handle(new WindowOpened(window));

    public static void RaiseWindowClosed(RunTimeId window) => LogEventHandler.Handle(new WindowClosed(window));
}
