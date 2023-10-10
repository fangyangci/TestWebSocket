using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebSocketClient
{
    class Program
    {
        private static string domain = "cfy-appservice.azurewebsites.net";

        static async Task Main(string[] args)
        {
            // 创建一个ClientWebSocket对象
            using (var ws = new ClientWebSocket())
            {
                // 连接到一个WebSocket服务器的地址，这里是一个示例地址，您可以替换成您想要连接的服务器地址
                var uri = new Uri($"wss://{domain}/connect");
                await ws.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("Connected to " + uri);

                _ = Task.Run(() => ReceiveMsg(ws));

                // 发送一条文本消息给服务器，这里是一个示例消息，您可以替换成您想要发送的内容
                var message = "Hello WebSocket!";
                await SendMsgAsync(ws, message);
                while (true)
                {
                    var str = Console.ReadLine();
                    if (!string.IsNullOrEmpty(str))
                    {
                        await SendMsgAsync(ws, str);
                    }                    
                }
            }
        }

        private static async Task SendMsgAsync(ClientWebSocket ws, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"{DateTime.UtcNow} Sent: " + message);
        }

        private static async Task ReceiveMsg(ClientWebSocket ws)
        {
            while (true)
            {
                // 接收服务器的回复消息，这里假设服务器会把收到的消息原样返回
                var buffer = new byte[4096];
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var reply = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"{DateTime.UtcNow} Received: " + reply);
            }
        }
    }
}
