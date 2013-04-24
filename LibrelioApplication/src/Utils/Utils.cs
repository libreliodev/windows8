using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LibrelioApplication.Utils
{
    class Utils
    {
        public static SolidColorBrush ColorFromHex(string color)
        {
            byte R = Convert.ToByte(color.Substring(1, 2), 16);
            byte G = Convert.ToByte(color.Substring(3, 2), 16);
            byte B = Convert.ToByte(color.Substring(5, 2), 16);

            return new SolidColorBrush(Color.FromArgb(255, R, G, B));
        }


        //public static async Task<bool> fileExistAsync(StorageFolder folder, string fileName)
        public static async Task<bool> fileExistAsync(StorageFolder folder, string fileName)
        {
            try
            {
                await folder.GetFileAsync(fileName);
                return true;
            }
            catch( Exception e )
            {
                return false;
            }
        }


        async public static void copyFolder(StorageFolder from, StorageFolder _to)
        {
            StorageFolder to = await _to.CreateFolderAsync(from.Name, CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> storageFiles = await from.GetFilesAsync();
            foreach (var storageFile in storageFiles)
            {
                await storageFile.CopyAsync(to, storageFile.Name, NameCollisionOption.ReplaceExisting);
            }

            //IReadOnlyList<StorageFolder> storageFolders = await from.GetFoldersAsync();
            var queryResult = from.CreateFolderQuery();
            IReadOnlyList<StorageFolder> storageFolders = await queryResult.GetFoldersAsync();
            foreach (var storageFolder in storageFolders)
            {
                copyFolder(storageFolder, to);
            }

        }

        async public static Task LoadDefaultData()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFolder init = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("CustomizationAssets");
            init = await init.GetFolderAsync("Covers");

            if (init == null) return;

            IReadOnlyList<StorageFile> storageFiles = await init.GetFilesAsync();
            foreach (var storageFile in storageFiles)
            {
                await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
            }
        }

        async public static Task<bool> prepareTestData()
        {
            Debug.WriteLine("prepareTestData");
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFolder init = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            init = await init.GetFolderAsync("test");


            IReadOnlyList<StorageFile> storageFiles = await init.GetFilesAsync();
            foreach (var storageFile in storageFiles)
            {
                await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
            }
            ////Var2
            //IReadOnlyList<IStorageItem> storageItems = null;
            //try
            //{
            //    storageItems = await init.GetFilesAsync();
            //}
            //catch (Exception ex)
            //{
            //    //rootPage.NotifyUser("Error retrieving file(s) from Clipboard: " + ex.Message, NotifyType.ErrorMessage);
            //}
            //foreach (var storageItem in storageItems) {
            //    var storageFile = storageItem as StorageFile;
            //    if (storageFile != null) {
            //        await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
            //    }
            //}


            //TODODEBUG
            IReadOnlyList<StorageFolder> storageFolders = await init.GetFoldersAsync();
            foreach (var storageFolder in storageFolders)
            {
                copyFolder(storageFolder, folder);
            }

            //copyFolder(init, folder);

            //IReadOnlyList<StorageFile> storageFiles = await init.GetFilesAsync();
            //foreach (var storageFile in storageFiles)
            //{
            //    await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
            //}
            Debug.WriteLine("prepareTestData - finished");
            return true;
        }

        public static void navigateTo(Type page, Object param = null) {
            Frame rootFrame = Window.Current.Content as Frame;
            if (!rootFrame.Navigate(page, param)) {
                {
                    throw new Exception("Failed to create initial page");
                }
            }
        }

        

    }
}
