using Microsoft.Web.WebView2.Core;

namespace GalleryNestApp.Service
{
    public class WebView2Provider
    {
        private CoreWebView2Environment _environment;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public async Task<CoreWebView2Environment> GetEnvironmentAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (_environment == null)
                {
                    var options = new CoreWebView2EnvironmentOptions
                    {
                        AdditionalBrowserArguments = "--disk-cache-size=1073741824"
                    };
                    _environment = await CoreWebView2Environment.CreateAsync(null, null, options);
                }
                return _environment;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
