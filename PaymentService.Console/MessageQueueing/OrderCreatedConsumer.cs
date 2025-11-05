using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace PaymentService.Console.MessageQueueing;

public class OrderCreatedConsumer
    (string hostName,
     string queueName,
     PaymentProcessor paymentProcessor)
    : IHostedService
{
    private readonly PaymentProcessor paymentProcessor = paymentProcessor;
    private readonly string hostName = hostName;
    private readonly string queueName = queueName;
    private IConnection? connection;
    private IChannel? channel;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory { HostName = hostName };

        var isDisconnected = true;
        while (isDisconnected)
        {
            try
            {
                connection = await factory
                    .CreateConnectionAsync(cancellationToken);

                isDisconnected = false;
            }
            catch (BrokerUnreachableException ex)
            {
                var msg =
                    $"RabbitMQ exception. {ex.Message}. Attempting again ...";
                System.Console.WriteLine(msg);
                Thread.Sleep(3000);
            }
        }

        channel = await connection
            .CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var id = new Guid(message);
            paymentProcessor.ProcessPayment(id).Wait();

            return Task.CompletedTask;
        };

        await channel.BasicConsumeAsync(
            queue: queueName,
            autoAck: true,
            consumer: consumer,
            cancellationToken: cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (channel != null)
        {
            await channel.CloseAsync();
            await channel.DisposeAsync();
        }

        if (connection != null)
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
    }
}