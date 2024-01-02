using System;
using System.Text;
using System.Threading.Tasks;
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
            .WithCleanSession(false)
            .WithClientId("Publisher")
            .WithCredentials("publisher", "123")
            .Build();

        await client.ConnectAsync(options);

        client.ApplicationMessageReceivedAsync += (async e =>
        {
            Console.WriteLine($"Nova mensagem recebida no tópico '{e.ApplicationMessage.Topic}': {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        });

        while (true)
        {
            Console.WriteLine("Escolha uma opção:");
            Console.WriteLine("1 - Publicar mensagem");
            Console.WriteLine("2 - Subscrever a um tópico");
            Console.WriteLine("3 - Reconectar ao broker");
            Console.WriteLine("4 - Sair");
            Console.Write("Opção: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.Write("Digite o tópico para publicar a mensagem: ");
                    var publishTopic = Console.ReadLine();

                    Console.Write("Digite a mensagem para enviar: ");
                    var publishInput = Console.ReadLine();

                    var publishMessage = new MqttApplicationMessage
                    {
                        Topic = publishTopic,
                        Payload = Encoding.UTF8.GetBytes(publishInput),
                        QualityOfServiceLevel = MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce,
                        Retain = true
                    };

                    await client.PublishAsync(publishMessage);
                    break;

                case "2":
                    Console.Write("Digite o tópico para se inscrever: ");
                    var subscribeTopic = Console.ReadLine();

                    await client.SubscribeAsync(subscribeTopic);
                    Console.WriteLine($"Inscrito no tópico '{subscribeTopic}'");
                    break;

                case "3":
                    await Reconnect(client, options);
                    break;
                        
                case "4":
                    await client.DisconnectAsync();
                    return;

                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        }
    }

    static async Task ConnectAndSubscribe(IMqttClient client, MqttClientOptions options)
    {
        client.DisconnectedAsync += (async e =>
        {
            Console.WriteLine("Desconectado do broker. Tentando reconectar...");
            await Task.Delay(TimeSpan.FromSeconds(5));
            await Reconnect(client, options);
        });

        await client.ConnectAsync(options);

        client.ApplicationMessageReceivedAsync += async e =>
        {
            Console.WriteLine($"Nova mensagem recebida no tópico '{e.ApplicationMessage.Topic}': {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
        };

        Console.WriteLine("Conectado ao broker.");
    }

    static async Task Reconnect(IMqttClient client, MqttClientOptions options)
    {
        try
        {
            await client.DisconnectAsync();
            Console.WriteLine("Desconectado. Tentando reconectar...");
            await ConnectAndSubscribe(client, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao reconectar: {ex.Message}");
        }
    }

}
