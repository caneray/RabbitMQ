using System;
using System.Text;
using RabbitMQ.Client;

namespace Send
{
    class Program
    {
        static void Main(string[] args)
        {
            var sendApp = new Send();
            sendApp.Start();
        }
    }
}