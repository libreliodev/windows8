using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using Windows.Data.Xml.Dom;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.Graphics.Imaging;
using MuPDFWinRT;
using Windows.ApplicationModel.Resources;
using System.Net.Http.Headers;
using Windows.Storage.Search;


namespace LibrelioApplication
{
    public class MagazineData
    {
        public string folderUrl { get; set; }
        public IRandomAccessStream stream { get; set; }
    }

    public class MagazineManager
    {
        private string _path;
        private string _name;
        private LibrelioUrl _pList = null;
        private IList<LibrelioUrl> _magazinesUrl = new List<LibrelioUrl>();
        private IList<LibrelioLocalUrl> _magazinesLocalUrl = new List<LibrelioLocalUrl>();
        private IList<LibrelioLocalUrl> _magazinesLocalUrlDownloaded = new List<LibrelioLocalUrl>();

        private StorageFolder _folder = null;

        private XmlDocument localXml = null;
        private XmlDocument localDownloadedXml = null;

        public LibrelioUrl PLIST { get { return _pList; } }
        public IList<LibrelioUrl> MagazineUrl { get { return _magazinesUrl; } }
        public IList<LibrelioLocalUrl> MagazineLocalUrl { get { return _magazinesLocalUrl; } }
        public IList<LibrelioLocalUrl> MagazineLocalUrlDownloaded { get { return _magazinesLocalUrlDownloaded; } }

        public string StatusText { get; set; }

        IList<string> links = new List<string>();


        public MagazineManager(string name)
        {
            this._path = "";
            this._name = name;
            StatusText = "";
        }

        /// <summary>
        /// You can download .plist, .pdf, and different assets
        /// </summary>
        /// <param name="path">The path to the magazine directory</param>
        /// <param name="name">Name of collection</param>
        public MagazineManager(string path, string name)
        {
            this._path = path;
            this._name = name;
            StatusText = "";

            _pList = new LibrelioUrl(0, path, name + ".plist");
        }

        public async Task<bool> LoadPLISTAsync()
        {
            bool isUpdated = false;

            _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_name, CreationCollisionOption.OpenIfExists);
            if (_magazinesLocalUrl.Count == 0)
            {
                await LoadLocalMetadata();
                await LoadLocalMetadataDownloaded();
            }

            var settings = new XmlLoadSettings();
            settings.ProhibitDtd = false;
            settings.ValidateOnParse = true;
            settings.ResolveExternals = true;

            try
            {
                XmlDocument xml = new XmlDocument();

                bool noXml = false;
                try
                {
                    var httpClient = new HttpClient();
		    httpClient.DefaultRequestHeaders.Add("user-agent", "LibrelioWinRT");
                    var url = new Uri(this._pList.AbsoluteUrl);
                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                    var value = ApplicationData.Current.LocalSettings.Values["last_update"] as string;
                    if (value == null)
                    {
                        var date = new DateTime(2012, 4, 15);
                        ApplicationData.Current.LocalSettings.Values["last_update"] = date.ToUniversalTime().ToString("R");
                        value = ApplicationData.Current.LocalSettings.Values["last_update"] as string;
                    }

                    httpRequestMessage.Headers.IfModifiedSince = DateTime.Parse(value);
                    var response = await httpClient.SendAsync(httpRequestMessage);

                    //var resonse = await new HttpClient().GetAsync(this._pList.AbsoluteUrl);
                    response.EnsureSuccessStatusCode();
                    var str = await response.Content.ReadAsStringAsync();
                    xml.LoadXml(str, settings);
                    //xml = await XmlDocument.LoadFromUriAsync(new Uri(this._pList.AbsoluteUrl), settings);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("304") || ex.Message.Contains("Not Modified")) return false;
                    noXml = true;
                }

                if (!noXml)
                {
                    await ReadPList(xml);
                    isUpdated = true;
                }
            }
            catch
            {
                throw new Exception("Unable to download plist");
            }

            return isUpdated;
        }

        public async Task LoadLocalMagazineList()
        {
            _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_name, CreationCollisionOption.OpenIfExists);
            await LoadLocalMetadata();
        }

        public async Task LoadLocalMagazineListDownloaded()
        {
            _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_name, CreationCollisionOption.OpenIfExists);
            await LoadLocalMetadataDownloaded();
        }

        public async Task<BitmapSource> DownloadThumbnailAsync(LibrelioUrl magUrl)
        {
            String s = ".";
            if (magUrl.FullName.Contains("_"))
            {
                s = "_.";
            }
            var pos = magUrl.AbsoluteUrl.LastIndexOf(s);
            var url = magUrl.AbsoluteUrl.Substring(0, pos) + ".png";
            var stream = await DownloadManager.DownloadFileAsync(url);

            var bitmap = new BitmapImage();
            try
            {
                await bitmap.SetSourceAsync(stream);
            }
            catch
            {
                bitmap = null;
            }
            return bitmap;
        }

        public async Task<BitmapSource> DownloadThumbnailAsync(LibrelioUrl magUrl, StorageFolder folder)
        {
            string s = ".";
            if (magUrl.FullName.Contains("_."))
            {
                s = "_.";
            }
            var pos = magUrl.AbsoluteUrl.LastIndexOf(s);
            var url = magUrl.AbsoluteUrl.Substring(0, pos) + ".png";
            var stream = await DownloadManager.DownloadFileAsync(url);

            await DownloadManager.StoreToFolderAsync(magUrl.FullName.Replace(s + "pdf", ".png"), folder, stream);

            var bitmap = new BitmapImage();
            try
            {
                await bitmap.SetSourceAsync(stream);
            }
            catch
            {
                bitmap = null;
            }
            return bitmap;
        }

        public async Task<BitmapSource> DownloadThumbnailAsync(LibrelioLocalUrl magUrl, StorageFolder folder)
        {
            string s = ".";
            if (magUrl.FullName.Contains("_."))
            {
                s = "_.";
            }
            var pos = magUrl.Url.LastIndexOf(s);
            var url = magUrl.Url.Substring(0, pos) + ".png";
            var stream = await DownloadManager.DownloadFileAsync(url);

            await DownloadManager.StoreToFolderAsync(magUrl.FullName.Replace(s + "pdf", ".png"), folder, stream);

            var bitmap = new BitmapImage();
            try
            {
                await bitmap.SetSourceAsync(stream);
            }
            catch
            {
                bitmap = null;
            }
            return bitmap;
        }

        public async Task<IRandomAccessStream> DownloadMagazineAsync(LibrelioUrl magUrl, StorageFolder folder, bool isd, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var loader = new ResourceLoader();
            StatusText = loader.GetString("download_progress");

            var stream = await DownloadPDFAsync(magUrl, folder, isd, progress, cancelToken);
            if (stream == null || cancelToken.IsCancellationRequested) return null;

            await GetUrlsFromPDF(stream);

            StatusText = loader.GetString("downloading") + " 2/" + (links.Count + 1);
            var url = DownloadManager.ConvertToLocalUrl(magUrl, folder, isd);

            if (url != null)
            {
                await DownloadPDFAssetsAsync(url, links, progress, cancelToken);
            }

            StatusText = loader.GetString("done");

            return stream;
        }

        public async Task<IRandomAccessStream> DownloadMagazineAsync(LibrelioLocalUrl magUrl, StorageFolder folder, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var loader = new ResourceLoader();
            StatusText = loader.GetString("download_progress");

            var stream = await DownloadPDFAsync(magUrl, folder, progress, cancelToken);
            if (stream == null || cancelToken.IsCancellationRequested) return null;

            await GetUrlsFromPDF(stream);

            StatusText = loader.GetString("downloading") + " 2/" + (links.Count + 1);
            magUrl.FolderPath = folder.Path + "\\";
            //var url = DownloadManager.ConvertToLocalUrl(magUrl, folder);

            if (magUrl != null)
            {
                await DownloadPDFAssetsAsync(magUrl, links, progress, cancelToken);
            }

            StatusText = loader.GetString("done");

            return stream;
        }

        public async Task<IRandomAccessStream> DownloadMagazineAsync(LibrelioUrl magUrl, string redirectUrl, StorageFolder folder, bool isd, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var loader = new ResourceLoader();
            StatusText = loader.GetString("download_progress");

            var tmpUrl = magUrl.AbsoluteUrl;
            magUrl.AbsoluteUrl = redirectUrl;
            var stream = await DownloadPDFAsync(magUrl, folder, isd, progress, cancelToken);
            magUrl.AbsoluteUrl = tmpUrl;
            if (stream == null || cancelToken.IsCancellationRequested) return null;

            await GetUrlsFromPDF(stream);

            StatusText = loader.GetString("downloading") + " 2/" + (links.Count + 1);
            var url = DownloadManager.ConvertToLocalUrl(magUrl, folder, isd);

            if (url != null)
            {
                await DownloadPDFAssetsAsync(url, links, progress, cancelToken);
            }

            StatusText = loader.GetString("done");

            return stream;
        }

        public async Task<IRandomAccessStream> DownloadMagazineAsync(LibrelioLocalUrl magUrl, string redirectUrl, StorageFolder folder, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var loader = new ResourceLoader();
            StatusText = loader.GetString("download_progress");

            var tmpUrl = magUrl.Url;
            magUrl.Url = redirectUrl;
            var stream = await DownloadPDFAsync(magUrl, folder, progress, cancelToken);
            magUrl.Url = tmpUrl;
            if (stream == null || cancelToken.IsCancellationRequested) return null;

            await GetUrlsFromPDF(stream);

            StatusText = loader.GetString("downloading") + " 2/" + (links.Count + 1);

            if (magUrl != null)
            {
                await DownloadPDFAssetsAsync(magUrl, links, progress, cancelToken);
            }

            StatusText = loader.GetString("done");

            return stream;
        }

        public LibrelioLocalUrl FindInMetadata(LibrelioUrl url)
        {
            return DownloadManager.FindInMetadata(url, localXml);
        }

        public async Task<IRandomAccessStream> DownloadPDFAsync(LibrelioUrl magUrl, StorageFolder folder, bool isd, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            HttpClient client = new HttpClient();
	    httpClient.DefaultRequestHeaders.Add("user-agent", "LibrelioWinRT");

            var url = magUrl.AbsoluteUrl;
            if (isd) url = url.Replace("_.", ".");

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            //int read = 0;
            //int offset = 0;
            //byte[] responseBuffer = new byte[1024];

            //var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);
            //response.EnsureSuccessStatusCode();

            //var length = response.Content.Headers.ContentLength;

            //cancelToken.ThrowIfCancellationRequested();

            //var stream = new InMemoryRandomAccessStream();

            //using (var responseStream = await response.Content.ReadAsStreamAsync())
            //{
            //    do
            //    {
            //        cancelToken.ThrowIfCancellationRequested();

            //        read = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

            //        cancelToken.ThrowIfCancellationRequested();

            //        await stream.AsStream().WriteAsync(responseBuffer, 0, read);

            //        offset += read;
            //        uint val = (uint)(offset * 100 / length);
            //        if (val >= 100) val = 99;
            //        if (val <= 0) val = 1;
            //        progress.Report((int)val);
            //    }
            //    while (read != 0);
            //}

            //progress.Report(100);

            //await stream.FlushAsync();

            ////var folder = await AddMagazineFolderStructure(magUrl);
            ////var folder = await StorageFolder.GetFolderFromPathAsync(folderUrl);
            var name = magUrl.FullName;
            if (isd) name = name.Replace("_.", ".");
            var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

            //using (var protectedStream = await DownloadManager.ProtectPDFStream(stream))
            //using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            ////using (var unprotectedStream = await DownloadManager.UnprotectPDFStream(protectedStream))
            //{

            //    await RandomAccessStream.CopyAsync(protectedStream, fileStream.GetOutputStreamAt(0));

            //    await fileStream.FlushAsync();
            //}

            progress.Report(0);
            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(url), file);

            await HandleDownloadAsync(download, true, progress, cancelToken);

            progress.Report(100);

            if (cancelToken.IsCancellationRequested)
                return null;

            var stream = await download.ResultFile.OpenAsync(FileAccessMode.ReadWrite);
            var returnStream = new InMemoryRandomAccessStream();
            await RandomAccessStream.CopyAsync(stream.GetInputStreamAt(0), returnStream.GetOutputStreamAt(0));
            await returnStream.FlushAsync();
            var protectedStram = await DownloadManager.ProtectPDFStream(stream);
            await RandomAccessStream.CopyAndCloseAsync(protectedStram.GetInputStreamAt(0), stream.GetOutputStreamAt(0));
            await protectedStram.FlushAsync();
            await stream.FlushAsync();
            protectedStram.Dispose();
            stream.Dispose();

            var pdfStream = new MagazineData();
            pdfStream.folderUrl = folder.Path + "\\";
            pdfStream.stream = returnStream;

            //var fileHandle =
            //    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\testmagazine.pdf");

            //pdfStream.folderUrl = "C:\\Users\\Dorin\\Documents\\Magazines\\wind_355\\";
            //pdfStream.stream = await fileHandle.OpenReadAsync();

            return pdfStream.stream;
        }

        public async Task<IRandomAccessStream> DownloadPDFAsync(LibrelioLocalUrl magUrl, StorageFolder folder, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            //HttpClient client = new HttpClient();

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, magUrl.Url);

            //int read = 0;
            //int offset = 0;
            //byte[] responseBuffer = new byte[1024];

            //var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);
            //response.EnsureSuccessStatusCode();

            //var length = response.Content.Headers.ContentLength;

            //cancelToken.ThrowIfCancellationRequested();

            //var stream = new InMemoryRandomAccessStream();

            //using (var responseStream = await response.Content.ReadAsStreamAsync())
            //{
            //    do
            //    {
            //        cancelToken.ThrowIfCancellationRequested();

            //        read = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

            //        cancelToken.ThrowIfCancellationRequested();

            //        await stream.AsStream().WriteAsync(responseBuffer, 0, read);

            //        offset += read;
            //        uint val = (uint)(offset * 100 / length);
            //        if (val >= 100) val = 99;
            //        if (val <= 0) val = 1;
            //        progress.Report((int)val);
            //    }
            //    while (read != 0);
            //}

            //progress.Report(100);

            //await stream.FlushAsync();

            ////var folder = await AddMagazineFolderStructure(magUrl);
            ////var folder = await StorageFolder.GetFolderFromPathAsync(folderUrl);
            var file = await folder.CreateFileAsync(magUrl.FullName, CreationCollisionOption.ReplaceExisting);

            //using (var protectedStream = await DownloadManager.ProtectPDFStream(stream))
            //using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
            ////using (var unprotectedStream = await DownloadManager.UnprotectPDFStream(protectedStream))
            //{

            //    await RandomAccessStream.CopyAsync(protectedStream, fileStream.GetOutputStreamAt(0));

            //    await fileStream.FlushAsync();
            //}

            progress.Report(0);
            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(magUrl.Url), file);

            await HandleDownloadAsync(download, true, progress, cancelToken);

            progress.Report(100);

            var stream = await download.ResultFile.OpenAsync(FileAccessMode.ReadWrite);
            var protectedStram = await DownloadManager.ProtectPDFStream(stream);
            await RandomAccessStream.CopyAndCloseAsync(protectedStram.GetInputStreamAt(0), stream.GetOutputStreamAt(0));
            await protectedStram.FlushAsync();
            await stream.FlushAsync();
            protectedStram.Dispose();
            stream.Dispose();

            var pdfStream = new MagazineData();
            pdfStream.folderUrl = folder.Path + "\\";
            pdfStream.stream = stream;
            //var fileHandle =
            //    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\testmagazine.pdf");

            //pdfStream.folderUrl = "C:\\Users\\Dorin\\Documents\\Magazines\\wind_355\\";
            //pdfStream.stream = await fileHandle.OpenReadAsync();

            return pdfStream.stream;
        }

        public async Task<StorageFolder> AddMagazineFolderStructure(LibrelioUrl magUrl)
        {
            var currentFolder = _folder;

            var relUrl = magUrl.RelativeUrl;
            var strs = relUrl.Split('/');

            for (int i = 0; i < strs.Length - 1; i++)
            {
                var folder = strs[i];
                if (folder != "")
                {
                    try
                    {
                        var fld = await currentFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
                        currentFolder = fld;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            if (currentFolder != _folder)
            {
            //    var magLocal = new LibrelioLocalUrl(magUrl.Title, magUrl.Subtitle, currentFolder.Path + "\\", 
            //                                        magUrl.FullName, magUrl.AbsoluteUrl, magUrl.RelativeUrl);

            //    if (!UpdataLocalUrl(magLocal))
            //        _magazinesLocalUrl.Add(magLocal);

            //    await AddUpdateMetadataEntry(magLocal);

                return currentFolder;
            }

            return null;
        }

        public async Task MarkAsDownloaded(LibrelioUrl magUrl, StorageFolder currentFolder, bool isd)
        {
            if (currentFolder != _folder)
            {
                var magLocal = new LibrelioLocalUrl(magUrl.Index, magUrl.Title, magUrl.Subtitle, currentFolder.Path + "\\",
                                                    magUrl.FullName, magUrl.AbsoluteUrl, magUrl.RelativeUrl, isd);

                if (!UpdataLocalUrlDownloaded(magLocal))
                    _magazinesLocalUrlDownloaded.Add(magLocal);

                await AddUpdateMetadataDownloadedEntry(magLocal);

                var items = _magazinesLocalUrl.Where(magazine => magazine.FullName.Equals(magLocal.FullName));

                if (items != null && items.Count() > 0)
                {
                    var magazine = items.First();
                    var index = _magazinesLocalUrl.IndexOf(magazine);
                    if (index == -1) return;
                    _magazinesLocalUrl[index] = magLocal;
                }
            }
        }

        public async Task MarkAsDownloaded(LibrelioLocalUrl magUrl, StorageFolder currentFolder)
        {
            if (currentFolder != _folder)
            {
                if (!UpdataLocalUrlDownloaded(magUrl))
                    _magazinesLocalUrlDownloaded.Add(magUrl);

                await AddUpdateMetadataDownloadedEntry(magUrl);

                var items = _magazinesLocalUrl.Where(magazine => magazine.FullName.Equals(magUrl.FullName));

                if (items != null && items.Count() > 0)
                {
                    var magazine = items.First();
                    var index = _magazinesLocalUrl.IndexOf(magazine);
                    if (index == -1) return;
                    _magazinesLocalUrl[index] = magUrl;
                }
            }
        }

        public async Task RemoveMagazineDownloaded(LibrelioLocalUrl magLocal)
        {
            await RemoveDownloadedMetadataEntry(magLocal);
            _magazinesLocalUrlDownloaded.Remove(magLocal);

            var items = _magazinesLocalUrl.Where(magazine => magazine.FullName.Equals(magLocal.FullName));

            if (items != null && items.Count() > 0)
            {
                var magazine = items.First();
                var index = _magazinesLocalUrl.IndexOf(magazine);
                if (index == -1) return;
                _magazinesLocalUrl[index] = DownloadManager.DeleteLocalUrl(magLocal);
            }
        }

        public async Task<StorageFolder> AddMagazineFolderStructure(LibrelioLocalUrl magLocal)
        {
            var currentFolder = _folder;

            var relUrl = magLocal.RelativePath;
            var strs = relUrl.Split('/');

            for (int i = 0; i < strs.Length - 1; i++)
            {
                var folder = strs[i];
                if (folder != "")
                {
                    try
                    {
                        var fld = await currentFolder.CreateFolderAsync(folder, CreationCollisionOption.OpenIfExists);
                        currentFolder = fld;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            if (currentFolder != _folder)
            {
                //    var magLocal = new LibrelioLocalUrl(magUrl.Title, magUrl.Subtitle, currentFolder.Path + "\\", 
                //                                        magUrl.FullName, magUrl.AbsoluteUrl, magUrl.RelativeUrl);

                //    if (!UpdataLocalUrl(magLocal))
                //        _magazinesLocalUrl.Add(magLocal);

                //    await AddUpdateMetadataEntry(magLocal);

                return currentFolder;
            }

            return null;
        }

        public async Task AddUpdateMetadataEntry(LibrelioLocalUrl magLocal)
        {
            if(localXml == null)
                await LoadLocalMetadata();

            string xpath = "/root/mag[url='" + magLocal.Url + "']";
            var mags = localXml.SelectNodes(xpath);

            if (mags.Count > 0)
            {
                mags[0].SelectNodes("index")[0].InnerText = magLocal.Index.ToString();
                if (magLocal.Title == null) magLocal.Title = "";
                mags[0].SelectNodes("title")[0].InnerText = magLocal.Title;
                if (magLocal.Subtitle == null) magLocal.Subtitle = "";
                mags[0].SelectNodes("subtitle")[0].InnerText = magLocal.Subtitle;
                if (magLocal.FolderPath != "ND")
                {
                    mags[0].SelectNodes("path")[0].InnerText = magLocal.FolderPath + magLocal.FullName;
                    mags[0].SelectNodes("metadata")[0].InnerText = magLocal.FolderPath + magLocal.MetadataName;
                }
                else
                {
                    mags[0].SelectNodes("path")[0].InnerText = "ND";
                    mags[0].SelectNodes("metadata")[0].InnerText = "ND";
                }
                mags[0].SelectNodes("url")[0].InnerText = magLocal.Url;
                mags[0].SelectNodes("relPath")[0].InnerText = magLocal.RelativePath;
                mags[0].SelectNodes("sampledownloaded")[0].InnerText = magLocal.IsSampleDownloaded ? "true" : "false";
            }
            else
            {
                var mag = localXml.CreateElement("mag");

                var index = localXml.CreateElement("index");
                index.InnerText = magLocal.Index.ToString();
                mag.AppendChild(index);

                var title = localXml.CreateElement("title");
                if (magLocal.Title == null) magLocal.Title = "";
                title.InnerText = magLocal.Title;
                mag.AppendChild(title);

                var subtitle = localXml.CreateElement("subtitle");
                if (magLocal.Subtitle == null) magLocal.Subtitle = "";
                subtitle.InnerText = magLocal.Subtitle;
                mag.AppendChild(subtitle);

                var path = localXml.CreateElement("path");
                if (magLocal.FolderPath != "ND")
                    path.InnerText = magLocal.FolderPath + magLocal.FullName;
                else
                    path.InnerText = "ND";
                mag.AppendChild(path);

                var metadata = localXml.CreateElement("metadata");
                if (magLocal.FolderPath != "ND")
                    metadata.InnerText = magLocal.FolderPath + magLocal.MetadataName;
                else
                    metadata.InnerText = "ND";
                mag.AppendChild(metadata);

                var url = localXml.CreateElement("url");
                url.InnerText = magLocal.Url;
                mag.AppendChild(url);

                var relPath = localXml.CreateElement("relPath");
                relPath.InnerText = magLocal.RelativePath;
                mag.AppendChild(relPath);

                var isd = localXml.CreateElement("sampledownloaded");
                isd.InnerText = magLocal.IsSampleDownloaded ? "true" : "false";
                mag.AppendChild(isd);

                localXml.GetElementsByTagName("root")[0].AppendChild(mag);
            }

            var xmlfile = await _folder.CreateFileAsync("magazines.metadata", CreationCollisionOption.OpenIfExists);
            await localXml.SaveToFileAsync(xmlfile);
        }

        public async Task AddUpdateMetadataDownloadedEntry(LibrelioLocalUrl magLocal)
        {
            if (localDownloadedXml == null)
                await LoadLocalMetadataDownloaded();

            string xpath = "/root/mag[url='" + magLocal.Url + "']";
            var mags = localDownloadedXml.SelectNodes(xpath);

            if (mags.Count > 0)
            {
                mags[0].SelectNodes("index")[0].InnerText = magLocal.Index.ToString();
                if (magLocal.Title == null) magLocal.Title = "";
                mags[0].SelectNodes("title")[0].InnerText = magLocal.Title;
                if (magLocal.Subtitle == null) magLocal.Subtitle = "";
                mags[0].SelectNodes("subtitle")[0].InnerText = magLocal.Subtitle;
                if (magLocal.FolderPath != "ND")
                {
                    mags[0].SelectNodes("path")[0].InnerText = magLocal.FolderPath + magLocal.FullName;
                    mags[0].SelectNodes("metadata")[0].InnerText = magLocal.FolderPath + magLocal.MetadataName;
                }
                else
                {
                    mags[0].SelectNodes("path")[0].InnerText = "ND";
                    mags[0].SelectNodes("metadata")[0].InnerText = "ND";
                }
                mags[0].SelectNodes("url")[0].InnerText = magLocal.Url;
                mags[0].SelectNodes("relPath")[0].InnerText = magLocal.RelativePath;
                mags[0].SelectNodes("sampledownloaded")[0].InnerText = magLocal.IsSampleDownloaded ? "true" : "false";
            }
            else
            {
                var mag = localDownloadedXml.CreateElement("mag");

                var index = localDownloadedXml.CreateElement("index");
                index.InnerText = magLocal.Index.ToString();
                mag.AppendChild(index);

                var title = localDownloadedXml.CreateElement("title");
                if (magLocal.Title == null) magLocal.Title = "";
                title.InnerText = magLocal.Title;
                mag.AppendChild(title);

                var subtitle = localDownloadedXml.CreateElement("subtitle");
                if (magLocal.Subtitle == null) magLocal.Subtitle = "";
                subtitle.InnerText = magLocal.Subtitle;
                mag.AppendChild(subtitle);

                var path = localDownloadedXml.CreateElement("path");
                if (magLocal.FolderPath != "ND")
                    path.InnerText = magLocal.FolderPath + magLocal.FullName;
                else
                    path.InnerText = "ND";
                mag.AppendChild(path);

                var metadata = localDownloadedXml.CreateElement("metadata");
                if (magLocal.FolderPath != "ND")
                    metadata.InnerText = magLocal.FolderPath + magLocal.MetadataName;
                else
                    metadata.InnerText = "ND";
                mag.AppendChild(metadata);

                var url = localDownloadedXml.CreateElement("url");
                url.InnerText = magLocal.Url;
                mag.AppendChild(url);

                var relPath = localDownloadedXml.CreateElement("relPath");
                relPath.InnerText = magLocal.RelativePath;
                mag.AppendChild(relPath);

                var isd = localDownloadedXml.CreateElement("sampledownloaded");
                isd.InnerText = magLocal.IsSampleDownloaded ? "true" : "false";
                mag.AppendChild(isd);

                localDownloadedXml.GetElementsByTagName("root")[0].AppendChild(mag);
            }

            var xmlfile = await _folder.CreateFileAsync("magazines_downloaded.metadata", CreationCollisionOption.OpenIfExists);
            await localDownloadedXml.SaveToFileAsync(xmlfile);
        }

        public async Task ClearMetadataEntry()
        {
            if (localXml == null)
                await LoadLocalMetadata();

            string xpath = "/root";
            var mags = localXml.SelectNodes(xpath);
            var root = mags[0];

            while (root.ChildNodes.Count > 0)
            {
                root.RemoveChild(root.ChildNodes.Item(0));
            }
        }

        public async Task RemoveDownloadedMetadataEntry(LibrelioLocalUrl magLocal)
        {
            if (localDownloadedXml == null)
                await LoadLocalMetadataDownloaded();

            string xpath = "/root/mag[url='" + magLocal.Url + "']";
            var mags = localDownloadedXml.SelectNodes(xpath);

            if (mags.Count > 0)
            {
                var parent = mags[0].ParentNode;
                parent.RemoveChild(mags[0]);
            }

            var xmlfile = await _folder.CreateFileAsync("magazines_downloaded.metadata", CreationCollisionOption.OpenIfExists);
            await localDownloadedXml.SaveToFileAsync(xmlfile);
        }

        public IList<LibrelioUrl> GetAssets(string magazineName)
        {
            return null;
        }

        private async Task ReadPList(XmlDocument plist)
        {
            _magazinesUrl.Clear();

            var items = plist.SelectNodes("/plist/array/dict");
            for (int i = 0; i < items.Count; i++)
            {
                var dict = items[i];
                LibrelioUrl url = null;
                string tite = "";
                string subtitle = "";

                foreach (var key in dict.SelectNodes("key"))
                {
                    if (key.InnerText == "FileName")
                    {
                        var relUrl = GetValue(key);

                        if (relUrl != "")
                            url = new LibrelioUrl(i, this._path, relUrl);
                    }
                    else if (key.InnerText == "Title")
                    {
                        tite = GetValue(key);
                    }
                    else if (key.InnerText == "Subtitle")
                    {
                        subtitle = GetValue(key);
                    }
                }

                if (url != null && tite != "")
                    url.Title = tite;
                if (url != null && subtitle != "")
                    url.Subtitle = subtitle;
                if (url != null)
                    _magazinesUrl.Add(url);
            }

            await UpdateLocalMetadataFromPLIST();
        }

        private string GetValue(IXmlNode key)
        {
            var node = key;

            while (node.NextSibling != null && node.NodeName != "string")
                node = node.NextSibling;
            if (node != null)
                return node.InnerText;
            else
                return "";
        }

        private bool UpdataLocalUrl(LibrelioLocalUrl url)
        {
            foreach (var local in _magazinesLocalUrl)
            {
                if (local.FullName == url.FullName)
                {
                    var i = _magazinesLocalUrl.IndexOf(local);
                    _magazinesLocalUrl[i] = url;
                    return true;
                }
            }

            return false;
        }

        private bool UpdataLocalUrlDownloaded(LibrelioLocalUrl url)
        {
            foreach (var local in _magazinesLocalUrlDownloaded)
            {
                if (local.FullName == url.FullName)
                {
                    var i = _magazinesLocalUrlDownloaded.IndexOf(local);
                    _magazinesLocalUrlDownloaded[i] = url;
                    return true;
                }
            }

            return false;
        }

        private async Task UpdateLocalMetadataFromPLIST()
        {
            await ClearMetadataEntry();
            _magazinesLocalUrl.Clear();

            for (int i = 0; i < _magazinesUrl.Count; i++)
            {
                var url = _magazinesUrl[i];
                var local = DownloadManager.ConvertToLocalUrl(url);

                _magazinesLocalUrl.Add(local);

                await AddUpdateMetadataEntry(local);
            }

            foreach (var item in _magazinesLocalUrlDownloaded)
            {
                var items = _magazinesLocalUrl.Where(magazine => magazine.FullName.Equals(item.FullName));

                if (items != null && items.Count() > 0)
                {
                    var magazine = items.First();
                    var index = _magazinesLocalUrl.IndexOf(magazine);
                    if (index == -1) continue;
                    _magazinesLocalUrl[index] = item;
                }
            }
        }

        private async Task LoadLocalMetadata()
        {
            if (localXml == null)
            {
                var file = await _folder.CreateFileAsync("magazines.metadata", CreationCollisionOption.OpenIfExists);
                Task task = null;
                try
                {
                    localXml = await XmlDocument.LoadFromFileAsync(file);
                }
                catch
                {
                    localXml = new XmlDocument();
                    var root = localXml.CreateElement("root");
                    localXml.AppendChild(root);

                    task = localXml.SaveToFileAsync(file).AsTask();
                }

                if (task != null)
                    await task;
            }

            _magazinesLocalUrl.Clear();
            var mags = localXml.SelectNodes("/root/mag");
            bool error = false;
            foreach (var mag in mags)
            {
                try
                {
                    _magazinesLocalUrl.Add(DownloadManager.GetLocalUrl(mag));
                }
                catch
                {
                    error = true;
                    break;
                }
            }

            if (error)
            {
                var file = await _folder.CreateFileAsync("magazines.metadata", CreationCollisionOption.ReplaceExisting);

                localXml = new XmlDocument();
                var root = localXml.CreateElement("root");
                localXml.AppendChild(root);

                var task = localXml.SaveToFileAsync(file).AsTask();

                _magazinesLocalUrl.Clear();
            }

            if (_magazinesLocalUrl.Count == 0)
            {
                var fileHandle =
                            await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"CustomizationAssets\Magazines.plist");
                var stream = await fileHandle.OpenReadAsync();
                var dataReader = new DataReader(stream.GetInputStreamAt(0));
                var size = await dataReader.LoadAsync((uint)stream.Size);
                var str = dataReader.ReadString(size);
                dataReader.DetachStream();
                stream.Dispose();
                stream = null;
                if (str.Contains("<!DOCTYPE"))
                {
                    var pos = str.IndexOf("<!DOCTYPE");
                    var end = str.IndexOf(">", pos + 7);
                    if (end >= 0)
                        str = str.Remove(pos, end - pos + 1);
                }
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(str);
                await ReadPList(xml);

                var folder = ApplicationData.Current.LocalFolder;
                try
                {
                    // Set query options to create groups of files within result
                    var queryOptions = new QueryOptions(Windows.Storage.Search.CommonFolderQuery.DefaultQuery);
                    queryOptions.UserSearchFilter = "System.FileName:=Covers";

                    // Create query and retrieve result
                    StorageFolderQueryResult queryResult = folder.CreateFolderQueryWithOptions(queryOptions);
                    IReadOnlyList<StorageFolder> folders = await queryResult.GetFoldersAsync();

                    if (folders.Count != 1)
                    {
                        await Utils.Utils.LoadDefaultData();
                    }
                }
                catch { }
            }
        }

        private async Task LoadLocalMetadataDownloaded()
        {
            if (localDownloadedXml == null)
            {
                var file = await _folder.CreateFileAsync("magazines_downloaded.metadata", CreationCollisionOption.OpenIfExists);
                Task task = null;
                try
                {
                    localDownloadedXml = await XmlDocument.LoadFromFileAsync(file);
                }
                catch
                {
                    localDownloadedXml = new XmlDocument();
                    var root = localDownloadedXml.CreateElement("root");
                    localDownloadedXml.AppendChild(root);

                    task = localDownloadedXml.SaveToFileAsync(file).AsTask();
                }

                if (task != null)
                    await task;
            }

            _magazinesLocalUrlDownloaded.Clear();
            var mags = localDownloadedXml.SelectNodes("/root/mag");
            bool error = false;
            foreach (var mag in mags)
            {
                try
                {
                    _magazinesLocalUrlDownloaded.Add(DownloadManager.GetLocalUrl(mag));
                }
                catch
                {
                    error = true;
                    break;
                }
            }

            if (error)
            {
                var file = await _folder.CreateFileAsync("magazines_downloaded.metadata", CreationCollisionOption.ReplaceExisting);

                localDownloadedXml = new XmlDocument();
                var root = localDownloadedXml.CreateElement("root");
                localDownloadedXml.AppendChild(root);

                var task = localDownloadedXml.SaveToFileAsync(file).AsTask();

                _magazinesLocalUrlDownloaded.Clear();
            }

            foreach (var item in _magazinesLocalUrlDownloaded)
            {
                var items = _magazinesLocalUrl.Where(magazine => magazine.FullName.Equals(item.FullName));

                if (items != null && items.Count() > 0)
                {
                    var magazine = items.First();
                    var index = _magazinesLocalUrl.IndexOf(magazine);
                    if (index == -1) continue; 
                    _magazinesLocalUrl[index] = item;
                }
            }
        }

        private async Task GetUrlsFromPDF(IRandomAccessStream stream)
        {
            using (var dataReader = new DataReader(stream.GetInputStreamAt(0)))
            {
                uint u = await dataReader.LoadAsync((uint)stream.Size);
                IBuffer buffer = dataReader.ReadBuffer(u);

                GetPDFLinks(buffer);

                TimeSpan t = new TimeSpan(0, 0, 1);
                await Task.Delay(t);
            }
        }

        private void GetPDFLinks(IBuffer buffer)
        {
            var document = Document.Create(
                        buffer, // - file
                        DocumentType.PDF, // type
                        72 // - dpi
                      );

            links.Clear();

            var linkVistor = new LinkInfoVisitor();
            linkVistor.OnURILink += linkVistor_OnURILink;

            for (int i = 0; i < document.PageCount; i++)
            {
                var ls = document.GetLinks(i);

                for (int j = 0; j < ls.Count; j++)
                {
                    ls[j].AcceptVisitor(linkVistor);

                }
            }
        }

        private void linkVistor_OnURILink(LinkInfoVisitor __param0, LinkInfoURI __param1)
        {
            int pos = 0;
            int pos1 = 0;
            string str = __param1.URI;
            if (str.Contains("localhost"))
            {
                if (DownloadManager.IsImage(str))
                {
                    pos = str.LastIndexOf('_');
                    if (str.Contains(".jpg") && pos > 0)
                    {
                        pos1 = str.IndexOf(".jpg");
                    }

                    if (pos >= 0 && pos1 >= 0)
                    {
                        var s = str.Substring(0, pos1);
                        var ss = s.Substring(pos + 1);
                        int x = -1;
                        try
                        {
                            x = Convert.ToInt32(ss);
                        }
                        catch { }

                        if (x > 1 && x < 50)
                        {
                            for (int i = 1; i < x; i++)
                                links.Add(str.Replace("_" + ss + ".", "_" + Convert.ToString(i) + "."));
                        }
                    }

                    links.Add(str);
                }
                else if (DownloadManager.IsVideo(str))
                {
                    pos1 = str.IndexOf(".mp4");
                    str = str.Substring(0, pos1) + ".mp4";

                    links.Add(str);
                }
            }
        }

        private async Task<StorageFile> DownloadPDFAssetsAsync(LibrelioLocalUrl magUrl, IList<string> list, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(magUrl.FolderPath.Substring(0, magUrl.FolderPath.Length-1));
            var file = await folder.CreateFileAsync(magUrl.MetadataName, CreationCollisionOption.ReplaceExisting);
            var xml = new XmlDocument();
            var root = xml.CreateElement("root");
            var name = xml.CreateElement("name");
            name.InnerText = magUrl.FullName;
            var date = xml.CreateElement("date");
            date.InnerText = DateTime.Today.Month.ToString() + "/" + DateTime.Today.Day.ToString() + "/" + DateTime.Today.Year.ToString();
            xml.AppendChild(root);
            root.AppendChild(name);
            root.AppendChild(date);
            await xml.SaveToFileAsync(file);
            cancelToken.ThrowIfCancellationRequested();

            for (int i = 0; i < list.Count; i++)
            {
                var loader = new ResourceLoader();
                StatusText = loader.GetString("downloading") + " " + (i + 2) + "/" + (list.Count + 1);
                var url = list[i];
                cancelToken.ThrowIfCancellationRequested();

                string absLink = url;
                var pos = absLink.IndexOf('?');
                if (pos >= 0) absLink = url.Substring(0, pos);
                string fileName = absLink.Replace("http://localhost/", "");
                string linkString = "";
                linkString = folder.Path + "\\" + absLink.Replace("http://localhost/", "");
                pos = magUrl.Url.LastIndexOf('/');
                var assetUrl = magUrl.Url.Substring(0, pos + 1);
                absLink = absLink.Replace("http://localhost/", assetUrl);

                var sampleFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

                await DownloadFileAsyncWithProgress(absLink, sampleFile, progress, cancelToken);

                var asset = xml.CreateElement("asset");
                asset.InnerText = linkString;
                root.AppendChild(asset);
                await xml.SaveToFileAsync(file);
            }

            return file;
        }

        private async Task DownloadFileAsyncWithProgress(string url, StorageFile pdfFile, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            //HttpClient client = new HttpClient();

            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            //int read = 0;
            //int offset = 0;
            //byte[] responseBuffer = new byte[2000];

            //var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancelToken);
            //response.EnsureSuccessStatusCode();

            //var length = response.Content.Headers.ContentLength;

            //byte[] img = await response.Content.ReadAsByteArrayAsync();

            //cancelToken.ThrowIfCancellationRequested();

            //var stream = new InMemoryRandomAccessStream();
            //DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));

            //writer.WriteBytes(img);

            //await writer.StoreAsync();


            //var task1 = client.GetStreamAsync(new Uri(url));

            //var cancelation = new CancellationTokenSource();

            //var task2 = Task.Run(async () =>
            //{
            //    for (int i = 0; i < length; i += 9000)
            //    {
            //        await Task.Delay(1);
            //        int val = (int)(i * 100 / length);
            //        progress.Report(val);

            //        if (cancelation.Token.IsCancellationRequested)
            //        {
            //            progress.Report(99);
            //            return;
            //        }
            //    }
            //}, cancelation.Token);

            //var result = await task1;

            //cancelation.Cancel();
            //progress.Report(99);

            //await result.CopyToAsync(stream.AsStream());
            //await stream.FlushAsync();

            //await Task.Delay(670);

            //var responseStream = await response.Content.ReadAsStreamAsync();

            //do
            //{
            //    cancelToken.ThrowIfCancellationRequested();

            //    read = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

            //    cancelToken.ThrowIfCancellationRequested();

            //    await stream.AsStream().WriteAsync(responseBuffer, 0, read);

            //    offset += read;
            //    int val = (int)(offset * 100 / length);
            //    progress.Report(val);
            //}
            //while (read > 0);

            //await responseStream.FlushAsync();
            //responseStream.Dispose();
            //responseStream = null;
            //await stream.FlushAsync();

            //var protectedStream = await DownloadManager.ProtectPDFStream(stream);
            //var fileStream = await pdfFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            //using (var unprotectedStream = await DownloadManager.UnprotectPDFStream(protectedStream))

            //await RandomAccessStream.CopyAndCloseAsync(stream.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));

            //await stream.FlushAsync();
            //stream.Dispose();
            ///stream = null;
            //await protectedStream.FlushAsync();
            //protectedStream.Dispose();
            //protectedStream = null;
            //await fileStream.FlushAsync();
            //fileStream.Dispose();
            //fileStream = null;
            //pdfFile = null;

            progress.Report(0);
            BackgroundDownloader downloader = new BackgroundDownloader();
            DownloadOperation download = downloader.CreateDownload(new Uri(url), pdfFile);

            await HandleDownloadAsync(download,  true, progress, cancelToken);

            progress.Report(100);

            var stream = await download.ResultFile.OpenAsync(FileAccessMode.ReadWrite);
            var protectedStram = await DownloadManager.ProtectPDFStream(stream);
            await RandomAccessStream.CopyAndCloseAsync(protectedStram.GetInputStreamAt(0), stream.GetOutputStreamAt(0));
            await protectedStram.FlushAsync();
            await stream.FlushAsync();
            protectedStram.Dispose();
            stream.Dispose();
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var app = Application.Current as App;

            try
            {
                // Store the download so we can pause/resume. 
                app.activeDownloads.Add(download);

                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>((operation) =>
                {
                    if (operation.Progress.TotalBytesToReceive != 0)
                    {
                        ulong val = (ulong)(operation.Progress.BytesReceived * 100 / operation.Progress.TotalBytesToReceive);
                        progress.Report((int)val);
                    }
                });

                if (start)
                {
                    // Start the download and attach a progress handler. 
                    await download.StartAsync().AsTask(cancelToken, progressCallback);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler. 
                    await download.AttachAsync().AsTask(cancelToken, progressCallback);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
                app.activeDownloads.Remove(download);
            }
        } 
    }
}