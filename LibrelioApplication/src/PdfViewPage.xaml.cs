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
using Windows.UI.Xaml.Navigation;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace LibrelioApplication
{

    /// <summary>
    /// Custom Text box control derived from Textbox class.
    /// </summary>
    public class CustomTextBox : TextBox
    {
        public CustomTextBox()
        {
            this.Background = new SolidColorBrush(Windows.UI.Colors.Coral);
            this.BorderThickness = new Thickness(1);
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
        }
    }


    public class MyListView : ListView
    {
        public PointerEventHandler PointReleaseHandler { get; set; }
        protected override DependencyObject GetContainerForItemOverride()
        {
            MyListViewItem item = new MyListViewItem();
            item.PointerReleased += item_PointerReleased;
            return item;
        }

        void item_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (PointReleaseHandler != null)
                PointReleaseHandler(sender, e);
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            //base.OnPointerReleased(e);
            e.Handled = false; 
        }
    }

    public class MyListViewItem : ListViewItem
    {
        protected override void OnRightTapped(Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            base.OnRightTapped(e);
            e.Handled = false; // Stop 'swallowing' the event
        }
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            //base.OnPointerPressed(e);
            e.Handled = false; 
        }

        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            //base.OnPointerReleased(e);
            e.Handled = false;
        }


    }

    //TODO Move to some model file
    public struct PageLink
    {
        public Rect rect;
        public string url;
    }

    public struct PageData
    {
        public string Url { get; set; }
        public int Idx { get; set; }
        public List<PageLink> Links { get; set; }
    }

    /// <summary>
    /// A page that displays pdf (actually list of png files and description in json format).
    /// </summary>
    //public sealed partial class PdfViewPage : LibrelioApplication.Common.LayoutAwarePage
    public sealed partial class PdfViewPage : Page
    {

        static string LOCAL_HOST = "localhost";

        string pdfFileName;
        string curPageFileName;
        int pageNum;
        int pageCount;
        //List<PageLink> pageLinks = new List<PageLink>();
        List<PageData> pages = new List<PageData>();
        List<PageData> thumbs = new List<PageData>();
        const float INIT_ZOOM_FACTOR = 0.6f;
        const float PAGE_WIDTH = 950;

        public PdfViewPage()
        {
            this.InitializeComponent();
        }

        ///// <summary>
        ///// Populates the page with content passed during navigation.  Any saved state is also
        ///// provided when recreating a page from a prior session.
        ///// </summary>
        ///// <param name="navigationParameter">The parameter value passed to
        ///// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        ///// </param>
        ///// <param name="pageState">A dictionary of state preserved by this page during an earlier
        ///// session.  This will be null the first time a page is visited.</param>
        //protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Object navigationParameter = e.Parameter;
            pdfFileName = (string)navigationParameter;
            
            pageNum = 1;
            pageCount = 5;
            for (int p = 1; p <= pageCount; p++) {
                PageData data = new PageData() { Url = getPageName(p), Idx = p };
                data.Links = new List<PageLink>();
                PageLink link = new PageLink();
                //link.rect = new Rect(1400, 2800, 350, 400);
                link.rect = new Rect(0, 0, 800, 1000);
                //link.url = "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white";
                link.url = "http://localhost/sample_5.jpg?waplay=auto&wadelay=3000&wabgcolor=white";
                data.Links.Add(link);
                
                pages.Add( data );
                thumbs.Add(new PageData() { Url = getThumbName(p), Idx = p });
            }

            ////TODODEBUG
            pagesListView.PointReleaseHandler = PdfViewPage_PointerReleased;
            pagesListView.ItemsSource = pages;
            itemGridView.ItemsSource = thumbs;

            //qqq.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/test/img/sample_1.jpg"));
            scrollViewer.ZoomToFactor(INIT_ZOOM_FACTOR);
            embdedFrame.Navigating += embdedFrame_Navigating;
            embdedFrame.Navigated += embdedFrame_Navigated;

            
            ////TODODEBUG
            //pagesListView.IsSwipeEnabled = false;
            //pagesListView.IsTapEnabled = false;
            //pagesListView.IsItemClickEnabled = false;

        }

        void embdedFrame_Navigated(object sender, NavigationEventArgs e)
        {
        }

        void embdedFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
        }

        string getPageName(int page) {
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + ".png"));
        }

        string getThumbName(int page)
        {
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + "_thumb" + ".png"));
        }



        void PdfViewPage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (sender is MyListViewItem) {
                var p = e.GetCurrentPoint((UIElement)sender);
                PageData data = (PageData)(((MyListViewItem)sender).Content);
                Point pos = p.Position;
                for (int i = 0; i < data.Links.Count; i++)
                {
                    if (data.Links[i].rect.Contains(p.Position))
                    {
                        Uri uri = new Uri(data.Links[i].url);
                        if (uri.Host == LOCAL_HOST)
                        {
                            if (uri.LocalPath.EndsWith(".png") || uri.LocalPath.EndsWith(".jpg"))
                            {
                                if (uri.Query.Contains("warect=full"))
                                {
                                    Utils.Utils.navigateTo(typeof(LibrelioApplication.SlideShowPage), data.Links[i].url);
                                }
                                else
                                {
                                    //TODO continue with real links - right now coord and size worked incorrectly
                                    embdedFrame.Width = data.Links[i].rect.Width * scrollViewer.ZoomFactor;
                                    embdedFrame.Height = data.Links[i].rect.Height * scrollViewer.ZoomFactor;
                                    embdedFrame.Margin = new Thickness(30+data.Links[i].rect.Left * scrollViewer.ZoomFactor, data.Links[i].rect.Top * scrollViewer.ZoomFactor, 0, 0);
                                    embdedFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                    embdedFrame.Navigate(typeof(LibrelioApplication.SlideShowPage), data.Links[i].url);
                                }
                            }
                            if (uri.LocalPath.EndsWith(".mov"))
                            {
                            }
                        }
                    }
                }

            }
        }


        private void itemGridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            int idx = ((PageData)e.ClickedItem).Idx-1;
            scrollViewer.ScrollToHorizontalOffset(idx * PAGE_WIDTH * scrollViewer.ZoomFactor);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int i1 = 2;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            int i2 = 2;
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
        



    }
}
