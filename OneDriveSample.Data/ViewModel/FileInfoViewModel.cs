using System;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using OneDrive;
using OneDrive.Info;
using OneDriveSample.Helpers;

namespace OneDriveSample.ViewModel
{
    public class FileInfoViewModel : ItemInfoViewModel
    {
        private readonly IOneDriveService _service;

        public FileInfo Model
        {
            get;
        }

        public override ItemInfo ModelItemInfo => Model as ItemInfo;
#if DEBUG
        public FileInfoViewModel()
        {
            Model = new FileInfo
            {
                CreatedDateTime = DateTime.Now,
                CTag = "",
                ETag = "",
                Id = "aaaabbbb3333",
                LastModifiedDateTime = DateTime.Now,
                Name = "This is a file name",
                ParentId = "abcdefgh1234",
                SizeInBytes = 3214,
                WebUrl = "http://www.galasoft.ch/testfile"
            };
        }
#endif

        public FileInfoViewModel(FileInfo model, IOneDriveService service)
        {
            _service = service;
            Model = model;
        }

        public override async Task OpenItem()
        {
            Status = $"Loading {Model.Name}";

            // Use caching here
            if (Model.ContentStream == null)
            {
                await _service.Refresh(Model); // Needed to make sure that the DownloadUri is up to date
                await _service.DownloadContent(Model);
            }

            var imageInfo = Model as ImageInfo;

            if (imageInfo != null)
            {
                NavigationService.NavigateTo(ViewModelLocator.ImagePageKey, this);
                IsBusy = false;
                Status = string.Empty;
                return;
            }

            await SaveItem();
        }

        public async Task SaveItem()
        {
            if (Model.ContentStream == null)
            {
                throw new InvalidOperationException("ContentStream must be loaded first");
            }

            Status = $"Saving {Model.Name}";
            var fileSaver = ServiceLocator.Current.GetInstance<IFileSaver>();
            fileSaver.SaveFile(Model);
        }
    }
}