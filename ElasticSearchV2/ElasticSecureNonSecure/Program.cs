using Serilog;
using Serilog.Debugging;
using System;
using System.Security.Cryptography.X509Certificates;
using static System.Net.WebRequestMethods;

namespace ElasticSecureNonSecure
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool isSecureCluster = false;
            string ELASTIC_USERNAME = string.Empty;
            string ELASTIC_PASSWORD = string.Empty;
            string elasticUrl = string.Empty;
            // Enable Serilog internal debugging (very useful)
            SelfLog.Enable(Console.Error);

            Console.WriteLine("Are you going to connect the secure elastic cluster (https enabled)? (y/n)");
            var securecluster = Console.ReadLine()?.Trim().ToLower();
            if(securecluster=="y")
            {
                isSecureCluster = true;

            }

            if(isSecureCluster)
            {
                Console.WriteLine("You have chosen to connect to a secure cluster. ");
                elasticUrl = "https://localhost:9200";
                Console.WriteLine($"ELASTIC_URL {elasticUrl}");
                Console.WriteLine("Enter Elastic user name: default is elastic");
                ELASTIC_USERNAME = Console.ReadLine()?.Trim() ?? "elastic";
                Console.WriteLine("Enter Elastic password:");
                ELASTIC_PASSWORD = Console.ReadLine()?.Trim();

                Console.WriteLine($"ELASTIC_USERNAME : {ELASTIC_USERNAME}");
                Console.WriteLine($"ELASTIC_PASSWORD : {ELASTIC_PASSWORD}");
            }
            else
            {
                Console.WriteLine("You have chosen to connect to an unsecure cluster. Please set the following environment variable:");
                elasticUrl = "http://localhost:9200";
                Console.WriteLine($"ELASTIC_URL {elasticUrl}");
            }

           

            Console.WriteLine($"Connecting to Elasticsearch at: {elasticUrl}");

            var sinkOptions = new Serilog.Sinks.Elasticsearch.ElasticsearchSinkOptions(new Uri(elasticUrl))
            {
                AutoRegisterTemplate = true,
                IndexFormat = "microservice-logs-{0:yyyy.MM.dd}"
            };

            // 👉 If username exists → assume SECURE cluster
            if (isSecureCluster)
            {
                Console.WriteLine("Secure mode enabled");

                sinkOptions.ModifyConnectionSettings = x => x
                    .BasicAuthentication(ELASTIC_USERNAME, ELASTIC_PASSWORD)
                    .ServerCertificateValidationCallback((o, cert, chain, errors) => true);
            }
            else
            {
                Console.WriteLine("Unsecure mode enabled");
            }

            Log.Logger = new LoggerConfiguration()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(sinkOptions)
                .CreateLogger();

            Console.WriteLine("Sending enterprise logs...");



            var services = new[] { "PaymentService", "OrderService", "AuthService" };
            var random = new Random();

            for (int i = 0; i < 500; i++)
            {
                var service = services[random.Next(services.Length)];
                var levelError = random.Next(10) > 7;

                if (levelError)
                {
                    Log.Error("Service {ServiceName} failed for User {UserId} ResponseTime {ResponseTimeMs}",
                        service,
                        $"user-{random.Next(1, 10)}",
                        random.Next(50, 800));
                }
                else
                {
                    Log.Information("Service {ServiceName} processed request for User {UserId} ResponseTime {ResponseTimeMs}",
                        service,
                        $"user-{random.Next(1, 10)}",
                        random.Next(50, 800));
                }
            }

            Log.CloseAndFlush();
            Console.WriteLine("Logs sent successfully!");
        }
    }
}
