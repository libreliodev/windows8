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
    /// A page that displays pdf (actually list of png files and description in json format).
    /// </summary>
    //public sealed partial class PdfViewPage : LibrelioApplication.Common.LayoutAwarePage
    public sealed partial class PdfViewPage : Page
    {
        //TODO Move to some model file
        struct PageLink {
            public Rect rect;
            public string url;
        }

        public struct PageData {
            public string Url { get; set; }
            public int Idx { get; set; }
        }

        static string LOCAL_HOST = "localhost";

        string pdfFileName;
        string curPageFileName;
        int pageNum;
        int pageCount;
        List<PageLink> pageLinks = new List<PageLink>();
        List<PageData> pages = new List<PageData>();
        List<PageData> thumbs = new List<PageData>();
        const float INIT_ZOOM_FACTOR = 0.67f;

        public PdfViewPage()
        {
            this.InitializeComponent();
            scrollViewer.ZoomToFactor( INIT_ZOOM_FACTOR );
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
                pages.Add(new PageData() { Url = getPageName(p), Idx = p });
                thumbs.Add(new PageData() { Url = getThumbName(p), Idx = p });
            }

            pagesListView.ItemsSource = pages;
            itemGridView.ItemsSource = thumbs;



            ////curPageFileName = pdfFileName.Replace(".pdf","") + "_page" + pageNum + ".png";
            ////imgPage1.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", curPageFileName)));
            ////imgPage2.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", curPageFileName)));
            ////imgPage3.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", curPageFileName)));
            ////imgPage1.Source = new Uri( String.Format("ms-appdata:///local/{0}", curPageFileName) );
            ////imgPage1.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri( getPageName(1) ));
            ////imgPage2.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(getPageName(2)));
            ////imgPage3.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(getPageName(3)));

            //List<MagazineViewModel> lst = new List<MagazineViewModel>();
            //for (int i = 0; i < 10; i++)
            //{
            //    lst.Add(new MagazineViewModel("t1", "", getThumbName(1), "", ""));
            //    lst.Add(new MagazineViewModel("t1", "", getThumbName(2), "", ""));
            //    lst.Add(new MagazineViewModel("t1", "", getThumbName(3), "", ""));
            //    //lst.Add(new MagazineViewModel("t2", "", "ms-appdata:///local/wind_356.png", "", ""));
            //    //lst.Add(new MagazineViewModel("t3", "", "ms-appdata:///local/wind_357.png", "", ""));
            //}

            //itemGridView.ItemsSource = lst;

            PageLink l = new PageLink();
            //l.rect = new Rect(1400, 2800, 350, 400);
            l.rect = new Rect(0, 0, 400, 400);
            //l.url = "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white";
            l.url = "http://localhost/sample_5.jpg?waplay=auto1&wadelay=3000&wabgcolor=white";
            pageLinks.Add(l);
            PointerReleased += PdfViewPage_PointerReleased;
        }

        string getPageName(int page) {
            //return pdfFileName.Replace(".pdf", "") + "_page" + page + ".png";
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + ".png"));
        }

        string getThumbName(int page)
        {
            //return pdfFileName.Replace(".pdf", "") + "_page" + page + ".png";
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + "_thumb" + ".png"));
        }



        void PdfViewPage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //TODOThink!
            //var p = e.GetCurrentPoint(imgPage1);
            
            //int tt = 2;
            //for (int i = 0; i < pageLinks.Count; i++) { 
            //    if( pageLinks[i].rect.Contains( p.Position ) ){
            //        Uri uri = new Uri(pageLinks[i].url);
            //        if (uri.Host == LOCAL_HOST) {
            //            if (uri.LocalPath.EndsWith(".png") || uri.LocalPath.EndsWith(".jpg")) {
            //                if (uri.Query.Contains("warect=full"))
            //                {
            //                    Utils.Utils.navigateTo(typeof(LibrelioApplication.SlideShowPage), pageLinks[i].url);
            //                }
            //                else {
            //                    embdedFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //                    embdedFrame.Navigate( typeof(LibrelioApplication.SlideShowPage) , pageLinks[i].url );
            //                }
            //            }
            //            if (uri.LocalPath.EndsWith(".mov")) { 
            //            }
            //        }
            //        //string url = pageLinks[i].url;

            //        //if (url.StartsWith(LOCAL_SIGN)) { 

            //        //}
            //    }
            //}
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
