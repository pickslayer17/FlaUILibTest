using System.Collections.Concurrent;
using FlaUI.Core.AutomationElements;

namespace UIDriver;

public sealed class ApplicationManager
{
    private readonly ConcurrentDictionary<string, WindowContainer> _containers = new(); // todo: добавить объект runTimeId с методом toString, который хранит реальный intp[] и в equals сравнивает по содержимому int[], и hashCode тоже чтобы правильный был
    private readonly ConcurrentDictionary<Guid, Order> _orders = new(); 

    private WindowContainer? _default;
    private WindowContainer? _desktop;

    public Task<AutomationElement> Submit(BY by) => throw new NotImplementedException();
}
