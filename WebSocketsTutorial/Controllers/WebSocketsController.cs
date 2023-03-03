using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebSocketsTutorial.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketsController : ControllerBase
    {
        private new const int BadRequest = ((int)HttpStatusCode.BadRequest);
        private readonly ILogger<WebSocketsController> _logger;

        public WebSocketsController(ILogger<WebSocketsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/ws")]
        public async Task Get()
        {
          if (HttpContext.WebSockets.IsWebSocketRequest)
          {
              using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
              _logger.Log(LogLevel.Information, "WebSocket connection established");
              await Echo(webSocket);
          }
          else
          {
              HttpContext.Response.StatusCode = BadRequest;
          }
        }
        
        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _logger.Log(LogLevel.Information, "Message received from Client");

            while (!result.CloseStatus.HasValue)
            {
                var serverMsg = Encoding.UTF8.GetBytes($"Server: Hello. You said: {Encoding.UTF8.GetString(buffer)}");
                await webSocket.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message sent to Client");

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _logger.Log(LogLevel.Information, "Message received from Client");
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _logger.Log(LogLevel.Information, "WebSocket connection closed");
        }
    }
}
