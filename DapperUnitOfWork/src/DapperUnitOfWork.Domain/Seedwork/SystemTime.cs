using System;

namespace DapperUnitOfWork.Domain.Seedwork
{
    public static class SystemTime
    {
        private static readonly Func<DateTimeOffset> DefaultProvider = () => DateTimeOffset.UtcNow;
        private static Func<DateTimeOffset> _timeProvider = DefaultProvider;

        public static DateTimeOffset Now => _timeProvider();

        public static void SetTimeProvider(Func<DateTimeOffset> timeProvider)
        {
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public static void ResetTimeProvider()
        {
            _timeProvider = DefaultProvider;
        }
    }
}