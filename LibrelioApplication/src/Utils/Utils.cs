using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace LibrelioApplication.Utils
{
    class Utils
    {
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


        async public static void copyFolder(StorageFolder from, StorageFolder to)
        {
            await to.CreateFolderAsync(from.Name, CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> storageFiles = await from.GetFilesAsync();
            foreach (var storageFile in storageFiles)
            {
                await storageFile.CopyAsync(to, storageFile.Name, NameCollisionOption.ReplaceExisting);
            }

            IReadOnlyList<StorageFolder> storageFolders = await from.GetFoldersAsync();
            foreach (var storageFolder in storageFolders)
            {
                copyFolder(storageFolder, to);
            }

        }

        async public static void prepareTestData()
        {
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;

            StorageFolder init = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
            init = await init.GetFolderAsync("test");


            IReadOnlyList<StorageFile> storageFiles = await init.GetFilesAsync();
            foreach (var storageFile in storageFiles)
            {
                await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
            }

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
        }



    }
}
