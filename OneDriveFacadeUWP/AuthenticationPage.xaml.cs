using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace OneDriveSimpleSample
{
    public sealed partial class AuthenticationPage
    {
        public AuthenticationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            base.OnNavigatedTo(args);
            OneDriveService driveLink = (OneDriveService)(args.Parameter);
            Loaded += (s, e) =>
            {
                var uri = driveLink.GetStartUri();
                Web.Navigate(uri);
            };

            Web.NavigationCompleted += (s, e) =>
            {
                if (driveLink.CheckRedirectUrl(e.Uri.AbsoluteUri))
                {
                    driveLink.ContinueGetTokens(e.Uri);
                }
            };

            Web.NavigationFailed += (s, e) =>
            {
                driveLink.ContinueGetTokens(null);
            };
        }
    }
}