namespace BetTracker.Core.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
}
