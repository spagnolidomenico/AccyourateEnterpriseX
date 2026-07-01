namespace Accyourate.Core.Platform;

public sealed class SystemClock : IClock
{
    public DateTime Now => DateTime.Now;
}
