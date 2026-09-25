using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using UIDriver.Diagnostics;

namespace UIDriver.Api;

public sealed class DriverOptions
{
    public IReadOnlyList<ITreeObserver> TreeObservers { get; init; } = [];
    public IReadOnlyList<IBranchObserver> BranchObservers { get; init; } = [];
    public ILoggerFactory LoggerFactory { get; init; } = NullLoggerFactory.Instance;
}
