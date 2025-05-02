using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalleryNestApp.Service
{
    public interface IParameterReceiver
    {
        void ReceiveParameter(object? parameter);
    }
}
