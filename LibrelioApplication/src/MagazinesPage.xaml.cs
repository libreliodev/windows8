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
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using Windows.Storage.Search;
using System.Net.Http;


// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace LibrelioApplication
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : LibrelioApplication.Common.LayoutAwarePage
    {
        //private MagazineManager manager = null;

        private bool isOpening = false;
        private bool isRefreshing = false;

        private CancellationTokenSource cts = null;

        public ItemsPage()
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
            purchaseModule.GetSample += purchaseModule_GetSample;
            purchaseModule.Open += purchaseModule_Open;
            purchaseModule.Delete += purchaseModule_Delete;
            
            Debug.WriteLine("LoadState - finished");
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //ClearCovers();
            var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                {
                    MagazineDataSource.RemoveGroups();
                });
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
            var app = Application.Current as App;

            var loader = new ResourceLoader();
            if (item.Group == MagazineDataSource.GetGroup(loader.GetString("all_magazines")))
            {
                if (item.IsPaid)
                {
                    if (item.IsDownloaded && !item.IsSampleDownloaded)
                    {
                        await OpenMagazine(item, false);
                    }
                    else
                    {
                        try
                        {
                            await purchaseModule.Init(item);
                        }
                        catch
                        {
                            purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    if (item.IsDownloaded)
                    {
                        await OpenMagazine(item, false);
                    }
                    else
                    {
                        DownloadMagazine(item, false);
                    }
                }
            }
            else if (item.Group == MagazineDataSource.GetGroup(loader.GetString("my_magazines")))
            {
                try
                {
                    await purchaseModule.Init(item, true);
                }
                catch
                {
                    purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }

            //if (!item.IsDownloaded && item.SecondButtonVisible == true) {

            //    //Utils.Utils.navigateTo(typeof(LibrelioApplication.PdfViewPage));
            //    if (item.IsSampleDownloaded)
            //    {
            //        await OpenMagazine(item, true);
            //    }
            //    else
            //    {
            //        DownloadMagazine(item, true);
            //    }

            //} else if (item.IsDownloaded) {

            //    if (app.Manager == null)
            //    {

            //        app.Manager = new MagazineManager("Magazines");
            //        await app.Manager.LoadLocalMagazineList();
            //    }

            //    item.IsOpening = true;
            //    isOpening = true;
            //    var mag = DownloadManager.GetLocalUrl(app.Manager.MagazineLocalUrl, item.FileName);
            //    if (mag == null) { isOpening = false; item.IsOpening = false; return; }
            //    var str = await DownloadManager.OpenPdfFile(mag);
            //    if (str == null) { isOpening = false; item.IsOpening = false; return; }
            //    item.IsOpening = false;
            //    isOpening = false;
            //    this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = str, folderUrl = mag.FolderPath });

            //} else if (item.DownloadOrReadButton == resourceLoader.GetString("download")) {

            //    if (item.IsPaid)
            //    {
            //        item.DownloadOrReadButton = resourceLoader.GetString("preparing");
            //        try
            //        {
            //            await purchaseModule.Init(item);
            //        }
            //        catch
            //        {
            //            purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //        }
            //        item.DownloadOrReadButton = resourceLoader.GetString("download");
            //    }
            //    else
            //    {
            //        var group = MagazineDataSource.GetGroup("All Magazines");
            //        group.Items.Remove(item);
            //        DownloadMagazine param = null;
            //        if (app.Manager.MagazineUrl.Count > 0)
            //        {
            //            var it = app.Manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

            //            if (it.Count() > 0)
            //                param = new DownloadMagazine()
            //                {
            //                    url = it.First()
            //                };
            //        }
            //        else if (app.Manager.MagazineLocalUrl.Count > 0)
            //        {
            //            var it = app.Manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

            //            if (it.Count() > 0)
            //                param = new DownloadMagazine()
            //                {
            //                    url = DownloadManager.ConvertFromLocalUrl(it.First())
            //                };
            //        }

            //        if (param != null)
            //        {
            //            app.needToDownload = true;
            //            this.Frame.Navigate(typeof(DownloadingPage), param);
            //        }
            //    }
            //}
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private async void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    if (isOpening) return;
        //    var button = sender as Button;
        //    var item = ((MagazineViewModel)button.DataContext);
        //    var resourceLoader = new ResourceLoader();
        //    if (item.DownloadOrReadButton == resourceLoader.GetString("read"))
        //    {
        //        await OpenMagazine(item, false);
        //    }
        //    else if (item.DownloadOrReadButton == resourceLoader.GetString("download"))
        //    {
        //        if (item.IsPaid)
        //        {
        //            item.DownloadOrReadButton = resourceLoader.GetString("preparing");
        //            try
        //            {
        //                await purchaseModule.Init(item);
        //            }
        //            catch
        //            {
        //                purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        //            }
        //            item.DownloadOrReadButton = resourceLoader.GetString("download");
        //        }
        //        else
        //        {
        //            DownloadMagazine(item, false);
        //        }
        //    }
        //}

        //private async void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    if (isOpening) return;
        //    var button = sender as Button;
        //    var item = ((MagazineViewModel)button.DataContext);
        //    var resourceLoader = new ResourceLoader();
        //    if (item.SampleOrDeleteButton == resourceLoader.GetString("delete"))
        //    {
        //        await DeleteMagazine(item);
        //    }
        //    else if (item.SampleOrDeleteButton == resourceLoader.GetString("sample"))
        //    {
        //        if (item.IsPaid)
        //        {
        //            if (item.IsSampleDownloaded)
        //            {
        //                await OpenMagazine(item, true);
        //            }
        //            else
        //            {
        //                DownloadMagazine(item, true);
        //            }
        //        }
        //    }
        //}

        private async Task OpenMagazine(MagazineViewModel item, bool sample)
        {
            var app = Application.Current as App;
            var resourceLoader = new ResourceLoader();

            isOpening = true;
            item.DownloadOrReadButton = resourceLoader.GetString("opening");
            item.IsOpening = true;

            if (app.Manager == null)
            {
                app.Manager = new MagazineManager("Magazines");
                await app.Manager.LoadLocalMagazineList();
            }

            var mag = DownloadManager.GetLocalUrl(app.Manager.MagazineLocalUrl, item.FileName);
            if (mag == null) { isOpening = false; return; }
            var str = await DownloadManager.OpenPdfFile(mag);
            if (str == null) { isOpening = false; return; }
            isOpening = false;
            item.DownloadOrReadButton = resourceLoader.GetString("read");
            item.IsOpening = false;
            this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = str, folderUrl = mag.FolderPath });
        }

        private void DownloadMagazine(MagazineViewModel item, bool sample)
        {
            var app = Application.Current as App;

            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            group.Items.Remove(item);
            DownloadMagazine param = null;
            if (app.Manager.MagazineUrl.Count > 0)
            {
                var it = app.Manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        url = it.First(),
                        IsSampleDownloaded = sample
                    };
            }
            else if (app.Manager.MagazineLocalUrl.Count > 0)
            {
                var it = app.Manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));

                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        url = DownloadManager.ConvertFromLocalUrl(it.First()),
                        IsSampleDownloaded = sample
                    };
            }

            if (param != null)
            {
                app.needToDownload = true;
                this.Frame.Navigate(typeof(DownloadingPage), param);
            }
        }

        private async Task DeleteMagazine(MagazineViewModel item)
        {
            var app = Application.Current as App;
            var resourceLoader = new ResourceLoader();

            item.SampleOrDeleteButton = resourceLoader.GetString("deleting");
            isOpening = true;

            if (app.Manager == null) {

                app.Manager = new MagazineManager("Magazines");
                await app.Manager.LoadLocalMagazineList();
            }

            var mag = DownloadManager.GetLocalUrl(app.Manager.MagazineLocalUrl, item.FileName);
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
            await app.Manager.AddUpdateMetadataEntry(mag);
            var title = item.FileName;
            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("my_magazines"));
            group.Items.Remove(item);
            if (app.snappedCollection != null)
            {
                try {
                    app.snappedCollection.Remove(item);
                } catch { }
            }
            if (group.Items.Count == 0)
            {
                group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
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
            group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            item = group.Items.Where((magazine) => magazine.FileName.Equals(title)).First();
            if (item != null)
            {
                item.IsDownloaded = false;
                item.IsSampleDownloaded = false;
            }

            isOpening = false;
        }

        void purchaseModule_GetSample(object sender, MagazineViewModel item)
        {
            if (item.IsPaid)
            {
                if (item.IsSampleDownloaded)
                {
                    var task = OpenMagazine(item, true);
                }
                else
                {
                    DownloadMagazine(item, true);
                }
            }
        }

        async void purchaseModule_Delete(object sender, MagazineViewModel item)
        {
            await DeleteMagazine(item);
            purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        void purchaseModule_Open(object sender, MagazineViewModel item)
        {
            if (item.IsDownloaded && !item.IsSampleDownloaded)
            {
                var task = OpenMagazine(item, false);
            }
            else if (item.IsSampleDownloaded)
            {
                var task = OpenMagazine(item, true);
            }
        }

        private async void purchaseModule_Bought(object sender, string url)
        {
            var purchase = sender as PurchaseModule;


            var item = purchase.GetCurrentItem();
            var resourceLoader = new ResourceLoader();
            var app = Application.Current as App;
            item.DownloadOrReadButton = resourceLoader.GetString("opening");


            if (app.Manager == null)
            {
                app.Manager = new MagazineManager("Magazines");
                await app.Manager.LoadLocalMagazineList();
            }

            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            group.Items.Remove(item);
            DownloadMagazine param = null;
            if (app.Manager.MagazineUrl.Count > 0)
            {
                var it = app.Manager.MagazineUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));


                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        url = it.First(),
                        redirectUrl = url
                    };
            }
            else if (app.Manager.MagazineLocalUrl.Count > 0)
            {
                var it = app.Manager.MagazineLocalUrl.Where((magUrl) => magUrl.FullName.Equals(item.FileName));


                if (it.Count() > 0)
                    param = new DownloadMagazine()
                    {
                        url = DownloadManager.ConvertFromLocalUrl(it.First()),
                        redirectUrl = url
                    };
            }
            item.DownloadOrReadButton = resourceLoader.GetString("read");


            if (param != null)
            {
                app.needToDownload = true;
                this.Frame.Navigate(typeof(DownloadingPage), param);
            }

        }

        private async void Grid_Loaded_1(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            //var app = Application.Current as App;
            await LoadUI();
            var value = ApplicationData.Current.LocalSettings.Values["last_checked"] as string;
            if (value == null)
            {
                var date = new DateTime(2012, 4, 15);
                ApplicationData.Current.LocalSettings.Values["last_checked"] = date.ToUniversalTime().ToString("R");
                value = ApplicationData.Current.LocalSettings.Values["last_checked"] as string;
            }
            var lastChecked = DateTimeOffset.Parse(value);
            var minutesPassed = (DateTime.UtcNow - lastChecked).TotalMinutes;
            if (minutesPassed > 30)
            {
                minutesPassed = 30;
                ApplicationData.Current.LocalSettings.Values["last_checked"] = DateTime.UtcNow.ToString("R");
            }

            var tsk = StartLoopRefresh((int)minutesPassed);

            //bool updatedView = false;

            //if (!app.loadedPList)
            //{
                //try
                //{
                    //if (app.Manager.MagazineLocalUrl.Count == 0)
                    //{
                        //await app.Manager.LoadLocalMagazineList();
                        //await LoadDefaultData();
                    //}
            
                    //if (app.Manager.MagazineLocalUrl.Count > 0)
                    //{
                        //var list = MagazineDataSource.LoadMagazines(app.Manager.MagazineLocalUrl);
                        //if (list.Count > 0)
                        //{
                        //    foreach (var item in list)
                        //    {
                        //        GridViewItem container = itemGridView.ItemContainerGenerator.ContainerFromItem(item) as GridViewItem;
                        //        VariableSizedWrapGrid.SetRowSpan(container, item.RowSpan);
                        //        VariableSizedWrapGrid.SetColumnSpan(container, item.ColSpan);
                        //        VariableSizedWrapGrid vswGrid = VisualTreeHelper.GetParent(container) as VariableSizedWrapGrid;
                        //        vswGrid.InvalidateMeasure();
                        //    }
                        //}
                        //UpdateCovers();

                        //await Task.Delay(50);
                        //var loader = new ResourceLoader();
                        //itemGridView.ScrollIntoView(MagazineDataSource.GetGroup(loader.GetString("all_magazines")), ScrollIntoViewAlignment.Leading);
                        //await Task.Delay(50);
                        //var scrollViewer = findFirstInVisualTree<ScrollViewer>(itemGridView);
                        //if (scrollViewer != null)
                        //{
                        //    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - 120);
                        //    updatedView = true;
                        //}
                        
                        //UpdateCovers();
                        //UpdateTile();
                        //await UpdateLocalCovers();
                    //}

                    //if (!app.loadedPList)
                    //{
                    //if (app.Manager.MagazineUrl.Count == 0)
                        //await UpdateUIOnline();

                        //app.loadedPList = true;
                    //}

                    

                    //await Task.Delay(100);
                    //itemGridView.ScrollIntoView(MagazineDataSource.GetGroup("All Magazines"), ScrollIntoViewAlignment.Leading);
                    //await Task.Delay(100);
                    //var scrollViewer1 = findFirstInVisualTree<ScrollViewer>(itemGridView);
                    //if (scrollViewer1 != null)
                    //{
                    //    scrollViewer1.ScrollToHorizontalOffset(scrollViewer1.HorizontalOffset - 120);
                    //}
                    //await LoadDefaultData();
                    //var task = DownloadCovers();
                    //var task2 = UpdateTile();
               // }
                //catch
                //{
                    //return;
                //}
                //app.loadedPList = true;
            //}
            //else
            //{
            //    try
            //    {
            //        if (app.Manager.MagazineUrl.Count == 0)
            //            await app.Manager.LoadPLISTAsync();

            //        if (app.Manager.MagazineLocalUrl.Count == 0)
            //            await app.Manager.LoadLocalMagazineList();

            //        var list = MagazineDataSource.LoadMagazines(app.Manager.MagazineLocalUrl);
            //        if (list.Count > 0)
            //        {
            //            foreach (var item in list)
            //            {
            //                GridViewItem container = itemGridView.ItemContainerGenerator.ContainerFromItem(item) as GridViewItem;
            //                VariableSizedWrapGrid.SetRowSpan(container, item.RowSpan);
            //                VariableSizedWrapGrid.SetColumnSpan(container, item.ColSpan);
            //                VariableSizedWrapGrid vswGrid = VisualTreeHelper.GetParent(container) as VariableSizedWrapGrid;
            //                vswGrid.InvalidateMeasure();
            //            }
            //        }

            //        UpdateCovers();
            //        await Task.Delay(50);
            //        itemGridView.ScrollIntoView(MagazineDataSource.GetGroup("All Magazines"), ScrollIntoViewAlignment.Leading);
            //        await Task.Delay(50);
            //        var scrollViewer1 = findFirstInVisualTree<ScrollViewer>(itemGridView);
            //        if (scrollViewer1 != null)
            //        {
            //            scrollViewer1.ScrollToHorizontalOffset(scrollViewer1.HorizontalOffset - 120);
            //        }
            //        await LoadDefaultData();
            //        var task = UpdateLocalCovers();
            //        UpdateTile();
            //    }
            //    catch
            //    {
            //        return;
            //    }
            //}
            var app = Application.Current as App;
            var loader1 = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader1.GetString("my_magazines"));
            if (group.Items.Count == 0)
            {
                app.NoMagazines = true;
                var loader = new ResourceLoader();
                group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
                if (group.Items.Count != 0)
                {
                    app.snappedCollection = new ObservableCollection<MagazineViewModel>();
                    app.snappedCollection.Add(group.Items.First());
                    itemListView.ItemsSource = app.snappedCollection;
                }
            }
            else
            {
                app.NoMagazines = false;
                snappedView.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                noMagazineSnapped.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                titleSnapped.Visibility = Windows.UI.Xaml.Visibility.Visible;
                snappedGridView.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (group.Items.Count != 0)
                {
                    app.snappedCollection = new ObservableCollection<MagazineViewModel>();
                    for (int i = 0; i < group.Items.Count; i++)
                    {
                        app.snappedCollection.Add(group.Items[i]);
                    }
                    snappedGridView.ItemsSource = app.snappedCollection;
                }
            }

            //UpdateCovers();

            //if (loadedPList)
            //{
            //    var task = DownloadCovers();
            //}
            //else
            //{
            //    var task1 = UpdateLocalCovers();
            //}

            //var task2 = UpdateTile();
        }

        private async Task StartLoopRefresh(int minutesPassed)
        {
            await Task.Delay((30 - minutesPassed) * 60 * 1000, cts.Token);
            if (cts.Token.IsCancellationRequested) return;
            await UpdateUIOnline();

            while (true)
            {
                await Task.Delay(30 * 60 * 1000, cts.Token);
                if (cts.Token.IsCancellationRequested) return;
                await UpdateUIOnline();
            }
        }

        private async Task LoadUI()
        {
            var app = Application.Current as App;
            if (app.Manager.MagazineLocalUrl.Count == 0)
            {
                await app.Manager.LoadLocalMagazineList();

                if (app.Manager.MagazineLocalUrl.Count > 0)
                {
                    await LoadUIInternal();
                }
            }
            else
            {
                await LoadUIInternal();
            }
        }

        private async Task LoadUIInternal()
        {
            try
            {
                var app = Application.Current as App;
                var list = MagazineDataSource.LoadMagazines(app.Manager.MagazineLocalUrl);
                if (list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        await UpdateCover(item);

                        GridViewItem container = itemGridView.ItemContainerGenerator.ContainerFromItem(item) as GridViewItem;
                        VariableSizedWrapGrid.SetRowSpan(container, item.RowSpan);
                        VariableSizedWrapGrid.SetColumnSpan(container, item.ColSpan);
                        VariableSizedWrapGrid vswGrid = VisualTreeHelper.GetParent(container) as VariableSizedWrapGrid;
                        vswGrid.InvalidateMeasure();
                    }
                }

                await UpdateCovers();
                await Task.Delay(50);
                var loader = new ResourceLoader();
                itemGridView.ScrollIntoView(MagazineDataSource.GetGroup(loader.GetString("all_magazines")), ScrollIntoViewAlignment.Leading);
                await Task.Delay(50);
                var scrollViewer1 = findFirstInVisualTree<ScrollViewer>(itemGridView);
                if (scrollViewer1 != null)
                {
                    scrollViewer1.ScrollToHorizontalOffset(scrollViewer1.HorizontalOffset - 120);
                }
                var task = UpdateLocalCovers();
                var task1 = UpdateTile();
            }
            catch { }
        }

        private async Task UpdateUIOnline()
        {
            if (isRefreshing) return;

            isRefreshing = true;

            var app = Application.Current as App;
            var isUpdated = await app.Manager.LoadPLISTAsync();

            var list = MagazineDataSource.LoadMagazines(app.Manager.MagazineLocalUrl);
            if (list.Count > 0)
            {
                foreach (var item in list)
                {
                    GridViewItem container = itemGridView.ItemContainerGenerator.ContainerFromItem(item) as GridViewItem;
                    VariableSizedWrapGrid.SetRowSpan(container, item.RowSpan);
                    VariableSizedWrapGrid.SetColumnSpan(container, item.ColSpan);
                    VariableSizedWrapGrid vswGrid = VisualTreeHelper.GetParent(container) as VariableSizedWrapGrid;
                    vswGrid.InvalidateMeasure();
                }
            }

            await DownloadCovers(isUpdated);

            foreach (var item in list)
            {
                await UpdateCover(item);
            }

            isRefreshing = false;
        }

        private async Task DownloadCovers(bool isUpdated)
        {
            var folder = ApplicationData.Current.LocalFolder;
            folder = await folder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            if (folder == null) return;

            BackgroundDownloader downloader = new BackgroundDownloader();
            //ApplicationData.Current.LocalSettings.Values.Remove("last_update");
            var value = ApplicationData.Current.LocalSettings.Values["last_update"] as string;
            if (value == null)
            {
                var date = new DateTime(2012, 4, 15);
                ApplicationData.Current.LocalSettings.Values["last_update"] = date.ToUniversalTime().ToString("R");
                value = ApplicationData.Current.LocalSettings.Values["last_update"] as string;
            }
            downloader.SetRequestHeader("If-Modified-Since", value);

            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            if (group == null) return;
            if (group.Items.Count != 0)
            {
                var tasks = new Task[group.Items.Count + 1];
                int i = 0;
                bool error = false;
                foreach (var item in group.Items)
                {
                        DownloadOperation download = null;
                        StorageFile file = null;

                        if (item.PngFile.Contains("_newsstand.png"))
                        {
                            var url = item.PngFile.Replace("_newsstand.png", ".png");

                            file = await folder.CreateFileAsync(url, CreationCollisionOption.OpenIfExists);
                            download = downloader.CreateDownload(new Uri(item.PngUrl.Replace("_newsstand.png", ".png")), file);

                            var task1 = Task.Run(async () =>
                            {
                                error = await HandleDownloadAsync(download, true);

                                var tsk = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
                                {
                                    if (!error)
                                    {
                                        var t = UpdateTile();
                                    }
                                });
                            });
                            tasks[i++] = task1;
                        }

                        file = await folder.CreateFileAsync(item.PngFile, CreationCollisionOption.OpenIfExists);
                        download = downloader.CreateDownload(new Uri(item.PngUrl), file);

                        var task = Task.Run(async () =>
                        {
                            error = await HandleDownloadAsync(download, true);

                            var tsk = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                            {
                                if (!error)
                                {
                                    await UpdateCover(item);
                                }
                            });
                        });
                        tasks[i++] = task;
                }
                await Task.WhenAll(tasks);
                if (!error && isUpdated)
                    ApplicationData.Current.LocalSettings.Values["last_update"] = DateTime.UtcNow.ToString("R");
            }
        }

        private async Task LoadDefaultData()
        {
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

        private async Task UpdateCovers()
        {
            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            if (group == null) return;
            if (group.Items.Count != 0)
            {
                for (int i = 0; i < group.Items.Count; i++)
                {
                    await UpdateCover(group.Items[i]);
                    //group.Items[i].Image = new BitmapImage(new Uri(group.Items[i].Thumbnail));
                }
            }
        }

        private async Task UpdateCover(MagazineViewModel item)
        {
            bool tryNoNewstand = false;
            try
            {
                var bitmap = new BitmapImage();
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(item.Thumbnail));
                var stream = await file.OpenAsync(FileAccessMode.Read);

                await bitmap.SetSourceAsync(stream);
                item.Image = bitmap;

                stream.Dispose();
            }
            catch
            {
                tryNoNewstand = true;
            }

            if (tryNoNewstand && item.Thumbnail.Contains("_newsstand.png"))
            {
                var url = item.Thumbnail.Replace("_newsstand.png", ".png");

                try
                {
                    var bitmap = new BitmapImage();
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
                    var stream = await file.OpenAsync(FileAccessMode.Read);

                    await bitmap.SetSourceAsync(stream);
                    item.Image = bitmap;

                    stream.Dispose();
                }
                catch { }
            }
        }

        private async Task UpdateLocalCovers()
        {
            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("my_magazines"));
            if (group == null) return;
            if (group.Items.Count != 0)
            {
                foreach (var item in group.Items)
                {
                    try
                    {
                        var file = await StorageFile.GetFileFromPathAsync(item.PngPath);
                        var image = new BitmapImage();
                        await image.SetSourceAsync(await file.OpenReadAsync());
                        item.Image = image;
                    }
                    catch { }
                }
            }
        }

        private void ClearCovers()
        {
            var loader = new ResourceLoader();
            var group = MagazineDataSource.GetGroup(loader.GetString("my_magazines"));
            if (group == null) return;
            if (group.Items.Count != 0)
            {
                for (int i = 0; i < group.Items.Count; i++)
                {
                    group.Items[i].Image = null;
                }
            }

            group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
            if (group.Items.Count != 0)
            {
                for (int i = 0; i < group.Items.Count; i++)
                {
                    group.Items[i].Image = null;
                }
            }
        }

        private async Task UpdateTile()
        {
            try
            {
                var loader = new ResourceLoader();
                var group = MagazineDataSource.GetGroup(loader.GetString("all_magazines"));
                if (group.Items.Count == 0) return;

                var item = group.Items[0];
                var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideSmallImageAndText04);

                XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
                tileTextAttributes[0].InnerText = item.Title;
                tileTextAttributes[1].InnerText = item.Subtitle;

                XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");

                string url = "";
                if (item.PngFile.Contains("_newsstand.png"))
                {
                    url = item.Thumbnail.Replace("_newsstand.png", ".png");
                }
                await ScaleImageForTile(url);
                ((XmlElement)tileImageAttributes[0]).SetAttribute("src", "ms-appdata:///local/Covers/thumbnail.png");

                TileNotification tileNotification = new TileNotification(tileXml);
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            }
            catch { }
        }

        private async Task ScaleImageForTile(string url)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(url));
            var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);

            var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);

            var memStream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(memStream, decoder);

            // Scaling occurs before flip/rotation.
            encoder.BitmapTransform.ScaledWidth = 195;
            encoder.BitmapTransform.ScaledHeight = 250;

            try
            {
                await encoder.FlushAsync();
            }
            catch (Exception)
            {
            }

            stream.Dispose();
            stream = null;

            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            file = await folder.CreateFileAsync("thumbnail.png", CreationCollisionOption.ReplaceExisting);
            stream = await file.OpenAsync(Windows.Storage.FileAccessMode.ReadWrite);
            // Overwrite the contents of the file with the updated image stream.
            memStream.Seek(0);
            await RandomAccessStream.CopyAndCloseAsync(memStream.GetInputStreamAt(0), stream.GetOutputStreamAt(0));

            await stream.FlushAsync();
            stream.Dispose();
            memStream.Dispose();

            await Task.Delay(1000);
        }

        private async void Appbar_Click(object sender, RoutedEventArgs e)
        {
            if (isRefreshing) return;

            Button b = sender as Button;
            if (b != null) {
                string tag = (string)b.Tag;
                switch (tag) {
                    case "Refresh":
                        {
                            await UpdateUIOnline();
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

        private async Task<bool> HandleDownloadAsync(DownloadOperation download, bool start, IProgress<int> progress = null, CancellationToken cancelToken = default(CancellationToken))
        {
            var app = Application.Current as App;
            bool error = false;

            try
            {
                // Store the download so we can pause/resume. 
                app.activeDownloads.Add(download);

                if (start)
                {
                    // Start the download and attach a progress handler. 
                    await download.StartAsync().AsTask(cancelToken);
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler. 
                    await download.AttachAsync().AsTask(cancelToken);
                }
            }
            catch (TaskCanceledException)
            {
                error = true;
            }
            catch (Exception ex)
            {
                error = true;
            }
            finally
            {
                app.activeDownloads.Remove(download);
            }

            return error;
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

        private static T findFirstInVisualTree<T>(DependencyObject parent) where T : class
        {
            if (parent == null)
            {
                return null;
            }

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                var childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = findFirstInVisualTree<T>(child);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null)
                    {
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = child as T;
                    break;
                }
            }

            return foundChild;
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            bool unsnapped = ((ApplicationView.Value != ApplicationViewState.Snapped) || ApplicationView.TryUnsnap());
            if (!unsnapped)
            {
                return;
            }
        }

        private void root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (purchaseModule.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                purchaseModule.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                e.Handled = true;
            }
        }

    }
}
