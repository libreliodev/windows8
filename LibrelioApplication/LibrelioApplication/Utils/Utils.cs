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
            catch
            {
                return false;
            }
        }
    }
}
