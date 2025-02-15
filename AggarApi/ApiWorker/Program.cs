namespace ApiWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            //builder.Services.AddHostedService<Worker>();
            builder.Services.AddHostedService(serviceProvider =>
                new RefreshTokenCleanupWorker(serviceProvider.GetService<ILogger<RefreshTokenCleanupWorker>>(), serviceProvider.GetService<IConfiguration>()));

            builder.Services.AddHostedService(serviceProvider =>
                new FileCacheCleanupWorker(serviceProvider.GetService<ILogger<FileCacheCleanupWorker>>(), serviceProvider.GetService<IConfiguration>()));

            var host = builder.Build();
            host.Run();
        }
    }
}