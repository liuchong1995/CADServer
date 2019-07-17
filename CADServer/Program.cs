using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CADServer
{
    class Program
    {
        /// <summary>
        /// 连接配置
        /// </summary>
        private static readonly ConnectionFactory RabbitMqFactory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "admin",
            Password = "1234",
            Port = 5672
        };
        /// <summary>
        /// 路由名称
        /// </summary>
        const string ExchangeName = "jidong.ps2cad";

        //队列名称
        const string ToPsQueueName = "cadResult";

        /// <summary>
        /// 路由名称
        /// </summary>
        const string TopExchangeName = "jidong.ps2cad";

        //队列名称
        const string TopQueueName = "orderNum";

        static void Main(string[] args)
        {
            TopicAcceptExchange();
            Console.WriteLine("按任意值，退出程序");
            Console.WriteLine("按任意值，退出程序");
            Console.ReadKey();
        }

        public static void TopicAcceptExchange()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(TopExchangeName, "topic", durable: true);
                    channel.QueueDeclare(TopQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                    channel.QueueBind(TopQueueName, TopExchangeName, routingKey: TopQueueName);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var msgBody = Encoding.UTF8.GetString(ea.Body);
                        Console.WriteLine(string.Format("***接收时间:{0}，消息内容：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msgBody));
                        int dots = msgBody.Split('.').Length - 1;
                        System.Threading.Thread.Sleep(5000);
                        Console.WriteLine(" [----] Done");
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        TopicExchangeSendMsg();
                    };
                    channel.BasicConsume(TopQueueName, noAck: false, consumer: consumer);

                    Console.WriteLine("按任意值，退出程序");
                    Console.ReadKey();
                }
            }
        }

        public static void TopicExchangeSendMsg()
        {
            using (IConnection conn = RabbitMqFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    channel.ExchangeDeclare(TopExchangeName, "topic", durable: true);
                    channel.QueueDeclare(ToPsQueueName, durable: true, autoDelete: false, exclusive: false, arguments: null);
                    channel.QueueBind(ToPsQueueName, TopExchangeName, routingKey: ToPsQueueName);
                    channel.ConfirmSelect();
                    
                    string vadata = "生成CAD模型成功......";
                    var msgBody = Encoding.UTF8.GetBytes(vadata);
                    channel.BasicPublish(exchange: TopExchangeName, routingKey: ToPsQueueName, basicProperties: null, body: msgBody);
                    Console.WriteLine(string.Format("***发送时间:{0}，发送完成，", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
            }
        }
    }
}
