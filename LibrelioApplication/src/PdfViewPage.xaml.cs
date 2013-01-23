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
    public sealed partial class PdfViewPage : LibrelioApplication.Common.LayoutAwarePage
    {
        string pdfFileName;
        string curPageFileName;
        int pageNum;

        public PdfViewPage()
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
            pdfFileName = (string)navigationParameter;
            pageNum = 1;
            curPageFileName = pdfFileName.Replace(".pdf","") + "_page" + pageNum + ".png";
            imgPage1.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", curPageFileName)));
            //imgPage1.Source = new Uri( String.Format("ms-appdata:///local/{0}", curPageFileName) );

            List<MagazineViewModel> lst = new List<MagazineViewModel>();
            for (int i = 0; i < 10; i++)
            {
                lst.Add(new MagazineViewModel("t1", "", "ms-appdata:///local/wind_355.png", "", ""));
                lst.Add(new MagazineViewModel("t2", "", "ms-appdata:///local/wind_356.png", "", ""));
                lst.Add(new MagazineViewModel("t3", "", "ms-appdata:///local/wind_357.png", "", ""));
            }

            itemGridView.ItemsSource = lst;
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
