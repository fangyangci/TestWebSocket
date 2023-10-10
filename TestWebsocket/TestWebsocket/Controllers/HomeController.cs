using System;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace TestWebsocket.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private static HashSet<WebSocket> _WebSockets = new HashSet<WebSocket>();

        [Route("")]
        public IActionResult Get()
        {
            return Content("llcfy");
        }

        [Route("Send")]
        public async Task<IActionResult> Send(string msg)
        {
            var messageBytes = Encoding.UTF8.GetBytes(msg);
            foreach (WebSocket ws in _WebSockets)
            {
                await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            return Content("llcfy succeed");
        }

        [Route("Disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            foreach (WebSocket ws in _WebSockets)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            
            return Content("llcfy succeed");
        }

        [Route("Connect")]
        public async Task<IActionResult> Connect()
        {
            var webSocket = await Request.HttpContext.WebSockets.AcceptWebSocketAsync();
            _WebSockets.Add(webSocket);
            // 调用一个自定义的方法来处理WebSocket通信
            try
            {
                await EchoAsync(webSocket);
            }
            finally 
            {
                _WebSockets.Remove(webSocket); 
            }
            
            return Content("llcfy websocket");
        }

        // 定义一个处理WebSocket通信的方法
        private async Task EchoAsync(WebSocket webSocket)
        {
            // 定义一个缓冲区大小为4KB的字节数组
            var buffer = new byte[4096];

            // 循环接收和发送WebSocket消息，直到连接关闭
            while (webSocket.State == WebSocketState.Open)
            {
                // 从WebSocket接收一个消息，并返回一个WebSocketReceiveResult对象
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // 检查消息的类型，如果是关闭消息，就关闭连接
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
                else
                {
                    // 如果是文本或二进制消息，就把消息原样发送回去
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }
        }
    
    }
}