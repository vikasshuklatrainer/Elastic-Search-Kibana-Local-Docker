using System;
using Serilog;

class Program
{
    static void Main(string[] args)
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.Console()
            .WriteTo.Elasticsearch(new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "microservice-logs-{0:yyyy.MM.dd}"
               //ModifyConnectionSettings = x => x
                    //.BasicAuthentication("elastic", "")
                    //.ServerCertificateValidationCallback((o, cert, chain, errors) => true)
            })
            .CreateLogger();

        Console.WriteLine("Sending enterprise logs to Elasticsearch...");

        var services = new[] { "PaymentService", "OrderService", "AuthService" };
        var random = new Random();

        for (int i = 0; i <100; i++)
        {
            var service = services[random.Next(services.Length)];
            var levelError = random.Next(10) > 7;

            if (levelError)
            {
                Log.Error("Service {ServiceName} failed for User {UserId} ResponseTime {ResponseTimeMs}",
                    service,
                    $"user-{random.Next(1,10)}",
                    random.Next(50,800));
            }
            else
            {
                Log.Information("Service {ServiceName} processed request for User {UserId} ResponseTime {ResponseTimeMs}",
                    service,
                    $"user-{random.Next(1,10)}",
                    random.Next(50,800));
            }
        }

        Log.CloseAndFlush();
        Console.WriteLine("Logs sent successfully!");
    }
}
