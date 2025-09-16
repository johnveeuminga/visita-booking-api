using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using visita_booking_api.Services.Implementation;
using visita_booking_api.Services.Interfaces;

namespace visita_booking_api.Extensions
{
    public static class SqsServiceCollectionExtensions
    {
        public static IServiceCollection AddSqsPaymentConsumer(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind options
            services.Configure<SqsOptions>(configuration.GetSection("Sqs"));

            // Register AWS credentials from environment or config
            var awsSection = configuration.GetSection("AWS");
            var region = awsSection.GetValue<string>("Region") ?? "ap-southeast-1";

            // Use default AWS credentials chain (Env, Shared config, EC2/ECS role)
            services.AddSingleton<IAmazonSQS>(sp =>
            {
                var creds = FallbackCredentialsFactory.GetCredentials();
                var config = new AmazonSQSConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
                return new AmazonSQSClient(creds, config);
            });

            // Hosted service will be added by consumer
            services.AddHostedService<SqsPaymentConsumerService>();

            // Register default payment event handler implementation
            services.AddScoped<IPaymentEventHandler, SqsPaymentEventHandler>();

            return services;
        }
    }
}
