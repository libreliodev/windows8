using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Storage.BulkAccess;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using MuPDFWinRT;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;


// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace LibrelioApplication
{
    public struct Item
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string FullName { get; set; }
    }

    public class DownloadMagazine
    {
        //public MagazineManager manager { get; set; }
        public LibrelioUrl url { get; set; }
        public bool IsSampleDownloaded { get; set; }
        public string redirectUrl { get; set; }
    }

    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class DownloadingPage : LibrelioApplication.Common.LayoutAwarePage
    {
        CancellationTokenSource cts;

        IList<string> links = new List<string>();
        StorageFolder folder = null;

        //MagazineManager manager = null;

        bool needtoGoBack = false;

        public DownloadingPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            var app = Application.Current as App;
            if (app.needToDownload == false) { needtoGoBack = true; return; }

            app.needToDownload = false;

            var item = navigationParameter as DownloadMagazine;

            if (item != null)
            {
                statusText.Text = "Download in progress";

                var url = item.url;

                LoadSnappedSource();
                //magList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                downloadView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                try
                {
                    var folder = await app.Manager.AddMagazineFolderStructure(url);
                    Windows.UI.Xaml.Media.Imaging.BitmapSource bitmap = null;
                    try
                    {
                        bitmap = await app.Manager.DownloadThumbnailAsync(url, folder);
                    }
                    catch { }
                    if (bitmap == null)
                    {
                        var messageDialog = new MessageDialog("Download failed, please check your internet connection");
                        var commands = new List<UICommand>();
                        var close = new UICommand("Close");
                        close.Invoked = closeCommandHandler; 
                        messageDialog.Commands.Clear();
                        messageDialog.Commands.Add(close);
                        var task = messageDialog.ShowAsync().AsTask();
                        return;
                    }
                    pdfThumbnail.Width = bitmap.PixelWidth * pdfThumbnail.Height / bitmap.PixelHeight;
                    pdfThumbnail.Source = bitmap;

                    pRing.IsActive = false;

                    var progressIndicator = new Progress<int>((value) =>
                    {
                        if (statusText.Text != app.Manager.StatusText)
                            statusText.Text = app.Manager.StatusText;
                        progressBar.Value = value;
                    });
                    cts = new CancellationTokenSource();

                    IRandomAccessStream stream = null;
                    if (item.redirectUrl == null) {

                        stream = await app.Manager.DownloadMagazineAsync(url, folder, item.IsSampleDownloaded, progressIndicator, cts.Token);

                    } else {

                        stream = await app.Manager.DownloadMagazineAsync(url, item.redirectUrl, folder, item.IsSampleDownloaded, progressIndicator, cts.Token);
                    }
                    if (stream == null) return;
                    statusText.Text = "Done.";
                    await app.Manager.MarkAsDownloaded(url, folder, item.IsSampleDownloaded);
                    await Task.Delay(1000);

                    var mag = DownloadManager.GetLocalUrl(app.Manager.MagazineLocalUrl, url.FullName);
                    this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = stream, folderUrl = mag.FolderPath });
                }
                catch (Exception ex)
                {
                    statusText.Text = "Error";
                    if (ex.Message == "Response status code does not indicate success: 403 (Forbidden).")
                    {
                        var messageDialog = new MessageDialog("Download failed, please check your internet connection");
                        var commands = new List<UICommand>();
                        var close = new UICommand("Close");
                        close.Invoked = closeCommandHandler;
                        messageDialog.Commands.Clear();
                        messageDialog.Commands.Add(close);
                        var task = messageDialog.ShowAsync().AsTask();
                        return;
                    }
                    else if (ex.Message == "The operation was canceled.")
                    {
                        var messageDialog = new MessageDialog("Download failed, please check your internet connection");
                        var commands = new List<UICommand>();
                        var close = new UICommand("Close");
                        close.Invoked = closeCommandHandler;
                        messageDialog.Commands.Clear();
                        messageDialog.Commands.Add(close);
                        var task = messageDialog.ShowAsync().AsTask();
                        return;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Unexpected error");
                        var commands = new List<UICommand>();
                        var close = new UICommand("Close");
                        close.Invoked = closeCommandHandler;
                        messageDialog.Commands.Clear();
                        messageDialog.Commands.Add(close);
                        var task = messageDialog.ShowAsync().AsTask();
                        return;
                    }
                }
            }
            else
            {
                //string s = navigationParameter as string;
                //if (s == "test")
                //{
                //    testView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //}
                //else
                //{
                //    needtoGoBack = true;
                //}
            }
        }

        private void closeCommandHandler(IUICommand command)
        {
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void LoadSnappedSource()
        {
            var app = Application.Current as App;
            if (app.NoMagazines)
            {
                itemListView.ItemsSource = app.snappedCollection;
            }
            else
            {
                snappedView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                noMagazineSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                titleSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                snappedGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (app.snappedCollection == null) return;

                if (app.snappedCollection.Count != 0)
                {
                    snappedGridView.ItemsSource = app.snappedCollection;
                }
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void pdfThumbnail_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (needtoGoBack)
                {
                    needtoGoBack = false;
                    if (this.Frame.CanGoBack) this.Frame.GoBack();
                    return;
                }

                var fileHandle =
                    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"CustomizationAssets\application_.xml");

                var xml = await XmlDocument.LoadFromFileAsync(fileHandle);
                var node = xml.SelectSingleNode("/resources/hex[@name='background_color']");
                string color = node.InnerText;
                progressBar.Foreground = Utils.Utils.ColorFromHex(color);

                //manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/niveales/wind/", "Magazines");


                //await manager.LoadPLISTAsync();

                //var mag = new List<Item>();
                //foreach (var url in manager.MagazineUrl)
                //{
                //    mag.Add(new Item { Title = url.Title, Subtitle = url.Subtitle, FullName = url.FullName} );
                //}
                //itemListView.ItemsSource = mag;

                //foreach (var magUrl in manager.MagazineUrl)
                //{
                //    await manager.DownloadMagazineLocalStorage(magUrl);
                //}

                // In order to test this scenario, the WindowsStoreProxy.xml needs to be changed
                // The receipt provided by the CurrentAppSimulator is NOT signed so we cannot verify it
                
                //var receipt1 = await CurrentAppSimulator.RequestProductPurchaseAsync("1234", true);

                //var xmlDocument = new XmlDocument();
                //xmlDocument.LoadXml(receipt1);

                //XmlNodeList elemList1 = xmlDocument.GetElementsByTagName("Receipt");
                //var certID1 = elemList1[0].Attributes.GetNamedItem("CertificateId").NodeValue;

                // We'll use the sample receipt from http://msdn.microsoft.com/en-us/library/windows/apps/jj649137.aspx

                //string str = "";
                //var file = await KnownFolders.DocumentsLibrary.GetFileAsync("receipt.pmd");
                //using (var stream = await file.OpenAsync(FileAccessMode.Read))
                //{
                //    var dataReader = new DataReader(stream.GetInputStreamAt(0));

                //    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                //    var size = await dataReader.LoadAsync((uint)stream.Size);
                //    var receipt = dataReader.ReadString(size);

                //    receipt = Uri.EscapeDataString(receipt);
                //    var url = "http://54.244.231.175/test/win8_verify.php/?receipt=" + receipt;// +"&id=" + certID;
                //    //var url = "https://lic.apps.microsoft.com/licensing/certificateserver/?cid=b809e47cd0110a4db043b3f73e83acd917fe1336";
                //    str = await new HttpClient().GetAsync(url).Result.Content.ReadAsStringAsync();
                //}

                //file = await KnownFolders.DocumentsLibrary.CreateFileAsync("output.pmd", CreationCollisionOption.ReplaceExisting);
                //using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                //{
                //    var dataWriter = new DataWriter(stream.GetOutputStreamAt(0));
                //    dataWriter.WriteString(str);

                //    await dataWriter.StoreAsync();
                //    await dataWriter.FlushAsync();
                //}

                //var metadataFile = await CreateMetadataFile("AntoineAlbeau");

                //var sampleFile = await folder.CreateFileAsync("AntoineAlbeau.pdf", CreationCollisionOption.ReplaceExisting);

                //statusText.Text = "Downloading pdf ... ";
                //var progressIndicator = new Progress<int>((value) => progressBar.Value = value);
                //cts = new CancellationTokenSource();

                //var url = "http://librelio-europe.s3.amazonaws.com/niveales/wind/windfree_albeau2012/AntoineAlbeau.pdf";
                //var stream = await DownloadFileAsyncWithProgress(url, sampleFile, progressIndicator, cts.Token);
                //await GetUrlsFromPDF(stream);

                //statusText.Text = "Downloading assets ... ";
                //await DownloadAssetsAsync(metadataFile);

                //statusText.Text = "Done";

                //await Task.Delay(1000);

                //this.Frame.Navigate(typeof(PdfViewPage), stream);
            }
            catch (HttpRequestException)
            {
            }
        }

        //private async Task<StorageFile> CreateMetadataFile(string name)
        //{
        //    //var roamingFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;
        //    var roamingFolder = KnownFolders.DocumentsLibrary;
        //    var file = await roamingFolder.CreateFileAsync(name + ".pmd", CreationCollisionOption.ReplaceExisting);
        //    folder = await roamingFolder.CreateFolderAsync(name, CreationCollisionOption.GenerateUniqueName);
        //    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);

        //    using (var outputStream = stream.GetOutputStreamAt(0))
        //    using (var dataWriter = new DataWriter(outputStream))
        //    {
        //        string data = name + "\r\n" + DateTime.Today.Month.ToString() + "/" + DateTime.Today.Day.ToString() + "/" + DateTime.Today.Year.ToString() + "\r\n";

        //        dataWriter.WriteString(data);

        //        await dataWriter.StoreAsync();
        //        await outputStream.FlushAsync();
        //    }

        //    return file;
        //}

        //private async Task<IRandomAccessStream> DownloadFileAsyncWithProgress(string url, StorageFile pdfFile, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken)) 
        //{
        //    HttpClient client = new HttpClient();

        //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url); 

        //    int read = 0; 
        //    int offset = 0;
        //    byte[] responseBuffer = new byte[1024];

        //    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelToken);
        //    var length = response.Content.Headers.ContentLength;

        //    cancelToken.ThrowIfCancellationRequested();

        //    var stream = new InMemoryRandomAccessStream();

        //    using (var responseStream = await response.Content.ReadAsStreamAsync())
        //    {
        //        do
        //        {
        //            cancelToken.ThrowIfCancellationRequested();

        //            read = await responseStream.ReadAsync(responseBuffer, 0, responseBuffer.Length);

        //            cancelToken.ThrowIfCancellationRequested();

        //            await stream.AsStream().WriteAsync(responseBuffer, 0, read);

        //            offset += read;
        //            int val = (int)(offset * 100 / length);
        //            progress.Report(val);
        //        }
        //        while (read != 0);
        //    }

        //    await stream.FlushAsync();

        //    using (var protectedStream = await DownloadManager.ProtectPDFStream(stream))
        //    using (var fileStream = await pdfFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
        //    //using (var unprotectedStream = await UnprotectPDFStream(protectedStream))
        //    {

        //        await RandomAccessStream.CopyAsync(protectedStream, fileStream.GetOutputStreamAt(0));

        //        await fileStream.FlushAsync();
        //    }

        //    return stream;
        //}

        //private async Task GetUrlsFromPDF(IRandomAccessStream stream)
        //{
        //    using (var dataReader = new DataReader(stream.GetInputStreamAt(0)))
        //    {
        //        uint u = await dataReader.LoadAsync((uint)stream.Size);
        //        IBuffer buffer = dataReader.ReadBuffer(u);

        //        GetPDFLinks(buffer);

        //        TimeSpan t = new TimeSpan(0, 0, 1);
        //        await Task.Delay(t);
        //    }
        //}

        //private void GetPDFLinks(IBuffer buffer)
        //{
        //    var document = Document.Create(
        //                buffer, // - file
        //                DocumentType.PDF, // type
        //                72 // - dpi
        //              );

        //    var linkVistor = new LinkInfoVisitor();
        //    linkVistor.OnURILink += linkVistor_OnURILink;

        //    for (int i = 0; i < document.PageCount; i++)
        //    {
        //        var links = document.GetLinks(i);

        //        for (int j = 0; j < links.Count; j++)
        //        {
        //            links[j].AcceptVisitor(linkVistor);
                    
        //        }
        //    }
        //}

        //void linkVistor_OnURILink(LinkInfoVisitor __param0, LinkInfoURI __param1)
        //{
        //    string str = __param1.URI;
        //    if (str.Contains("localhost"))
        //    {
        //        links.Add(str);
        //    }
        //}

        //private async Task DownloadAssetsAsync(StorageFile metadataFile)
        //{
        //    var absLinks = new List<string>();

        //    foreach (var link in links)
        //    {
        //        string absLink = link;
        //        var pos = absLink.IndexOf('?');
        //        if (pos >= 0) absLink = link.Substring(0, pos);
        //        string fileName = absLink.Replace("http://localhost/", "");
        //        string linkString = "";
        //        linkString = folder.Path + "\\" + absLink.Replace("http://localhost/", "") + "\r\n";
        //        absLink = absLink.Replace("http://localhost/", "http://librelio-europe.s3.amazonaws.com/niveales/wind/windfree_albeau2012/");
        //        absLinks.Add(absLink);

        //        var progressIndicator = new Progress<int>((value) => progressBar.Value = value);
        //        cts = new CancellationTokenSource();

        //        var sampleFile = await folder.CreateFileAsync(fileName + ".pmd", CreationCollisionOption.ReplaceExisting);

        //        await DownloadFileAsyncWithProgress(absLink, sampleFile, progressIndicator, cts.Token);

        //        using (var fileStream = await metadataFile.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite))
        //        using (var dataReader = new DataReader(fileStream.GetInputStreamAt(0)))
        //        using (var dataWriter = new DataWriter(fileStream.GetOutputStreamAt(0)))
        //        {
        //            var len = await dataReader.LoadAsync((uint)fileStream.Size);
        //            var data = dataReader.ReadString((uint)len);
        //            var size = dataWriter.WriteString(data + linkString);
        //            await fileStream.FlushAsync();
        //            await dataWriter.StoreAsync();
        //        }
        //    }
        //}

        private async void itemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Item item = (Item)e.ClickedItem;

            statusText.Text = "Download in progress";

            var app = Application.Current as App;
            foreach (var url in app.Manager.MagazineUrl)
            {
                if (url.FullName == item.FullName)
                {
                    //magList.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    downloadView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    var folder = await app.Manager.AddMagazineFolderStructure(url);
                    var bitmap = await app.Manager.DownloadThumbnailAsync(url, folder);
                    pdfThumbnail.Width = bitmap.PixelWidth * pdfThumbnail.Height / bitmap.PixelHeight;
                    pdfThumbnail.Source = bitmap;

                    pRing.IsActive = false;

                    var progressIndicator = new Progress<int>((value) =>
                    {
                        if (statusText.Text != app.Manager.StatusText)
                            statusText.Text = app.Manager.StatusText;
                        progressBar.Value = value;
                    });
                    cts = new CancellationTokenSource();

                    try
                    {
                        var stream = await app.Manager.DownloadMagazineAsync(url, folder, false, progressIndicator, cts.Token);
                        if (stream == null) return;
                        statusText.Text = "Done.";
                        await app.Manager.MarkAsDownloaded(url, folder, false);
                        await Task.Delay(1000);

                        var mag = DownloadManager.GetLocalUrl(app.Manager.MagazineLocalUrl, item.FullName);
                        this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = stream, folderUrl = mag.FolderPath });
                    }
                    catch (Exception ex)
                    {
                        statusText.Text = "Error";
                        if (ex.Message == "Response status code does not indicate success: 403 (Forbidden).")
                        {
                            var messageDialog = new MessageDialog("This is a paid app. You need to purchase it first");
                            var task = messageDialog.ShowAsync().AsTask();
                            return;
                        }
                        else if (ex.Message == "The operation was canceled.")
                        {
                            int x = 0;
                        }
                        else
                        {
                            var messageDialog = new MessageDialog("Unexpected error");
                            var task = messageDialog.ShowAsync().AsTask();
                            return;
                        }
                    }

                    return;
                }
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //testView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //magList.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                //manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/" + app.ClientName + "/" + app.MagazineName + "/", "Magazines");

                var app = Application.Current as App;
                if (app.Manager.MagazineUrl.Count == 0)
                    await app.Manager.LoadPLISTAsync();

                var mag = new List<Item>();
                foreach (var url in app.Manager.MagazineUrl)
                {
                    mag.Add(new Item { Title = url.Title, Subtitle = url.Subtitle, FullName = url.FullName });
                }
                itemListView.ItemsSource = mag;
            }
            catch
            {
            }
        }

        //private async void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    //var url = urlBox.Text;
        //    var url = "http://download.librelio.com/downloads/win8_verify.php";
        //    testOutput.Text = "Wait";

        //    string str = "";
        //    var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
        //    using (var stream = await file.OpenAsync(FileAccessMode.Read))
        //    {
        //        var dataReader = new DataReader(stream.GetInputStreamAt(0));

        //        dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
        //        var size = await dataReader.LoadAsync((uint)stream.Size);
        //        var receipt = dataReader.ReadString(size);

        //        receipt = Uri.EscapeDataString(receipt);
        //        var productId = "wind_358_";
        //        url += "?receipt=" + receipt + "&product_id=" + productId + "&urlstring=" + "niveales/wind/wind_358/wind_358_.pdf";
        //        try
        //        {
        //            str = await new HttpClient().GetAsync(url).Result.Content.ReadAsStringAsync();
        //            testOutput.Text = str;
        //        }
        //        catch
        //        {
        //            testOutput.Text = "error";
        //        }
        //    }
        //}

        private void NavBack(object sender, RoutedEventArgs e)
        {
            if (cts != null) cts.Cancel();
            if (this.Frame.CanGoBack)
                this.Frame.GoBack();
        }

        private void Grid_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            //if (ApplicationView.Value == ApplicationViewState.Snapped)
            //{
            //    if (cts != null) cts.Cancel();
            //    if (this.Frame.CanGoBack)
            //        this.Frame.GoBack();
            //}
        }
    }
}
