using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using OneDrive;

namespace OneDriveSample.ViewModel
{
    public class ViewModelLocator
    {
        public const string DetailsPageKey = "OneDriveSample.DetailsPage";
        public const string ImagePageKey = "OneDriveSample.ImagePage";

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Cross-Platform IOC initialization
            SimpleIoc.Default.Register<IOneDriveService>(
                () => new OneDriveService(
                    "000000004C169646",
                    "5Eo0ExcvTMGtw91SNrcBDpEffr1qWri6")
                {
                    Scope = OneDriveScopes.AppFolder | OneDriveScopes.ReadWrite | OneDriveScopes.Signin
                });

            SimpleIoc.Default.Register<MainViewModel>();
        }
    }
}