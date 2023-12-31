using System.Text;
using MQTTnet;
using MQTTnet.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer("localhost", 1883)  
            .Build();

        await client.ConnectAsync(options);

        client.ApplicationMessageReceivedAsync += (async e =>
        {
            Console.WriteLine($"Nova mensagem recebida: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        });

        await client.SubscribeAsync("sexo");

        while (true)
        {
            Console.Write("Digite a mensagem para enviar ou pressione enter para sair: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                break; 
            }

            var message = new MqttApplicationMessage
            {
                Topic = "sexo",
                Payload = Encoding.UTF8.GetBytes(input),
                QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,
                Retain = false
            };

            await client.PublishAsync(message);
        }

        await client.DisconnectAsync();
    }
}
