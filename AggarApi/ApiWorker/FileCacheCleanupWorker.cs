using ApiWorker.Models;
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWorker
{
    public class FileCacheCleanupWorker : BackgroundService
    {
        private readonly ILogger<FileCacheCleanupWorker> _logger;
        private readonly IConfiguration _configuration;

        public FileCacheCleanupWorker(ILogger<FileCacheCleanupWorker> logger, IConfiguration configuration)
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
                    await DeleteExpiredFiles(connectionString);
                var delay = TimeSpan.FromDays(1);
                await Task.Delay(delay, stoppingToken); ;//run script 1 time a day
            }
        }
        private async Task DeleteExpiredFiles(string constr)
        {
            using var connection = new SqlConnection(constr);

            var querySql = "select * from filecache where ExpiresOn<CURRENT_TIMESTAMP";

            var files = await connection.QueryAsync<FileCache>(querySql);
            if(files == null)
                return;

            foreach (var file in files)
            {
                if (File.Exists(file.Path))
                    File.Delete(file.Path);
            }
            var deleteSql = "delete from filecache where id in @Ids";
            await connection.ExecuteAsync(deleteSql, new { Ids = files.Select(f => f.Id) });
        }

    }
}
