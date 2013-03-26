using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.BulkAccess;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;

namespace LibrelioApplication
{
    public static class DownloadManager
    {
        public static string GetFullNameFromUrl(string url)
        {
            string fullName = "";

            var pos = url.LastIndexOf('/');
            if (pos >= 0)
            {
                fullName = url.Substring(pos+1);
            }
            else
            {
                pos = url.LastIndexOf('\\');
                if (pos >= 0)
                    fullName = url.Substring(pos+1);
                else
                    fullName = url;
            }

            return fullName;
        }

        public static string GetNameFromUrl(string url)
        {
            string name = "";
            var fullName = GetFullNameFromUrl(url);

            var pos = fullName.LastIndexOf('.');
            if (pos >= 0)
                name = fullName.Substring(0, pos);

            return name;
        }

        public static string GetNameFromFullName(string fullName)
        {
            string name = "";

            var pos = fullName.LastIndexOf('.');
            if (pos >= 0)
                name = fullName.Substring(0, pos);

            return name;
        }

        /// <summary>
        /// Use for small assets
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public static async Task<IRandomAccessStream> DownloadFileAsync(LibrelioUrl url, CancellationToken cancelToken = default(CancellationToken))
        {
            var stream = new InMemoryRandomAccessStream();

            using(var response = await new HttpClient().GetAsync(url.AbsoluteUrl))
            {
                var buffer = await response.Content.ReadAsStringAsync();
                var dataWriter = new DataWriter(stream.GetOutputStreamAt(0));
                dataWriter.WriteString(buffer);
                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();
            }

            return stream;
        }

        public static async Task StoreToFolderAsync(string fullName, StorageFolder folder, IRandomAccessStream inputStream, CancellationToken cancelToken = default(CancellationToken))
        {
            if (fullName == "") return;

            var file = await folder.CreateFileAsync(fullName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(inputStream.GetInputStreamAt(0), stream.GetOutputStreamAt(0)).AsTask(cancelToken);
            }
        }

        public static async Task<string> GetStringFromStream(IRandomAccessStream stream)
        {
            using (var dataReader = new DataReader(stream.GetInputStreamAt(0)))
            {
                var size = await dataReader.LoadAsync((uint)stream.Size);
                return dataReader.ReadString(size);
            }
        }
    }

    public sealed class LibrelioUrl
    {
        public LibrelioUrl(string absUrl, string relUrl)
        {
            AbsoluteUrl = absUrl + relUrl;
            RelativeUrl = relUrl;

            Name = DownloadManager.GetNameFromUrl(AbsoluteUrl);
            FullName = DownloadManager.GetFullNameFromUrl(AbsoluteUrl);
        }

        public string AbsoluteUrl { get; set; }
        public string RelativeUrl { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
    }
}