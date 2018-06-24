using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace OneDriveFacadeUWP
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
            OneDriveStorage driveLink = (OneDriveStorage)(args.Parameter);
            driveLink.LogIn(Web,Frame);
        }
    }
}