using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Console;
using PaymentService.Console.MessageQueueing;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var isDocker =
    Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Docker";

var appSettings = isDocker
    ? "appsettings.Docker.json"
    : "appsettings.json";

var config = new ConfigurationBuilder()
    .AddJsonFile(appSettings)
    .Build();

var hostName =
    config.GetSection("MessageQueuing:HostName").Value!;
var orderQueue =
    config.GetSection("MessageQueuing:OrderCreatedQueue").Value!;
var paymentQueue =
    config.GetSection("MessageQueuing:PaymentCompletedQueue").Value!;

builder.Services.AddTransient<PaymentProcessor>();

builder.Services.AddHostedService(config =>
{
    var paymentProcessor = config.GetRequiredService<PaymentProcessor>();
    return new OrderCreatedConsumer(hostName, orderQueue, paymentProcessor);
});

builder.Services.AddSingleton(_ =>
    new PaymentCompleteProducer(hostName, paymentQueue));

var app = builder.Build();

app.Run();