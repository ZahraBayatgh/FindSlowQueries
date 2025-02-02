using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace FindSlowQueries.Interceptors
{
    public class QueryInterceptor : DbCommandInterceptor
    {
        private readonly ILogger<QueryInterceptor> _logger;
        private readonly ConcurrentDictionary<DbCommand, Stopwatch> _timers = new();

        public QueryInterceptor(ILogger<QueryInterceptor> logger)
        {
            _logger = logger;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            _timers[command] = Stopwatch.StartNew();
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
        {
            await Task.Delay(500);

            var stopwatch = _timers[command];
            stopwatch.Stop();
            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                _logger.LogWarning("⚠️ Slow query: {CommandText} (Time: {ElapsedMilliseconds} ms)",
                                   command.CommandText, elapsedMilliseconds);
            }

            _timers.TryRemove(command, out _);

            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }
    }
}
