using System.Reflection;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using OneDrive.Info;

namespace OneDriveSample.ViewModel
{
    public abstract class ItemInfoViewModel : ViewModelBase
    {
        private bool _isBusy;
        private RelayCommand _openItemCommand;
        private string _status;

        public bool HasChildren => (ModelItemInfo as FolderInfo)?.ChildCount > 0;

        // Quite messy :) This should be solved with an icon, etc...
        public string InfoName => ModelItemInfo.GetType().GetTypeInfo().Name.ToLower().Replace("info", "");

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (Set(ref _isBusy, value, true))
                {
                    OnBusy();
                }
            }
        }

        public bool IsFile => !IsFolder;

        public bool IsFolder => ModelItemInfo.Kind == ItemInfo.FileFolderKind.Folder;

        public abstract ItemInfo ModelItemInfo
        {
            get;
        }

        public RelayCommand OpenItemCommand => _openItemCommand
                                               ?? (_openItemCommand = new RelayCommand(
                                                   async () =>
                                                   {
                                                       IsBusy = true;
                                                       await OpenItem();
                                                   }));

        public string SizeFormatted
        {
            get
            {
                if (ModelItemInfo.SizeInBytes > 1024 * 1024)
                {
                    return $"{((ModelItemInfo.SizeInBytes / 1024.0) / 1024.0):N2} MB";
                }

                if (ModelItemInfo.SizeInBytes > 1024)
                {
                    return $"{(ModelItemInfo.SizeInBytes / 1024.0):N2} kB";
                }

                return $"{ModelItemInfo.SizeInBytes} bytes";
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                Set(ref _status, value);
            }
        }

        protected IDialogService DialogService => ServiceLocator.Current.GetInstance<IDialogService>();

        protected INavigationService NavigationService => ServiceLocator.Current.GetInstance<INavigationService>();

        public abstract Task OpenItem();

        protected virtual void OnBusy()
        {
            // Do nothing
        }
    }
}