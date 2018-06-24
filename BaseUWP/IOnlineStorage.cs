using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base
{
    public interface IOnlineStorage : IStorage
    {
        bool LogIn(Windows.UI.Xaml.Controls.WebView webView, Windows.UI.Xaml.Controls.Frame parentFrame);
        bool IsConnected();
        void LogOut();
    }
}
