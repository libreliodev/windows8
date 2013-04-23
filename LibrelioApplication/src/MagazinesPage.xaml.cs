using LibrelioApplication.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Resources;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Collections.ObjectModel;


// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace LibrelioApplication
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : LibrelioApplication.Common.LayoutAwarePage
    {
        private MagazineManager manager = null;

        private bool isOpening = false;

        public ItemsPage()
        {
            this.InitializeComponent();
#if TEST_SLIDE
            testSlideshow.Visibility = Windows.UI.Xaml.Visibility.Visible;
#endif
#if TEST_VIDEO
            testVideo.Visibility = Windows.UI.Xaml.Visibility.Visible;
#endif
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Debug.WriteLine("LoadState");
            //var dataSrc = new LibrelioApplication.Data.MagazineDataSource();
            //this.DefaultViewModel["Items"] = dataSrc;
            //// TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var sampleDataGroups = MagazineDataSource.GetGroups((String)navigationParameter);
            var sampleDataGroups = MagazineDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Groups"] = sampleDataGroups;

            purchaseModule.Bought += purchaseModule_Bought;
            
            Debug.WriteLine("LoadState - finished");
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            MagazineDataSource.RemoveGroups();
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                return;
            }
            //// Navigate to the appropriate destination page, configuring the new page
            //// by passing required information as a navigation parameter
            //var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            //this.Frame.Navigate(typeof(SplitPage), groupId);
            if (isOpening) return;
            var item = ((MagazineViewModel)e.ClickedItem);
            if (item.IsOpening) return;
            //sParam += LibrelioApplication.PdfViewPage.qqq;
            var resourceLoader = new ResourceLoader();
            if (!item.IsDownloaded && item.SecondButtonVisible == true) {

                //Utils.Utils.navigateTo(typeof(LibrelioApplication.PdfViewPage));
                var group = MagazineDataSource.GetGroup("All Magazines");
                group.Items.Remove(item);
                var app = Application.Current as App;
                app.needToDownload = true;
                this.Frame.Navigate(typeof(DownloadingPage), "test");

            } else if (item.IsDownloaded) {

                if (manager == null) {

                    manager = new MagazineManager("Magazines");
                    await manager.LoadLocalMagazineList();
                }

                item.IsOpening = true;
                isOpening = true;
                var mag = DownloadManager.GetLocalUrl(manager.MagazineLocalUrl, item.FileName);
                if (mag == null) { isOpening = false; item.IsOpening = false; return; }
                var str = await DownloadManager.OpenPdfFile(mag);
                if (str == null) { isOpening = false; item.IsOpening = false; return; }
                item.IsOpening = false;
                isOpening = false;
                this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = str, folderUrl = mag.FolderPath });

            } else if (item.DownloadOrReadButton == resourceLoader.GetString("download")) {

                if (item.IsPaid)
                {
                    item.DownloadOrReadButton = resourceLoader.GetString("preparing");
                    try
                    {
                        await purchaseModule.Init(item);
                    }
                    catch
                    {
                        purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    item.DownloadOrReadButton = resourceLoader.GetString("download");
                }
                else
                {
                    var group = MagazineDataSource.GetGroup("All Magazines");
                    group.Items.Remove(item);
                    DownloadMagazine param = null;
                    if (manager.MagazineUrl.Count > 0)
                    {
                        var it = manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                        if (it.Count() > 0)
                            param = new DownloadMagazine()
                            {
                                manager = manager,
                                url = it.First()
                            };
                    }
                    else if (manager.MagazineLocalUrl.Count > 0)
                    {
                        var it = manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                        if (it.Count() > 0)
                            param = new DownloadMagazine()
                            {
                                manager = manager,
                                url = DownloadManager.ConvertFromLocalUrl(it.First())
                            };
                    }

                    if (param != null)
                    {
                        var app = Application.Current as App;
                        app.needToDownload = true;
                        this.Frame.Navigate(typeof(DownloadingPage), param);
                    }
                }
            }
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (isOpening) return;
            var button = sender as Button;
            var item = ((MagazineViewModel)button.DataContext);
            var resourceLoader = new ResourceLoader();
            if (item.DownloadOrReadButton == resourceLoader.GetString("read"))
            {
                isOpening = true;
                item.DownloadOrReadButton = resourceLoader.GetString("opening");

                if (manager == null)
                {
                    manager = new MagazineManager("Magazines");
                    await manager.LoadLocalMagazineList();
                }

                var mag = DownloadManager.GetLocalUrl(manager.MagazineLocalUrl, item.FileName);
                if (mag == null) { isOpening = false; return; }
                var str = await DownloadManager.OpenPdfFile(mag);
                if (str == null) { isOpening = false; return; }
                isOpening = false;
                item.DownloadOrReadButton = resourceLoader.GetString("read");
                this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = str, folderUrl = mag.FolderPath });
            }
            else if (item.DownloadOrReadButton == resourceLoader.GetString("download"))
            {
                if (item.IsPaid)
                {
                    item.DownloadOrReadButton = resourceLoader.GetString("preparing");
                    try
                    {
                        await purchaseModule.Init(item);
                    }
                    catch
                    {
                        purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    item.DownloadOrReadButton = resourceLoader.GetString("download");
                }
                else
                {
                    var group = MagazineDataSource.GetGroup("All Magazines");
                    group.Items.Remove(item);
                    DownloadMagazine param = null;
                    if (manager.MagazineUrl.Count > 0)
                    {
                        var it = manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                        if (it.Count() > 0)
                            param = new DownloadMagazine()
                            {
                                manager = manager,
                                url = it.First()
                            };
                    }
                    else if (manager.MagazineLocalUrl.Count > 0)
                    {
                        var it = manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                        if (it.Count() > 0)
                            param = new DownloadMagazine()
                            {
                                manager = manager,
                                url = DownloadManager.ConvertFromLocalUrl(it.First())
                            };
                    }

                    if (param != null)
                    {
                        var app = Application.Current as App;
                        app.needToDownload = true;
                        this.Frame.Navigate(typeof(DownloadingPage), param);
                    }
                }
            }
        }

        private async void purchaseModule_Bought(object sender, string url)
        {
            var purchase = sender as PurchaseModule;

            var item = purchase.GetCurrentItem();
            var resourceLoader = new ResourceLoader();
            item.DownloadOrReadButton = resourceLoader.GetString("opening");

            if (manager == null)
            {
                manager = new MagazineManager("Magazines");
                await manager.LoadLocalMagazineList();
            }

            var group = MagazineDataSource.GetGroup("All Magazines");
            group.Items.Remove(item);
            DownloadMagazine param = null;
            if (manager.MagazineUrl.Count > 0)
            {
                var it = manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        manager = manager,
                        url = it.First(),
                        redirectUrl = url
                    };
            }
            else if (manager.MagazineLocalUrl.Count > 0)
            {
                var it = manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        manager = manager,
                        url = DownloadManager.ConvertFromLocalUrl(it.First()),
                        redirectUrl = url
                    };
            }
            item.DownloadOrReadButton = resourceLoader.GetString("read");

            if (param != null)
            {
                var app = Application.Current as App;
                app.needToDownload = true;
                this.Frame.Navigate(typeof(DownloadingPage), param);
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (isOpening) return;
            var button = sender as Button;
            var item = ((MagazineViewModel)button.DataContext);
            var resourceLoader = new ResourceLoader();
            if (item.SampleOrDeleteButton == resourceLoader.GetString("delete"))
            {
                item.SampleOrDeleteButton = resourceLoader.GetString("deleting");

                if (manager == null)
                {
                    manager = new MagazineManager("Magazines");
                    await manager.LoadLocalMagazineList();
                }

                var mag = DownloadManager.GetLocalUrl(manager.MagazineLocalUrl, item.FileName);
                var folder = await StorageFolder.GetFolderFromPathAsync(mag.FolderPath);
                var files = await folder.GetFilesAsync();
                foreach (var file in files) {

                    try {

                        await file.DeleteAsync(StorageDeleteOption.PermanentDelete);

                    } catch { }
                }

                try {

                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);

                } catch { }

                mag = DownloadManager.DeleteLocalUrl(mag);
                await manager.AddUpdateMetadataEntry(mag);
                var title = item.FileName;
                var group = MagazineDataSource.GetGroup("My Magazines");
                group.Items.Remove(item);
                if (group.Items.Count == 0)
                {
                    group = MagazineDataSource.GetGroup("All Magazines");
                    if (group.Items.Count != 0)
                    {
                        snappedView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                        noMagazineSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        titleSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        snappedGridView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        var source = new ObservableCollection<MagazineViewModel>();
                        source.Add(group.Items.First());
                        itemListView.ItemsSource = source;
                    }
                }
                group = MagazineDataSource.GetGroup("All Magazines");
                item = group.Items.Where((magazine) => magazine.FileName.Equals(title)).First();
                if (item != null)
                {
                    item.IsDownloaded = false;
                }
            }
            else if (item.SampleOrDeleteButton == resourceLoader.GetString("sample"))
            {
                var group = MagazineDataSource.GetGroup("All Magazines");
                group.Items.Remove(item);
                var app = Application.Current as App;
                app.needToDownload = true;
                this.Frame.Navigate(typeof(DownloadingPage), "test");
            }
        }

        private void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                var app = Application.Current as App;
                if (!app.loadedPList)
                {
                    try
                    {
                        manager = await MagazineDataSource.LoadMagazinesAsync();
                    }
                    catch
                    {
                        return;
                    }
                    app.loadedPList = true;
                }
                else
                {
                    try
                    {
                        manager = await MagazineDataSource.LoadLocalMagazinesAsync();
                    }
                    catch
                    {
                        return;
                    }
                }

                var group = MagazineDataSource.GetGroup("My Magazines");
                if (group.Items.Count == 0)
                {
                    group = MagazineDataSource.GetGroup("All Magazines");
                    if (group.Items.Count != 0)
                    {
                        var source = new ObservableCollection<MagazineViewModel>();
                        source.Add(group.Items.First());
                        itemListView.ItemsSource = source;
                    }
                }
                else
                {
                    snappedView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                    noMagazineSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    titleSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    snappedGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                    if (group.Items.Count != 0)
                    {
                        snappedGridView.ItemsSource = group.Items;
                    }
                }

                try
                {
                    await UpdateTile();
                }
                catch { }
            });
        }

        private async Task UpdateTile()
        {
            var group = MagazineDataSource.GetGroup("All Magazines");
            if (group.Items.Count == 0) return;

            var item = group.Items[0];
            var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideSmallImageAndText04);

            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = item.Title;
            tileTextAttributes[1].InnerText = item.Subtitle;

            XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
            var tempFolder = ApplicationData.Current.LocalFolder;
            var imageFile = await tempFolder.CreateFileAsync("firstpge.png", CreationCollisionOption.ReplaceExisting);
            //var fileHandle =
            //        await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\firstpge.png");

            //((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/WideLogo.png");
            //((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "background");
            ((XmlElement)tileImageAttributes[0]).SetAttribute("src", item.Thumbnail);
            ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "magazine image");

            //XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText04);
            //XmlNodeList squareTileTextAttributes = squareTileXml.GetElementsByTagName("text");
            //squareTileTextAttributes[0].AppendChild(squareTileXml.CreateTextNode("Hello World! My very own tile notification"));
            //IXmlNode node = tileXml.ImportNode(squareTileXml.GetElementsByTagName("binding").Item(0), true);
            //tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);

            //tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(10);

            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }

        private async void Appbar_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null) {
                string tag = (string)b.Tag;
                switch (tag) {
                    case "Refresh":
                        {
                            MagazineDataSource.RemoveGroups();
                            manager = await MagazineDataSource.LoadMagazinesAsync();
                            break;
                        }
                    //case "Slideshow":
                    //    //msgBox if need:
                    //    Utils.Utils.navigateTo(typeof(LibrelioApplication.SlideShowPage), "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white");
                    //    break;
                    //case "Video":
                    //    Utils.Utils.navigateTo(typeof(LibrelioApplication.VideoPage), "http://localhost/test_move1.mov?waplay=auto1");
                    //    break;
                }

            }
            
        }

        private T FindChild<T>(UIElement element, Func<T, bool> isObject)
                     where T : UIElement
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var uiElement = VisualTreeHelper.GetChild(element, i) as UIElement;
                var child = uiElement as T;
                if (child != null)
                {
                    if (isObject(child))
                        return child;
                }

                var result = FindChild(uiElement, isObject);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private List<T> FindChilds<T>(UIElement element, Func<T, bool> isObject, List<T> res = null)
                     where T : UIElement
        {
            if( res == null )
                res = new List<T>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var uiElement = VisualTreeHelper.GetChild(element, i) as UIElement;
                var child = uiElement as T;
                if (child != null)
                {
                    if (isObject(child))
                        res.Add(child);
                }

                FindChilds(uiElement, isObject, res);
            }
            return res;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                return;
            }
        }

    }
}
