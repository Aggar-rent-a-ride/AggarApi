using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiWorker
{
    public class RefreshTokenCleanupWorker : BackgroundService
    {
        private readonly ILogger<RefreshTokenCleanupWorker> _logger;
        private readonly IConfiguration _configuration;

        public RefreshTokenCleanupWorker(ILogger<RefreshTokenCleanupWorker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var connectionString = _configuration.GetConnectionString("default");
                if (connectionString != null)
                    DeleteRevokedOrExpiredRefreshTokens(connectionString);
                var delay = TimeSpan.FromDays(1);
                await Task.Delay(delay, stoppingToken); ;//run script 1 time a day
            }
        }
        private async Task DeleteRevokedOrExpiredRefreshTokens(string constr)
        {
            using var connection = new SqlConnection(constr);

            var sql = "delete from RefreshToken where RevokedOn is not null or ExpiresOn<GETUTCDATE()";

            await connection.ExecuteAsync(sql);
        }
    }
}
