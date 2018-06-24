using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using OneDrive;
using OneDrive.Info;
using OneDriveSample.Helpers;

namespace OneDriveSample.ViewModel
{
    public class FolderInfoViewModel : ItemInfoViewModel
    {
        private readonly IOneDriveService _service;

        private RelayCommand _uploadCommand;

        public ObservableCollection<ItemInfoViewModel> Children
        {
            get;
        }

        public FolderInfo Model
        {
            get;
        }

        public override ItemInfo ModelItemInfo => Model as ItemInfo;

        public RelayCommand UploadCommand
        {
            get
            {
                return _uploadCommand
                       ?? (_uploadCommand = new RelayCommand(
                           () =>
                           {
                               IsBusy = true;
                               Status = "Uploading a new file";
                               Picker.GetFile(async (fileName, stream) => await UploadContinue(fileName, stream));
                           },
                           () => !IsBusy));
            }
        }

        private IFilePicker Picker => ServiceLocator.Current.GetInstance<IFilePicker>();

        public FolderInfoViewModel(FolderInfo model, IOneDriveService service)
        {
            Model = model;
            _service = service;

            Messenger.Default.Register<PropertyChangedMessage<bool>>(
                this,
                HandleBusyChild);

            Messenger.Default.Register<PropertyChangedMessage<string>>(
                this,
                HandleStatus);

            Children = new ObservableCollection<ItemInfoViewModel>();

            if (model.Children == null)
            {
                return;
            }

            PopulateChildrenCollection();
        }

        public override async Task OpenItem()
        {
            Status = $"Loading {Model.Name}";

            // ALways refresh. We could do some caching here.
            await _service.PopulateChildren(Model);
            PopulateChildrenCollection();
            NavigationService.NavigateTo(ViewModelLocator.DetailsPageKey, this);

            IsBusy = false;
            Status = string.Empty;
        }

        protected override void OnBusy()
        {
            UploadCommand.RaiseCanExecuteChanged();
        }

        private void HandleBusyChild(PropertyChangedMessage<bool> message)
        {
            var child = ((ItemInfoViewModel)message.Sender);
            if (Children.Contains(child)
                && (message.PropertyName == nameof(ItemInfoViewModel.IsBusy)))
            {
                IsBusy = child.IsBusy;
            }
        }

        private void HandleStatus(PropertyChangedMessage<string> message)
        {
            var child = ((ItemInfoViewModel)message.Sender);
            if (Children.Contains(child)
                && (message.PropertyName == nameof(ItemInfoViewModel.Status)))
            {
                Status = child.Status;
            }
        }

        private void PopulateChildrenCollection()
        {
            Children.Clear();

            foreach (var info in Model.Children)
            {
                switch (info.Kind)
                {
                    case ItemInfo.FileFolderKind.Folder:
                        Children.Add(new FolderInfoViewModel((FolderInfo)info, _service));
                        break;

                    case ItemInfo.FileFolderKind.File:
                        Children.Add(new FileInfoViewModel((FileInfo)info, _service));
                        break;
                }
            }
        }

        private async Task UploadContinue(string fileName, Stream stream)
        {
            Exception error = null;

            try
            {
                if (stream == null)
                {
                    await DialogService.ShowMessage("Nothing was picked", "No image");
                    IsBusy = false;
                    return;
                }

                Status = $"Uploading {fileName}";

                var info = await _service.SaveFile(Model.Id, fileName, stream);

                Status = "Refreshing children";
                await _service.Refresh(Model);
                await _service.PopulateChildren(Model);
                PopulateChildrenCollection();

                await DialogService.ShowMessage($"Uploaded file with ID {info.Id}", "Success");
                Status = "Done";
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                await DialogService.ShowError(error, "Problem when uploading", "OK", null);
                IsBusy = false;
                Status = string.Empty;
                return;
            }

            IsBusy = false;
        }

#if DEBUG
        public FolderInfoViewModel()
            : this(true)
        {
            Model = new FolderInfo
            {
                ChildCount = 3,
                CreatedDateTime = DateTime.Now,
                CTag = "{4BA8CE3D-3C9E-48C0-B944-754C14646B58}",
                ETag = "{3197AC11-039C-4955-8D7E-93EBFF59FAFB}",
                Id = "abcdefgh1234",
                LastModifiedDateTime = DateTime.Now,
                Name = "This is a folder name",
                ParentId = "zyxwvu98765",
                SizeInBytes = 12345678,
                WebUrl = "http://www.galasoft.ch/testfolder"
            };
        }

        public FolderInfoViewModel(bool generateChildren)
        {
            if (generateChildren)
            {
                Children = new ObservableCollection<ItemInfoViewModel>
                {
                    new FolderInfoViewModel(false),
                    new FileInfoViewModel(),
                    new FileInfoViewModel()
                };
            }
        }
#endif
    }
}