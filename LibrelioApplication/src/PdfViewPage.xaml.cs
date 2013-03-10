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
// Added by Dorin Damaschin
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using MuPDFWinRT;

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
    //public struct PageLink
    //{
    //    public Rect rect;
    //    public string url;
    //}

    //public struct PageData
    //{
    //    public string Url { get; set; }
    //    public int Idx { get; set; }
    //    public List<PageLink> Links { get; set; }
    //}

    // Changed by Dorin Damaschin
    //---------------------------------------------
    //TODO Move to some model file

    // Changed the structure to a bindable class so the UI will update when changes are 
    // made to the properties
    public class PageLink : LibrelioApplication.Common.BindableBase
    {
        private Rect _rect = new Rect(0, 0, 0, 0);
        private string _url = "";

        public Rect rect
        {
            get { return _rect; }
            set
            {
                _rect = value;
                OnPropertyChanged("rect");
            }
        }

        public string url
        {
            get { return _url; }
            set
            {
                _url = value;
                OnPropertyChanged("url");
            }
        }
    }

    // Changed the structure to a bindable class so the UI will update when changes are 
    // made to the properties
    public class PageData : LibrelioApplication.Common.BindableBase
    {
        //public string Url { get; set; }
        private ImageSource _image = null;
        private int _idx = 0;
        private ObservableCollection<PageLink> _links = new ObservableCollection<PageLink>();
        private double _width = 0;
        private double _height = 0;
        private bool _loading = true;

        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public int Idx
        {
            get { return _idx; }
            set
            {
                _idx = value;
                OnPropertyChanged("Idx");
            }
        }

        public ObservableCollection<PageLink> Links
        {
            get { return _links; }
            set
            {
                _links = value;
                OnPropertyChanged("Links");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public bool Loading
        {
            get { return _loading; }
            set
            {
                _loading = value;
                OnPropertyChanged("Loading");
            }
        }
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
        //List<PageData> pages = new List<PageData>();
        //List<PageData> thumbs = new List<PageData>();
        
        // Changed by Dorin Damaschin
        ObservableCollection<PageData> pages = new ObservableCollection<PageData>();
        ObservableCollection<PageData> thumbs = new ObservableCollection<PageData>();
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
            
            // Changed by Dorin Damaschin

            //pageNum = 1;
            //pageCount = 5;
            //for (int p = 1; p <= pageCount; p++) {
            //    PageData data = new PageData() { Url = getPageName(p), Idx = p };
            //    data.Links = new List<PageLink>();
            //    PageLink link = new PageLink();
            //    //link.rect = new Rect(1400, 2800, 350, 400);
            //    link.rect = new Rect(0, 0, 800, 1000);
            //    //link.url = "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white";
            //    link.url = "http://localhost/sample_5.jpg?waplay=auto&wadelay=3000&wabgcolor=white";
            //    data.Links.Add(link);
                
            //    pages.Add( data );
            //    thumbs.Add(new PageData() { Url = getThumbName(p), Idx = p });
            //}

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

        // Added by Dorin Damaschin
        // Load pdf pages once the container is loaded
        private async void pagesListView_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPages();
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
                Windows.Foundation.Point pos = p.Position;
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

        // Added by Dorin Damaschin
        //---------------------------------------------------------------

        // Load PDF pages async 
        private async Task LoadPages()
        {
            var buffer = await GetPDFFileData();
            if (buffer == null) return;

            // create the MuPDF Document on a background thread
            var createDocOp = Task.Run<Document>(() =>
            {
                return Document.Create(
                    buffer, // - file
                    DocumentType.PDF, // type
                    160 // - dpi
                  );
            });

            using (var document = await createDocOp)
            {
                pageNum = 0;
                pageCount = document.PageCount;
                for (int p = 0; p < pageCount; p++)
                {
                    MuPDFWinRT.Point size = document.GetPageSize(p);

                    // add the loading DataTemplate to the UI
                    PageData data = new PageData() { Image = null, Idx = p + 1, Width = size.X, Height = size.Y, Loading = true };
                    pages.Add(data);

                    // if we loaded more than 8 pages use a smaller size
                    if (p > 8)
                    {
                        size.Y = 100 * size.Y / size.X;
                        size.X = 100;
                    }

                    // load page to a bitmap buffer on a background thread
                    var image = new WriteableBitmap(size.X, size.Y);
                    IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                    buf.Length = image.PixelBuffer.Length;

                    await Task.Run(() =>
                    {
                        document.DrawPage(p, buf, 0, 0, size.X, size.Y, false);
                    });

                    // copy the buffer to the WriteableBitmap ( UI Thread )
                    buf.CopyTo(image.PixelBuffer);
                    image.Invalidate();

                    data.Loading = false;
                    data.Image = image;

                    //data.Links = new List<PageLink>();
                    PageLink link = new PageLink();
                    //link.rect = new Rect(1400, 2800, 350, 400);
                    link.rect = new Rect(0, 0, 800, 1000);
                    //link.url = "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white";
                    link.url = "http://localhost/sample_5.jpg?waplay=auto&wadelay=3000&wabgcolor=white";
                    data.Links.Add(link);

                    thumbs.Add(new PageData() { Image = image, Idx = p + 1 });
                }
            }
        }

        // open the testmagazine.pdf in the Assets\test folder and return a buffer with it's content
        private async Task<IBuffer> GetPDFFileData()
        {
            try
            {

                var fileHandle =
                    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\testmagazine.pdf");

                using (IRandomAccessStream randomAccessStream = await fileHandle.OpenReadAsync())

                using (IInputStream inputStreamAt = randomAccessStream.GetInputStreamAt(0))
                using (var dataReader = new DataReader(inputStreamAt))
                {
                    uint u = await dataReader.LoadAsync((uint)randomAccessStream.Size);
                    IBuffer readBuffer = dataReader.ReadBuffer(u);

                    return readBuffer;
                }

            }
            catch
            {

                return null;
            }
        }

    }
}
