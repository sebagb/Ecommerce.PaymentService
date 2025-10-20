using PaymentService.Console.MessageQueueing;

namespace PaymentService.Console;

public class PaymentProcessor
    (PaymentCompleteProducer producer)
{
    private readonly PaymentCompleteProducer messageProducer = producer;
    private const string paymentSucceeded = "Succeeded";
    private const string paymentFailed = "Failed";
    private const string delimiter = ":";

    public async Task ProcessPayment(Guid id)
    {
        var randomOption = new Random().Next(0, 2);

        var paymentMessage = randomOption switch
        {
            0 => paymentSucceeded,
            1 => paymentFailed,
            _ => paymentFailed
        };

        var processMessage = $"{id}{delimiter}{paymentMessage}";

        await messageProducer.Publish(processMessage);
    }
}