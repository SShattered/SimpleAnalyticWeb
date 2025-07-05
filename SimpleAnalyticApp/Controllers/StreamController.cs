using Microsoft.AspNetCore.Mvc;
using SimpleAnalyticApp.Utils;

namespace SimpleAnalyticApp.Controllers
{
    [Route("stream")]
    public class StreamController : Controller
    {
        private readonly TcpClientHandler _receiver;

        public StreamController(TcpClientHandler receiver)
        {
            _receiver = receiver;
        }

        [HttpGet("{id}")]
        public async Task MJPEG(string id)
        {
            Response.ContentType = "multipart/x-mixed-replace; boundary=frame";

            while (true)
            {
                var frame = _receiver.GetLatestFrame(id);
                if (frame == null)
                {
                    await Task.Delay(100); // Wait if not yet received
                    continue;
                }

                await Response.Body.WriteAsync("--frame\r\n"u8.ToArray());
                await Response.Body.WriteAsync("Content-Type: image/jpeg\r\n\r\n"u8.ToArray());
                await Response.Body.WriteAsync(frame);
                await Response.Body.WriteAsync("\r\n"u8.ToArray());
                await Response.Body.FlushAsync();

                await Task.Delay(100); // ~30 FPS
            }
        }
    }
}
