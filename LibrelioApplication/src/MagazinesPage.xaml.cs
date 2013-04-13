﻿using LibrelioApplication.Data;
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
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Streams;

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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Debug.WriteLine("LoadState");
            //var dataSrc = new LibrelioApplication.Data.MagazineDataSource();
            //this.DefaultViewModel["Items"] = dataSrc;
            //// TODO: Create an appropriate data model for your problem domain to replace the sample data
            //var sampleDataGroups = MagazineDataSource.GetGroups((String)navigationParameter);
            manager = await MagazineDataSource.LoadMagazinesAsync();
            var sampleDataGroups = MagazineDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Groups"] = sampleDataGroups;
            Debug.WriteLine("LoadState - finished");
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        async void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //// Navigate to the appropriate destination page, configuring the new page
            //// by passing required information as a navigation parameter
            //var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            //this.Frame.Navigate(typeof(SplitPage), groupId);
            var item = ((MagazineViewModel)e.ClickedItem);
            //sParam += LibrelioApplication.PdfViewPage.qqq;
            if (!item.IsDownloaded) {

                Utils.Utils.navigateTo(typeof(LibrelioApplication.PdfViewPage));

            } else {

                if (manager == null) {

                    manager = new MagazineManager("Magazines");
                    await manager.LoadLocalMagazineList();
                }

                var mag = DownloadManager.GetLocalUrl(manager.MagazineLocalUrl, item.FileName);
                this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = await DownloadManager.OpenPdfFile(mag), folderUrl = mag.FolderPath });
            }
        }

        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content.Equals("Read"))
            {
                var item = ((MagazineViewModel)button.DataContext);

                if (manager == null)
                {
                    manager = new MagazineManager("Magazines");
                    await manager.LoadLocalMagazineList();
                }

                var mag = DownloadManager.GetLocalUrl(manager.MagazineLocalUrl, item.FileName);
                this.Frame.Navigate(typeof(PdfViewPage), new MagazineData() { stream = await DownloadManager.OpenPdfFile(mag), folderUrl = mag.FolderPath });
            }
            else
            {
                this.Frame.Navigate(typeof(DownloadingPage));
            }
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button.Content.Equals("Delete"))
            {
                var item = ((MagazineViewModel)button.DataContext);

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
                var group = MagazineDataSource.GetGroup("My Magazines");
                group.Items.Remove(item);
                if (group.Items.Count == 0)
                {
                    MagazineDataSource.RemoveGroup(group.UniqueId);
                }
            }
        }

        private void Appbar_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null) {
                string tag = (string)b.Tag;
                switch (tag) { 
                    case "Slideshow":
                        //msgBox if need:
                        Utils.Utils.navigateTo(typeof(LibrelioApplication.SlideShowPage), "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white");
                        break;
                    case "Video":
                        Utils.Utils.navigateTo(typeof(LibrelioApplication.VideoPage), "http://localhost/test_move1.mov?waplay=auto1");
                        break;
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


    }
}
