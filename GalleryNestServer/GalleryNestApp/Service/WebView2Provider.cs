using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    _environment = await CoreWebView2Environment.CreateAsync();
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
