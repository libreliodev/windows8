using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.BulkAccess;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;
using Windows.Data.Xml.Dom;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using System.Collections.Generic;

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

        public static async Task<IRandomAccessStream> DownloadFileAsync(string url, CancellationToken cancelToken = default(CancellationToken))
        {
            var stream = new InMemoryRandomAccessStream();

            using (var response = await new HttpClient().GetAsync(url))
            {
                var buffer = await response.Content.ReadAsByteArrayAsync();
                var dataWriter = new DataWriter(stream.GetOutputStreamAt(0));
                dataWriter.WriteBytes(buffer);
                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();
            }

            return stream;
        }

        public static async Task<StorageFile> StoreToFolderAsync(string fullName, StorageFolder folder, IRandomAccessStream inputStream, CancellationToken cancelToken = default(CancellationToken))
        {
            if (fullName == "") return null;

            var file = await folder.CreateFileAsync(fullName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await RandomAccessStream.CopyAndCloseAsync(inputStream.GetInputStreamAt(0), stream.GetOutputStreamAt(0)).AsTask(cancelToken);
            }

            return file;
        }

        public static async Task<string> GetStringFromStream(IRandomAccessStream stream)
        {
            using (var dataReader = new DataReader(stream.GetInputStreamAt(0)))
            {
                var size = await dataReader.LoadAsync((uint)stream.Size);
                return dataReader.ReadString(size);
            }
        }

        public static LibrelioLocalUrl ConvertToLocalUrl(LibrelioUrl url)
        {
            return new LibrelioLocalUrl(url.Title, url.Subtitle, "ND", url.FullName, url.AbsoluteUrl, url.RelativeUrl);
        }

        public static LibrelioLocalUrl ConvertToLocalUrl(LibrelioUrl url, StorageFolder folder)
        {
            return new LibrelioLocalUrl(url.Title, url.Subtitle, folder.Path + "\\", url.FullName, url.AbsoluteUrl, url.RelativeUrl);
        }

        public static LibrelioLocalUrl FindInMetadata(LibrelioUrl url, XmlDocument xml)
        {
            if (xml == null) return null;

            string xpath = "/root/mag[url='" + url.AbsoluteUrl + "']";
            var nodes = xml.SelectNodes(xpath);

            if (nodes.Count > 0)
            {
                var title = nodes[0].SelectNodes("title")[0].InnerText;
                var subtitle = nodes[0].SelectNodes("subtitle")[0].InnerText;
                var path = nodes[0].SelectNodes("path")[0].InnerText;
                if (path != "ND")
                {
                    var pos = path.LastIndexOf('\\');
                    path = path.Substring(0, pos + 1);
                }
                var metadata = nodes[0].SelectNodes("metadata")[0].InnerText;
                if (metadata != "ND")
                {
                    var pos = metadata.LastIndexOf('\\');
                    metadata = metadata.Substring(pos + 1);
                }
                var u = nodes[0].SelectNodes("url")[0].InnerText;
                var rel = nodes[0].SelectNodes("relPath")[0].InnerText;

                return new LibrelioLocalUrl(title, subtitle, path, GetFullNameFromUrl(rel), u, rel);
            }
            else
            {
                return null;
            }
        }

        public static LibrelioLocalUrl GetLocalUrl(IXmlNode mag)
        {
            if (mag == null) return null;

            if (mag.ChildNodes.Count > 0)
            {
                var title = mag.SelectNodes("title")[0].InnerText;
                var subtitle = mag.SelectNodes("subtitle")[0].InnerText;
                var path = mag.SelectNodes("path")[0].InnerText;
                var pos = 0;
                if (path != "" && path != "ND")
                {
                    pos = path.LastIndexOf('\\');
                    path = path.Substring(0, pos + 1);
                }
                var metadata = mag.SelectNodes("metadata")[0].InnerText;
                pos = metadata.LastIndexOf('\\');
                metadata = metadata.Substring(pos + 1);
                var u = mag.SelectNodes("url")[0].InnerText;
                var rel = mag.SelectNodes("relPath")[0].InnerText;

                return new LibrelioLocalUrl(title, subtitle, path, GetFullNameFromUrl(rel), u, rel);
            }
            else
            {
                return null;
            }
        }

        public static LibrelioLocalUrl GetLocalUrl(IList<LibrelioLocalUrl> list, string name)
        {
            foreach (var link in list)
            {
                if (link.FullName == name)
                    return link;
            }

            return null;
        }

        public static bool IsDownloaded(LibrelioLocalUrl url)
        {
            return url.FolderPath != "ND";
        }

        public static async Task<IRandomAccessStream> ProtectPDFStream(IRandomAccessStream source)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            InMemoryRandomAccessStream protectedData = new InMemoryRandomAccessStream();
            IOutputStream dest = protectedData.GetOutputStreamAt(0);

            await Provider.ProtectStreamAsync(source.GetInputStreamAt(0), dest);
            await dest.FlushAsync();

            //Verify that the protected data does not match the original
            //DataReader reader1 = new DataReader(source.GetInputStreamAt(0));
            //DataReader reader2 = new DataReader(protectedData.GetInputStreamAt(0));
            //var size1 = await reader1.LoadAsync((uint)(source.Size < 10000 ? source.Size : 10000));
            //var size2 = await reader2.LoadAsync((uint)(protectedData.Size < 10000 ? protectedData.Size : 10000));
            //IBuffer buffOriginalData = reader1.ReadBuffer((uint)size1);
            //IBuffer buffProtectedData = reader2.ReadBuffer((uint)size2);

            //if (CryptographicBuffer.Compare(buffOriginalData, buffProtectedData))
            //{
            //    throw new Exception("ProtectPDFStream returned unprotected data");
            //}

            //protectedData.Seek(0);
            //await protectedData.FlushAsync();

            // Return the encrypted data.
            return protectedData;
        }

        public static async Task<IRandomAccessStream> UnprotectPDFStream(IRandomAccessStream source)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            InMemoryRandomAccessStream unprotectedData = new InMemoryRandomAccessStream();
            IOutputStream dest = unprotectedData.GetOutputStreamAt(0);

            await Provider.UnprotectStreamAsync(source.GetInputStreamAt(0), dest);
            await unprotectedData.FlushAsync();
            unprotectedData.Seek(0);

            return unprotectedData;
        }

        public static async Task<IRandomAccessStream> OpenPdfFile(LibrelioLocalUrl url)
        {
            StorageFolder folder = null;
            try {

                folder = await StorageFolder.GetFolderFromPathAsync(url.FolderPath);

            } catch { }
            if (folder == null) return null;
            StorageFile file = null;
            try {

                file = await folder.GetFileAsync(url.FullName);

            } catch { }
            if (file == null) return null;

            var stream = await file.OpenAsync(FileAccessMode.Read);

            return await UnprotectPDFStream(stream);
        }

        public static LibrelioLocalUrl DeleteLocalUrl(LibrelioLocalUrl url)
        {
            url.MetadataName = "";
            url.FolderPath = "ND";

            return url;
        }

        public static async Task StoreReceiptAsync(string receipt)
        {
        }

        public static bool IsFullScreenButton(string url)
        {
            return url.Contains("warect=full");
        }

        public static bool IsImage(string url)
        {
            return (url.Contains("jpg") || url.Contains("png"));
        }

        public static bool IsVideo(string url)
        {
            return (url.Contains("mp4") || IsEmbedAsset(url));
        }

        public static bool IsFullScreenAsset(string url)
        {
            return url.Contains("warect=full");
        }

        public static bool IsLocalAsset(string url)
        {
            return url.Contains("localhost");
        }
        
        public static bool IsAutoPlay(string url)
        {
            return url.Contains("waplay=auto");
        }

        public static bool IsNoTransitions(string url)
        {
            return url.Contains("watransition=none");
        }

        public static bool IsLink(string url)
        {
            return !IsLocalAsset(url) && !(!IsFullScreenAsset(url) && url.Contains("waplay=auto"));
        }

        public static bool IsEmbedAsset(string url)
        {
            return !IsLocalAsset(url) && !IsFullScreenAsset(url) && url.Contains("waplay=auto");
        }
    }

    public sealed class LibrelioUrl
    {
        public LibrelioUrl(string absUrl, string relUrl)
        {
            AbsoluteUrl = absUrl + relUrl;
            RelativeUrl = relUrl;

            Title = DownloadManager.GetNameFromUrl(AbsoluteUrl);
            FullName = DownloadManager.GetFullNameFromUrl(AbsoluteUrl);
        }

        public string AbsoluteUrl { get; set; }
        public string RelativeUrl { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FullName { get; set; }
    }

    public sealed class LibrelioLocalUrl
    {
        public LibrelioLocalUrl(string title, string subtitle, string path, string fullName, string url, string relativePath)
        {
            FolderPath = path;

            Title = title;
            Subtitle = subtitle;
            FullName = fullName;
            var pos = fullName.LastIndexOf('.');
            if (pos >= 0)
                MetadataName = fullName.Substring(0, pos) + ".metadata";
            else
                MetadataName = "";
            Url = url;
            RelativePath = relativePath;
        }

        public string FolderPath { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FullName { get; set; }
        public string MetadataName { get; set; }
        public string Url { get; set; }
        public string RelativePath { get; set; }

        public bool IsDownloaded
        {
            get
            {
                return FolderPath != "ND";
            }
        }
    }
}