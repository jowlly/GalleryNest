using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GalleryNestServer.Controllers
{
    [Route("api/device")]
    [ApiController]
    public class DeviceController(ILogger<DeviceController> logger, IConfiguration config) : ControllerBase
    {
        private readonly ILogger<DeviceController> _logger = logger;
        private readonly IConfiguration _config = config;

        private string GetEthernetIpAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var ni in interfaces)
            {
                if (ni.Name == "Ethernet" &&
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    ni.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProps = ni.GetIPProperties();

                    var ipv4Address = ipProps.UnicastAddresses
                        .FirstOrDefault(addr =>
                            addr.Address.AddressFamily == AddressFamily.InterNetwork)?
                        .Address?.ToString();

                    if (!string.IsNullOrEmpty(ipv4Address))
                    {
                        return ipv4Address;
                    }
                }
            }

            throw new InvalidOperationException("Ethernet adapter with IPv4 address not found");
        }

        private string GetServerPort()
        {
            var urls = _config["ASPNETCORE_URLS"]?.Split(';');
            var port = urls?.FirstOrDefault(u => u.StartsWith("http://"))?
                .Split(':').Last() ?? "5000";

            return port;
        }

        [HttpGet("qr")]
        public IActionResult GenerateQrCode()
        {
            try
            {
                var serverIp = GetEthernetIpAddress();
                var port = GetServerPort();
                var connectUrl = $"http://{serverIp}:{port}";

                var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(connectUrl, QRCodeGenerator.ECCLevel.Q);

                var svgQrCode = new SvgQRCode(qrCodeData);
                var svgContent = svgQrCode.GetGraphic(
                    viewBox: new Size(20, 20),
                    darkColor: Color.FromArgb(0, 0, 0),
                    lightColor: Color.FromArgb(255, 255, 255),
                    drawQuietZones: true,
                    sizingMode: SvgQRCode.SizingMode.ViewBoxAttribute
                );

                return new ContentResult
                {
                    ContentType = "image/svg+xml",
                    Content = svgContent,
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating QR code");
                return StatusCode(500, "Internal Server Error");
            }

        }

        
    }
}
