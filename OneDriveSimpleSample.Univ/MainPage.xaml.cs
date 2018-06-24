using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OneDriveSimpleSample.Request;
using OneDriveSimpleSample.Response;
using OneDriveSimpleSample.Helpers;

using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace OneDriveSimpleSample
{
    public sealed partial class MainPage : Page
    {
        private static string _downloadFilePath;
        private static string _folderPath;
        private readonly OneDriveService _service = ((App)Application.Current).ServiceInstance;
        private readonly OneDriveFacadeUWP.OneDriveStorage _oneDriveStorage;
        private string _savedId;

        public MainPage()
        {
            InitializeComponent();
            _oneDriveStorage = new OneDriveFacadeUWP.OneDriveStorage();
            var navManager = SystemNavigationManager.GetForCurrentView();
            navManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Restore the entered values
            FolderPathText.Text = _folderPath ?? string.Empty;
            DownloadFilePathText.Text = _downloadFilePath ?? string.Empty;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Save the entered values for later
            _folderPath = FolderPathText.Text;
            _downloadFilePath = DownloadFilePathText.Text;
            base.OnNavigatingFrom(e);
        }

        private async void AuthenticateClick(object sender, RoutedEventArgs e)
        {
            ShowBusy(true);

            if (!_oneDriveStorage.IsConnected()) 
            {
                Frame.Navigate(typeof(OneDriveFacadeUWP.AuthenticationPage), _oneDriveStorage);    
            }else
            {
                var dialog = new MessageDialog("You are authenticated!", "Success!");
                await dialog.ShowAsync();
            }
        }

        private async void BrowseSubfolderClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FolderPathText.Text))
            {
                var dialog = new MessageDialog("Please enter a path to a folder, for example Apps/OneDriveSample", "Error!");
                await dialog.ShowAsync();
                return;
            }

            Exception error = null;
            ItemInfoResponse subfolder = null;

            ShowBusy(true);

            try
            {
                subfolder = await _service.GetItem(FolderPathText.Text);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            if (subfolder == null)
            {
                var dialog = new MessageDialog($"Not found: {FolderPathText.Text}");
                await dialog.ShowAsync();
            }
            else
            {
                var children = await _service.PopulateChildren(subfolder);
                DisplayHelper.ShowContent(
                    "SHOW SUBFOLDER CONTENT ------------------------",
                    subfolder,
                    children,
                    async message =>
                    {
                        var dialog = new MessageDialog(message);
                        await dialog.ShowAsync();
                    });
            }

            ShowBusy(false);
        }

        private async void DownloadFileClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DownloadFilePathText.Text))
            {
                var dialog = new MessageDialog("Please enter a path to an existing file, for example Apps/OneDriveSample/Test.jpg", "Error!");
                await dialog.ShowAsync();
                return;
            }

            Exception error = null;
            ItemInfoResponse foundFile = null;
            Stream contentStream = null;

            ShowBusy(true);

            try
            {
                foundFile = await _service.GetItem(DownloadFilePathText.Text);

                if (foundFile == null)
                {
                    var dialog = new MessageDialog($"Not found: {DownloadFilePathText.Text}");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                    return;
                }

                // Get the file's content
                contentStream = await _service.RefreshAndDownloadContent(foundFile, false);

                if (contentStream == null)
                {
                    var dialog = new MessageDialog($"Content not found: {DownloadFilePathText.Text}");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                    return;
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            // Save the retrieved stream to the local drive

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = foundFile.Name
            };

            var extension = Path.GetExtension(foundFile.Name);

            picker.FileTypeChoices.Add(
                $"{extension} files",
                new List<string>
                {
                    extension
                });

            var targetFile = await picker.PickSaveFileAsync();

            using (var targetStream = await targetFile.OpenStreamForWriteAsync())
            {
                using (var writer = new BinaryWriter(targetStream))
                {
                    contentStream.Position = 0;

                    using (var reader = new BinaryReader(contentStream))
                    {
                        byte[] bytes;

                        do
                        {
                            bytes = reader.ReadBytes(1024);
                            writer.Write(bytes);
                        }
                        while (bytes.Length == 1024);
                    }
                }
            }

            var successDialog = new MessageDialog("Done saving the file!", "Success");
            await successDialog.ShowAsync();
            ShowBusy(false);
        }

        private async void GetAppRootClick(object sender, RoutedEventArgs e)
        {
            Exception error = null;
            ItemInfoResponse folder = null;
            IList<ItemInfoResponse> children = null;

            ShowBusy(true);

            try
            {
                folder = await _service.GetAppRoot();
                children = await _service.PopulateChildren(folder);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            DisplayHelper.ShowContent(
                "SHOW APP FOLDER CONTENT ++++++++++++++++++++++",
                folder,
                children,
                async message =>
                {
                    var dialog = new MessageDialog(message);
                    await dialog.ShowAsync();
                    ShowBusy(false);
                });
        }

        private async void GetLinkClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_savedId))
            {
                var dialog =
                    new MessageDialog(
                        "For the purpose of this demo, save a file first using the Upload File button",
                        "No file saved");
                await dialog.ShowAsync();
                return;
            }

            Exception error = null;
            LinkResponseInfo linkInfo = null;

            ShowBusy(true);

            try
            {
                linkInfo = await _service.GetLink(LinkKind.View, _savedId); // This could also be LinkKind.Edit
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            Debug.WriteLine("RETRIEVED LINK ---------------------");
            Debug.WriteLine(linkInfo.Link.WebUrl);
            var successDialog = new MessageDialog(
                $"The link was copied to the Debug window: {linkInfo.Link.WebUrl}",
                "No file saved");
            await successDialog.ShowAsync();
            ShowBusy(false);
        }

        private async void GetRootFolderClick(object sender, RoutedEventArgs e)
        {
            Exception error = null;
            ItemInfoResponse folder = null;
            IList<ItemInfoResponse> children = null;

            ShowBusy(true);

            try
            {
                folder = await _service.GetRootFolder();
                children = await _service.PopulateChildren(folder);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            DisplayHelper.ShowContent(
                "SHOW ROOT FOLDER ++++++++++++++++++++++",
                folder,
                children,
                async message =>
                {
                    var dialog = new MessageDialog(message);
                    await dialog.ShowAsync();
                });

            ShowBusy(false);
        }

        private async void LogOffClick(object sender, RoutedEventArgs e)
        {
            Exception error = null;
            ShowBusy(true);

            try
            {
                _oneDriveStorage.LogOut();
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                var dialog = new MessageDialog(error.Message, "Error!");
                await dialog.ShowAsync();
                ShowBusy(false);
                return;
            }

            var successDialog = new MessageDialog("You are now logged off", "Success");
            await successDialog.ShowAsync();
            ShowBusy(false);
        }

        private void ShowBusy(bool isBusy)
        {
            Progress.IsActive = isBusy;
            PleaseWaitCache.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void UploadFileClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };

            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                ShowBusy(true);

                Exception error = null;

                try
                {
                    // For the demo, save this file in the App folder
                    var folder = await _service.GetAppRoot();

                    using (var stream = await file.OpenStreamForReadAsync())
                    {
                        var info = await _service.SaveFile(folder.Id, file.Name, stream);

                        // Save for the GetLink demo
                        _savedId = info.Id;

                        var successDialog =
                            new MessageDialog(
                                $"Uploaded file has ID {info.Id}. You can now use the Get Link button to retrieve a direct link to the file",
                                "Success");
                        await successDialog.ShowAsync();
                    }

                    ShowBusy(false);
                }
                catch (Exception ex)
                {
                    error = ex;
                }

                if (error != null)
                {
                    var dialog = new MessageDialog(error.Message, "Error!");
                    await dialog.ShowAsync();
                    ShowBusy(false);
                }
            }
        }
    }
}
