using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

using Base;

namespace OneDriveFacadeUWP
{
    public class OneDriveStorage : IOnlineStorage
    {
        OneDriveSimpleSample.OneDriveService driveLink;
        public OneDriveStorage()
        {
            driveLink = new OneDriveSimpleSample.OneDriveService("000000004C169646");
        }

        public bool CopyFile(string origin, string destination, bool move = false)
        {
            throw new NotImplementedException();
        }

        public string[] GetFolderContent(string folder)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected()
        {
            return driveLink.CheckAuthenticate( () => {}, () => {});
        }

        public bool IsFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool IsFolder(string path)
        {
            throw new NotImplementedException();
        }

        public bool LogIn(Windows.UI.Xaml.Controls.WebView webView, Windows.UI.Xaml.Controls.Frame parentFrame)
        {
            var uri = driveLink.GetStartUri();
            webView.Navigate(uri);

            webView.NavigationCompleted += (s, e) =>
            {
                if (driveLink.CheckRedirectUrl(e.Uri.AbsoluteUri))
                {
                    driveLink.ContinueGetTokens(e.Uri);
                    var dialog = new Windows.UI.Popups.MessageDialog("You are authenticated!", "Success!");
                    dialog.ShowAsync();
                    parentFrame.GoBack();
                }
            };

            webView.NavigationFailed += (s, e) =>
            {
                driveLink.ContinueGetTokens(null);
                var dialog = new Windows.UI.Popups.MessageDialog("There problems authenticating you. Please try again :(", "Fail!");
                dialog.ShowAsync();
                parentFrame.GoBack();
            };
            return true;
        }

        public async void LogOut()
        {
            await driveLink.Logout();
        }

        public bool MakeFolder(string path)
        {
            throw new NotImplementedException();
        }
    }
}
