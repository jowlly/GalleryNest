using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
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

        public static IEnumerable<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var validHosts = host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork);
            return validHosts.Count() > 0 ? validHosts.Select(x => x.ToString()) : throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private string GetServerIpAddress()
        {
            // Настройки из конфига
            var preferredAdapter = _config["Network:PreferredAdapter"]?.Split("|") ?? ["Ethernet"];
            var fallbackToAny = bool.Parse(_config["Network:FallbackToAny"] ?? "true");

            var interfaces = NetworkInterface.GetAllNetworkInterfaces().ToList();
            interfaces = interfaces.Where(i => i.OperationalStatus == OperationalStatus.Up &&
                            i.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                            !i.Description.Contains("Virtual") &&
                            !i.Description.Contains("Pseudo")).ToList();

            foreach (var ni in interfaces)
            {
                _logger.LogInformation($"Found interface: {ni.Name} ({ni.Description}), Type: {ni.NetworkInterfaceType}");

                // Приоритет для Ethernet адаптера
                if (preferredAdapter.Any(x => ni.Name.Equals(x, StringComparison.OrdinalIgnoreCase)) ||
                    preferredAdapter.Any(x => ni.Description.Contains(x)) ||
                    preferredAdapter.Any(x => ni.NetworkInterfaceType.ToString().Contains(x)))
                {
                    var ip = ni.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork)?
                        .Address.ToString();

                    if (!string.IsNullOrEmpty(ip))
                    {
                        _logger.LogInformation($"Selected IP: {ip} from adapter: {ni.Name}");
                        return ip;
                    }
                }
            }

            if (fallbackToAny)
            {
                var fallbackIp = NetworkInterface.GetAllNetworkInterfaces()
                    .SelectMany(i => i.GetIPProperties().UnicastAddresses)
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork &&
                                        !IPAddress.IsLoopback(a.Address))?
                    .Address.ToString();

                if (!string.IsNullOrEmpty(fallbackIp))
                {
                    _logger.LogWarning($"Using fallback IP: {fallbackIp}");
                    return fallbackIp;
                }
            }

            throw new InvalidOperationException("No suitable network interface found");
        }



        private string GetServerPort()
        {
            var urls = _config["ASPNETCORE_URLS"]?.Split(';');
            var port = urls?.FirstOrDefault(u => u.StartsWith("http://"))?
                .Split(':').Last() ?? "5285";

            return port;
        }

        [HttpGet("qr")]
        public IActionResult GenerateQrCode()
        {
            try
            {
                var serverIp = GetLocalIPAddress();
                var port = GetServerPort();
                var connectUrl = $"http://{string.Join("|", serverIp)}:{port}";

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

        [HttpGet("connect")]
        public async Task<IActionResult> GetServerAddress()
        {
            try
            {
                var serverIp = GetServerIpAddress();
                var port = GetServerPort();
                return Ok(new { serverUrl = $"http://{serverIp}:{port}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting server address");
                return StatusCode(500, "Internal Server Error");
            }
        }


    }
}
