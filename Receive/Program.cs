
using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            var receiverApp = new Receive();
            receiverApp.Start();
        }
    }

    
}