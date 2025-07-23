using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Send
{
    public class Send
    {
        public void Start()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            // Kuyruk oluşturma
            channel.QueueDeclare(queue: "queue_A", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "queue_B", durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Gelen mesajları dinleyen yapı
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("\n[Receiver]: " + message);
                channel.BasicAck(ea.DeliveryTag, false);
            };

            // queue_B'den gelen mesajları dinle
            channel.BasicConsume(queue: "queue_B", autoAck: false, consumer: consumer);

            // Konsoldan mesaj gönderme
            Console.WriteLine("Receiver'a mesaj yaz ve Enter'a bas (çıkmak için 'exit' yaz):");
            string input;
            while ((input = Console.ReadLine()) != "exit")
            {
                var body = Encoding.UTF8.GetBytes(input);
                channel.BasicPublish(exchange: "", routingKey: "queue_A", basicProperties: null, body: body);
                Console.WriteLine("[Sen]: " + input);
            }

            Console.WriteLine("Send uygulaması kapatıldı.");
        }
    }
}
