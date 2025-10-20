using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentService.Console;
using PaymentService.Console.MessageQueueing;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var hostName =
    builder.Configuration["MessageQueuing:HostName"]!;
var orderQueue =
    builder.Configuration["MessageQueuing:OrderCreatedQueue"]!;
var paymentQueue =
    builder.Configuration["MessageQueuing:PaymentCompletedQueue"]!;

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