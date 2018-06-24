using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using OneDrive;
using OneDrive.Info;

namespace OneDriveSample.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IOneDriveService _oneDriveService;
        private RelayCommand<string> _getSpecialFolderCommand;
        private bool _isBusy;
        private RelayCommand _logOutCommand;

        public RelayCommand<string> GetSpecialFolderCommand
        {
            get
            {
                return _getSpecialFolderCommand
                       ?? (_getSpecialFolderCommand = new RelayCommand<string>(
                           async kind =>
                           {
                               var folderKind = (SpecialFolder)Enum.Parse(typeof (SpecialFolder), kind);

                               Exception error = null;

                               try
                               {
                                   IsBusy = true;
                                   _oneDriveService.CheckAuthenticate(
                                       async () => await GetSpecialFolderContinue(folderKind));
                               }
                               catch (Exception ex)
                               {
                                   error = ex;
                               }

                               if (error != null)
                               {
                                   await DialogService.ShowError(error, "There was an issue", "OK", null);
                                   IsBusy = false;
                               }
                           },
                           kind => !IsBusy));
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                if (Set(ref _isBusy, value))
                {
                    GetSpecialFolderCommand.RaiseCanExecuteChanged();
                    LogOutCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public RelayCommand LogOutCommand
        {
            get
            {
                return _logOutCommand
                       ?? (_logOutCommand = new RelayCommand(
                           async () =>
                           {
                               if (!await DialogService.ShowMessage(
                                   "Are you sure you want to log out?",
                                   "Leaving already?",
                                   "Yes",
                                   "No",
                                   null))
                               {
                                   return;
                               }

                               Exception error = null;

                               try
                               {
                                   IsBusy = true;
                                   await _oneDriveService.Logout();
                               }
                               catch (Exception ex)
                               {
                                   error = ex;
                               }

                               if (error != null)
                               {
                                   await DialogService.ShowError(error, "There was an issue", "OK", null);
                               }

                               IsBusy = false;
                           },
                           () => !IsBusy));
            }
        }

        private IDialogService DialogService => ServiceLocator.Current.GetInstance<IDialogService>();

        public MainViewModel(
            IOneDriveService oneDriveService,
            INavigationService navigationService)
        {
            _oneDriveService = oneDriveService;
            _navigationService = navigationService;
        }

        public void CheckAuthError()
        {
            if (_oneDriveService.HasAuthError)
            {
                IsBusy = false;
                _oneDriveService.HasAuthError = false;
            }
        }

        private async Task GetSpecialFolderContinue(SpecialFolder kind)
        {
            Exception error = null;

            try
            {
                FolderInfo info;

                switch (kind)
                {
                    case SpecialFolder.AppRoot:
                        info = await _oneDriveService.GetAppRoot(true);
                        break;

                    case SpecialFolder.RootFolder:
                        info = await _oneDriveService.GetRootFolder(true);
                        break;

                    default:
                        info = await _oneDriveService.GetSpecialFolder(kind);
                        break;
                }

                await _oneDriveService.PopulateChildren(info);
                var folder = new FolderInfoViewModel(info, _oneDriveService);
                _navigationService.NavigateTo(ViewModelLocator.DetailsPageKey, folder);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                await DialogService.ShowError(error, "There was an issue", "OK", null);
            }

            IsBusy = false;
        }
    }
}