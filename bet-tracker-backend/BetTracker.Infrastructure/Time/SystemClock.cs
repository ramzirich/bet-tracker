using BetTracker.Core.Abstractions;

namespace BetTracker.Infrastructure.Time;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
