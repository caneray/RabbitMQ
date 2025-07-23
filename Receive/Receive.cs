using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;

namespace Receive
{
    public class Receive
    {
        public void Start()
        {
            // Projenin Genel Mantığı =>
            // Send queue A üzerinden mesajları gönderir. Receiver queueA üzerinden dinler.
            // Receiver queue B üzerinden mesajları gönderir. Send queue B üzerinden mesajları dinler.

            // Burada HostName = "localhost" parametresi, RabbitMQ sunucusunun yerel makinede (localhost) olduğunu belirtiyor. Eğer RabbitMQ başka bir sunucuda çalışıyorsa, buraya o sunucunun IP adresi veya DNS adı yazılabilir.
            var factory = new ConnectionFactory() { HostName = "localhost" }; // RabbitMQ ya bir bağlantı kurmak için gerekli olan ayarlarları yapmak amacıyla kullanılır.

            // CreateConnection metodu, yukarıdaki yapılandırma ile RabbitMQ sunucusuna bir bağlantı oluşturur. Bu bağlantı, mesajlaşma işlemleri için kullanılır.
            using var connection = factory.CreateConnection();

            // RabbitMQ'ya bağlanan ve kuyruk işlemlerini yöneten bağlantı nesnesidir. Tüm mesaj gönderme ve alma işlemleri bu kanal üzerinden yapılır.
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "queue_A", durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueDeclare(queue: "queue_B", durable: false, exclusive: false, autoDelete: false, arguments: null);
            // durable: false => Geçici kuyruk. RabbitMQ yeniden başlatıldığında silinir. True olursa kalıcı demektir. Kritik mesajların kaybolmaması gerektiği durumlarda true kullanılır.
            // exclusive: false => Birden fazla uygulamanın aynı kuyruğu dinlediğinde kullanılır. True olursa yalnızca kuyruk oluşturan bağlantı (connection) tarafından kullanılabilir ve o bağlantı kapandığında otomatik olarak silinir.
            // autoDelete: false => Son tüketici bağlantıyı kapatsa bile kuyruk silinmez, kuyruk açık kalır. True olursa tüketici bağlantıyı kapattığında kuyruk otomatik olarak silinir.
            // arguments: null => Ek özellikler olarak düşünülebilir. Örneğin belirli bir süre sonra mesajların otomatik silinmesi özelliğinin eklenmesi gibi

            var consumer = new EventingBasicConsumer(channel); // RabbitMQ'dan asenkron olarak mesaj almak için kullanılır.
            consumer.Received += (model, ea) => // ea => Gelen mesajın tüm bilgilerini içerir. (body, header, delivery tag vb.)
            {
                var body = ea.Body.ToArray(); // Mesajın gövdesi (body) byte dizisi olarak gelir. ToArray ile byte dizisini okunacak hale getiririz.
                var message = Encoding.UTF8.GetString(body); // Utf8 formatında stringe çevirdik.
                Console.WriteLine("\n[Send]: " + message);
                channel.BasicAck(ea.DeliveryTag, false); 
                // BasicAck => RabbitMQ ya mesajı başarıyla aldığımızı ve işlediğimizi bildiririz. Yoksa rabbitmq sürekli olarak mesaj göndermeye çalışır.
                // ea.DeliveryTag => Mesajların benzersiz kimlikleri vardır. Hangi mesajın işlendiği bu tag üzerinden takip edilir.
                // false => Tek bir mesaj için izin verdiğimizi söylüyoruz. True olsaydı bundan önceki tüm mesajları onayladım şeklinde olurdu.
            };

            channel.BasicConsume(queue: "queue_A", autoAck: false, consumer: consumer); // queue A için dinlemeye başlar.
            // autoAck: false => Mesajı manuel olarak onaylayacağımı gösterir. True olsaydı gelen mesajı otomatik olarak onayladığımı söylerdim.

            Console.WriteLine("Send'e mesaj yaz ve Enter'a bas (çıkmak için 'exit' yaz):");
            string input;
            while ((input = Console.ReadLine()) != "exit")
            {
                var body = Encoding.UTF8.GetBytes(input);
                channel.BasicPublish(exchange: "", routingKey: "queue_B", basicProperties: null, body: body); 
                Console.WriteLine("[Sen]: " + input);
            }

            Console.WriteLine("Receiver uygulaması kapatıldı.");
        }
    }
}
