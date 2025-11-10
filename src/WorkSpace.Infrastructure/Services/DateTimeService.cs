using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime NowUtc => DateTime.UtcNow;
}