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
            // ����һ���Զ���ķ���������WebSocketͨ��
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

        // ����һ������WebSocketͨ�ŵķ���
        private async Task EchoAsync(WebSocket webSocket)
        {
            // ����һ����������СΪ4KB���ֽ�����
            var buffer = new byte[4096];

            // ѭ�����պͷ���WebSocket��Ϣ��ֱ�����ӹر�
            while (webSocket.State == WebSocketState.Open)
            {
                // ��WebSocket����һ����Ϣ��������һ��WebSocketReceiveResult����
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // �����Ϣ�����ͣ�����ǹر���Ϣ���͹ر�����
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
                else
                {
                    // ������ı����������Ϣ���Ͱ���Ϣԭ�����ͻ�ȥ
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                }
            }
        }
    
    }
}