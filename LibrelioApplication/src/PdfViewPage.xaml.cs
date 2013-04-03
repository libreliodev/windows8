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
using System.Threading;
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

    public enum UIType
    {
        SlideShow,
        VideoPlayer,
        PageButton
    };

    public struct UIAddon
    {
        public UIElement element { get; set; }
        public UIType type { get; set; }
        public string url { get; set; }
    }

    // Changed the structure to a bindable class so the UI will update when changes are 
    // made to the properties
    public class PageData : LibrelioApplication.Common.BindableBase
    {
        //public string Url { get; set; }
        private ImageSource _image = null;
        private ImageSource _imageRight = null;
        private ImageSource _imageSource = null;
        private int _idx = 0;
        private ObservableCollection<PageLink> _links = null;
        private ObservableCollection<PageLink> _linksLeft = null;
        private ObservableCollection<PageLink> _linksRight = null;
        private double _width = 0;
        private double _widthRight = 0;
        private double _pageWidth = 0;
        private double _height = 0;
        private double _pageHeight = 0;
        private bool _loading = true;
        private IList<UIAddon> _pageAddons = new List<UIAddon>();
        private bool _onePage = false;

        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public ImageSource ImageRight
        {
            get { return _imageRight; }
            set
            {
                _imageRight = value;
                OnPropertyChanged("ImageRight");
            }
        }

        public bool OnePage
        {
            get { return _onePage; }
            set
            {
                _onePage = value;
                OnPropertyChanged("OnePage");
            }
        }

        public bool NotOnePage
        {
            get { return !_onePage; }
        }

        public ImageSource ZoomedImage
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                OnPropertyChanged("ZoomedImage");
            }
        }

        public IList<UIAddon> Addons
        {
            get
            {
                return _pageAddons;
            }
            set
            {
                _pageAddons = value;
            }
        }

        public string PageNumber
        {
            get { return _idx.ToString(); }
        }

        public string PageNumberLeft
        {
            get { return (2 * _idx - 2).ToString(); }
        }

        public string PageNumberRight
        {
            get { return (2 * _idx - 1).ToString(); }
        }

        public string PageNumberOne
        {
            get 
            {
                if (_idx == 1) return "1";

                return (2 * (_idx - 1)).ToString(); 
            }
        }

        public float ZoomFactor { get; set; }

        public float FirstPageZoomFactor { get; set; }
        public float SecondPageZoomFactor { get; set; }

        public int Idx
        {
            get { return _idx; }
            set
            {
                _idx = value;
                OnPropertyChanged("Idx");
                OnPropertyChanged("PageNumber");
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

        public ObservableCollection<PageLink> LinksLeft
        {
            get { return _linksLeft; }
            set
            {
                _linksLeft = value;
                OnPropertyChanged("LinksLeft");
            }
        }

        public ObservableCollection<PageLink> LinksRight
        {
            get { return _linksRight; }
            set
            {
                _linksRight = value;
                OnPropertyChanged("LinksRight");
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

        public double WidthRight
        {
            get { return _widthRight; }
            set
            {
                _widthRight = value;
                OnPropertyChanged("WidthRight");
            }
        }

        public double PageWidth
        {
            get { return _pageWidth; }
            set
            {
                _pageWidth = value;
                OnPropertyChanged("PageWidth");
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

        public double PageHeight
        {
            get { return _pageHeight; }
            set
            {
                _pageHeight = value;
                OnPropertyChanged("PageHeight");
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

    struct LinkInfo
    {
        public LinkInfoVisitor visitor { get; set; }
        public int index { get; set; }
        public int count { get; set; }
        public int handled { get; set; }
    }

    /// <summary>
    /// A page that displays pdf (actually list of png files and description in json format).
    /// </summary>
    //public sealed partial class PdfViewPage : LibrelioApplication.Common.LayoutAwarePage
    public sealed partial class PdfViewPage : Page
    {

        static string LOCAL_HOST = "localhost";

        MagazineData pdfStream = null;

        string pdfFileName;
        string curPageFileName;
        int pageNum;
        Collection<int> pagesInView = new Collection<int>();
        int pageCount;

        //List<PageLink> pageLinks = new List<PageLink>();
        //List<PageData> pages = new List<PageData>();
        //List<PageData> thumbs = new List<PageData>();
        
        ObservableCollection<PageData> pages = new ObservableCollection<PageData>();
        ObservableCollection<PageData> thumbs = new ObservableCollection<PageData>();

        MuPDFWinRT.Document thumbsDoc = null;
        MuPDFWinRT.Document document = null;

        //const float INIT_ZOOM_FACTOR = 0.6f;
        //const float PAGE_WIDTH = 950;
        const int NUM_NEIGHBOURS_REDRAW = 0;
        const int NUM_NEIGHBOURS_BUFFER = 2;
        // we don't buffer unless we jumped BUFFER_OFFSET pages
        const int BUFFER_OFFSET = 1;

        ScrollViewer scrollViewer = null;
        float defaultZoomFactor = 1.0f;
        float currentZoomFactor = 1.0f;
        float offsetZF = 1.0f;

        bool needRedraw = false;
        bool needBuffer = false;
        bool isBusyRedraw = false;
        bool isBuffering = false;

        CancellationTokenSource cancelBuffer = null;
        CancellationTokenSource cancelRedraw = null;

        IList<LinkInfo> visitorList = new List<LinkInfo>();

        Windows.Foundation.Point touchPoint;
        bool controlPressed = false;
        UIElement currentElement = null;

        int pageBuffer = 0;

        private bool loadedFirstPage = false;

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
            pdfStream = navigationParameter as MagazineData;
            
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
            //pagesListView.PointReleaseHandler = PdfViewPage_PointerReleased;

            //qqq.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri("ms-appx:///Assets/test/img/sample_1.jpg"));
            //scrollViewer.ZoomToFactor(INIT_ZOOM_FACTOR);
            //embdedFrame.Navigating += embdedFrame_Navigating;
            //embdedFrame.Navigated += embdedFrame_Navigated;

            
            ////TODODEBUG
            //pagesListView.IsSwipeEnabled = false;
            //pagesListView.IsTapEnabled = false;
            //pagesListView.IsItemClickEnabled = false;

        }

        // Load pdf pages once the container is loaded
        private async void pagesListView_Loaded(object sender, RoutedEventArgs e)
        {
            var buffer = await GetPDFFileData();
            if (buffer == null) return;

            pdfStream.stream = null;

            // create the MuPDF Document on a background thread
            document = await CreateDocumentAsync(buffer);

            pageCount = document.PageCount / 2 + 1;

            await LoadPagesAsync();

            thumbsDoc = await CreateThumbsDocumentAsync(buffer);

            SetScrollViewer();

            pagesListView.ItemsSource = pages;
            startRing.IsActive = false;

            var thumbsOp = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                await LoadThumbsAsync();
            });
        }

        void embdedFrame_Navigated(object sender, NavigationEventArgs e)
        {
        }

        void embdedFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
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

        string getPageName(int page) {
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + ".png"));
        }

        string getThumbName(int page)
        {
            return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + "_thumb" + ".png"));
        }

        float CalculateZoomFactor(int height)
        {
            var rect = Window.Current.Bounds;
            return (float)(rect.Height-6) / height;
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
                                    //embdedFrame.Width = data.Links[i].rect.Width * scrollViewer.ZoomFactor;
                                    //embdedFrame.Height = data.Links[i].rect.Height * scrollViewer.ZoomFactor;
                                    //embdedFrame.Margin = new Thickness(30+data.Links[i].rect.Left * scrollViewer.ZoomFactor, data.Links[i].rect.Top * scrollViewer.ZoomFactor, 0, 0);
                                    //embdedFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
                                    //embdedFrame.Navigate(typeof(LibrelioApplication.SlideShowPage), data.Links[i].url);
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
            pagesListView.ScrollIntoView(pages[idx]);
            //scrollViewer.ScrollToHorizontalOffset(idx * PAGE_WIDTH * scrollViewer.ZoomFactor);
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

        //---------------------------------------------------------------

        // open the testmagazine.pdf in the Assets\test folder and return a buffer with it's content
        private async Task<IBuffer> GetPDFFileData()
        {
            try
            {

                if (pdfStream == null)
                {
                    pdfStream = new MagazineData();
                    var fileHandle =
                        await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\testmagazine.pdf");

                    pdfStream.folderUrl = "C:\\Users\\Dorin\\Documents\\Magazines\\wind_355\\";
                    pdfStream.stream = await fileHandle.OpenReadAsync();
                }

                using (IInputStream inputStreamAt = pdfStream.stream.GetInputStreamAt(0))
                using (var dataReader = new DataReader(inputStreamAt))
                {
                    uint u = await dataReader.LoadAsync((uint)pdfStream.stream.Size);
                    IBuffer readBuffer = dataReader.ReadBuffer(u);

                    return readBuffer;
                }

            }
            catch
            {

                return null;
            }
        }


        private async Task<Document> CreateDocumentAsync(IBuffer buffer)
        {
            return await Task.Run<Document>(() =>
            {
                return Document.Create(
                    buffer, // - file
                    DocumentType.PDF, // type
                    72 // - dpi
                  );
            });
        }

        private async Task<Document> CreateThumbsDocumentAsync(IBuffer buffer)
        {
            return await Task.Run<Document>(() =>
            {
                return Document.Create(
                    buffer, // - file
                    DocumentType.PDF, // type
                    72 / 4 // - dpi
                  );
            });
        }

        // Load the thumbs async
        private async Task LoadThumbsAsync()
        {
            if (thumbsDoc == null) return;

            itemGridView.ItemsSource = thumbs;

            thumbs.Add(new PageData() { Image = null, Idx = 1, OnePage = true, ZoomFactor = 1.0f });

            for (int p = 1; p < pageCount - 1; p++)
            {
                thumbs.Add(new PageData() { Image = null, Idx = p + 1, WidthRight = 75, ZoomFactor = 1.0f });
            }

            thumbs.Add(new PageData() { Image = null, Idx = pageCount, OnePage = true, ZoomFactor = 1.0f });

            MuPDFWinRT.Point size = thumbsDoc.GetPageSize(0);
            var width = 75;
            var height = size.Y * 75 / size.X;

            var image = await DrawToBufferAsync(thumbsDoc, 0, width, height);
            thumbs[0].Image = image;

            for (int p = 1; p < pageCount - 1; p++)
            {
                // load page to a bitmap buffer on a background thread
                image = await DrawToBufferAsync(thumbsDoc, 2 * p - 1, width, height);

                thumbs[p].Image = image;

                // load page to a bitmap buffer on a background thread
                image = await DrawToBufferAsync(thumbsDoc, 2 * p, width, height);

                thumbs[p].ImageRight = image;
            }

            image = await DrawToBufferAsync(thumbsDoc, pageCount - 1, width, height);
            thumbs[pageCount - 1].Image = image;

            thumbsDoc = null;
        }

        // Load initial pages async
        private async Task LoadPagesAsync()
        {
            int width = 0;
            int height = 0;
            MuPDFWinRT.Point size = document.GetPageSize(0);
            // calculate display zoom factor
            defaultZoomFactor = CalculateZoomFactor(size.Y);
            currentZoomFactor = defaultZoomFactor;

            if (defaultZoomFactor > 1)
            {
                // if the screen is bigger the the document size we adjust with offsetZF
                offsetZF = defaultZoomFactor;
                defaultZoomFactor = currentZoomFactor = 1.0f;
            }

            width = (int)(size.X * currentZoomFactor * offsetZF);
            height = (int)(size.Y * currentZoomFactor * offsetZF);

            // add the loading DataTemplate to the UI       
            PageData data = new PageData()
            {
                Image = null,
                Width = Window.Current.Bounds.Width,
                Height = Window.Current.Bounds.Height,
                Idx = 1,
                ZoomFactor = defaultZoomFactor,
                FirstPageZoomFactor = defaultZoomFactor,
                SecondPageZoomFactor = defaultZoomFactor,
                PageWidth = width,
                PageHeight = height,
                Loading = false
            };

            pages.Add(data);

            for (int p = 1; p < pageCount - 1; p++)
            {
                // add the loading DataTemplate to the UI       
                data = new PageData()
                {
                    Image = null,
                    Width = Window.Current.Bounds.Width,
                    Height = Window.Current.Bounds.Height,
                    Idx = p + 1,
                    ZoomFactor = defaultZoomFactor,
                    FirstPageZoomFactor = defaultZoomFactor,
                    SecondPageZoomFactor = defaultZoomFactor,
                    PageWidth = 2 * width,
                    PageHeight = height,
                    Loading = false
                };
                pages.Add(data);
            }

            // add the loading DataTemplate to the UI       
            data = new PageData()
            {
                Image = null,
                Width = Window.Current.Bounds.Width,
                Height = Window.Current.Bounds.Height,
                Idx = pageCount,
                ZoomFactor = defaultZoomFactor,
                FirstPageZoomFactor = defaultZoomFactor,
                SecondPageZoomFactor = defaultZoomFactor,
                PageWidth = width,
                PageHeight = height,
                Loading = false
            };
            pages.Add(data);
            data = new PageData()
            {
                Image = null,
                Width = Window.Current.Bounds.Width,
                Height = Window.Current.Bounds.Height,
                Idx = pageCount + 1,
                ZoomFactor = defaultZoomFactor,
                FirstPageZoomFactor = defaultZoomFactor,
                SecondPageZoomFactor = defaultZoomFactor,
                PageWidth = width,
                PageHeight = height,
                Loading = false
            };
            pages.Add(data);

            // load page to a bitmap buffer on a background thread
            var image = await DrawToBufferAsync(document, 0, width, height);

            pages[0].Image = image;
            pages[0].PageWidth = width;
            pages[0].ZoomFactor = currentZoomFactor;

            for (int p = 1; p < 3; p++)
            {
                width = size.X;
                height = size.Y;

                pages[p].PageWidth = 2 * width;
                pages[p].ZoomFactor = currentZoomFactor;

                // load page to a bitmap buffer on a background thread
                 await DrawTwoPagesToBufferAsync(document, p, width, height);
            }

            pageNum = CalcPageNum();
            pageBuffer = pageNum;
        }

        // Set the pagesListView ScrollViewer proprieties
        private void SetScrollViewer()
        {
            scrollViewer = findFirstInVisualTree<ScrollViewer>(pagesListView);
            if (scrollViewer != null)
            {
                scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
                scrollViewer.HorizontalSnapPointsAlignment = SnapPointsAlignment.Near;
                scrollViewer.ViewChanged += scrollviewer_ViewChanged;
            }
        }

        private void SetScrollViewer(ScrollViewer scr)
        {
            if (currentZoomFactor < 1)
            {
                scrollViewer.ZoomToFactor(currentZoomFactor);
            }

            if (!loadedFirstPage)
            {
                loadedFirstPage = true;
                var task = InitPageLink(0);
            }
        }

        // Determine the page in the center screen
        private int CalcPageNum()
        {
            if (scrollViewer == null) return 0;

            var x = scrollViewer.HorizontalOffset + (scrollViewer.ViewportWidth / 2);

            var pageWidth = scrollViewer.ExtentWidth / (pageCount + 1);

            return (int)(x / pageWidth);
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task<WriteableBitmap> DrawToBufferAsync(Document doc, int pageNumber, int width, int height)
        {
            var image = new WriteableBitmap(width, height);
            IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf.Length = image.PixelBuffer.Length;

            await Task.Run(() =>
            {
                doc.DrawPage(pageNumber, buf, 0, 0, width, height, false);
            });

            // copy the buffer to the WriteableBitmap ( UI Thread )
            var stream = buf.AsStream();
            await stream.CopyToAsync(image.PixelBuffer.AsStream());
            image.Invalidate();

            return image;
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task DrawTwoPagesToBufferAsync(Document doc, int page, int width, int height, CancellationToken token = default(CancellationToken), bool zoomed = false)
        {
            var image = new WriteableBitmap(width, height);
            IBuffer buf1 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf1.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            IBuffer buf2 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf2.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            image = new WriteableBitmap(2 * width, height);

            token.ThrowIfCancellationRequested();

            using (var stream = new InMemoryRandomAccessStream())
            {
                bool drawFirstPage = false;

                if (pages[page].FirstPageZoomFactor - currentZoomFactor < -0.1 ||
                    pages[page].FirstPageZoomFactor - currentZoomFactor > 0.1 || !zoomed)
                {
                    using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        await Task.Run(() =>
                        {
                            token.ThrowIfCancellationRequested();

                            doc.DrawPage(2 * page - 1, buf1, 0, 0, width, height, false);

                            token.ThrowIfCancellationRequested();

                            var size = width * 4;
                            for (int x = 0; x < height; x++)
                            {
                                dataWriter.WriteBuffer(buf1, (uint)(x * size), (uint)size);
                                dataWriter.WriteBytes(new byte[size]);

                                token.ThrowIfCancellationRequested();
                            }
                        });

                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();

                        drawFirstPage = true;
                    }

                    token.ThrowIfCancellationRequested();

                    await stream.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                    image.Invalidate();

                    if (zoomed)
                    {
                        pages[page].ZoomedImage = image;
                    }
                    else
                    {
                        pages[page].Image = image;
                    }

                    pages[page].FirstPageZoomFactor = currentZoomFactor;
                    pages[page].SecondPageZoomFactor = defaultZoomFactor;

                    token.ThrowIfCancellationRequested();
                }

                if (pages[page].SecondPageZoomFactor - currentZoomFactor < -0.1 ||
                    pages[page].SecondPageZoomFactor - currentZoomFactor > 0.1 || !zoomed)
                {
                    stream.Seek(0);
                    using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        int m = 1;
                        if (!drawFirstPage)
                        {
                            var bmp = pages[page].Image as WriteableBitmap;
                            token.ThrowIfCancellationRequested();
                            buf1 = new Windows.Storage.Streams.Buffer(2 * image.PixelBuffer.Capacity);
                            buf1.Length = 2 * image.PixelBuffer.Length;
                            await bmp.PixelBuffer.AsStream().CopyToAsync(buf1.AsStream());

                            m = 2;

                            token.ThrowIfCancellationRequested();
                        }

                        await Task.Run(() =>
                        {
                            token.ThrowIfCancellationRequested();

                            doc.DrawPage(2 * page, buf2, 0, 0, width, height, false);

                            token.ThrowIfCancellationRequested();

                            var size = width * 4;
                            for (int x = 0; x < height; x++)
                            {
                                dataWriter.WriteBuffer(buf1, (uint)(m * x * size), (uint)size);
                                dataWriter.WriteBuffer(buf2, (uint)(x * size), (uint)size);

                                token.ThrowIfCancellationRequested();
                            }
                        });

                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();

                        token.ThrowIfCancellationRequested();

                        await stream.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                        image.Invalidate();

                        pages[page].SecondPageZoomFactor = currentZoomFactor;
                    }
                }

                if (drawFirstPage)
                {
                    pages[page].ZoomFactor = currentZoomFactor;
                }
            }
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task DrawTwoPagesToBufferReversedAsync(Document doc, int page, int width, int height, CancellationToken token = default(CancellationToken), bool zoomed = false)
        {
            var image = new WriteableBitmap(width, height);
            IBuffer buf1 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf1.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            IBuffer buf2 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf2.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            image = new WriteableBitmap(2 * width, height);

            token.ThrowIfCancellationRequested();

            using (var stream = new InMemoryRandomAccessStream())
            {
                bool drawFirstPage = false;

                if (pages[page].SecondPageZoomFactor - currentZoomFactor < -0.1 ||
                    pages[page].SecondPageZoomFactor - currentZoomFactor > 0.1 || !zoomed)
                {
                    using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        await Task.Run(() =>
                        {
                            token.ThrowIfCancellationRequested();

                            doc.DrawPage(2 * page, buf2, 0, 0, width, height, false);

                            token.ThrowIfCancellationRequested();

                            var size = width * 4;
                            for (int x = 0; x < height; x++)
                            {
                                dataWriter.WriteBytes(new byte[size]);
                                dataWriter.WriteBuffer(buf2, (uint)(x * size), (uint)size);

                                token.ThrowIfCancellationRequested();
                            }
                        });

                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();

                        token.ThrowIfCancellationRequested();

                        await stream.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                        image.Invalidate();

                        pages[page].FirstPageZoomFactor = defaultZoomFactor;
                        pages[page].SecondPageZoomFactor = currentZoomFactor;

                        if (zoomed)
                        {
                            pages[page].ZoomedImage = image;
                        }
                        else
                        {
                            pages[page].Image = image;
                        }

                        drawFirstPage = true;
                    }
                }

                if (pages[page].FirstPageZoomFactor - currentZoomFactor < -0.1 ||
                    pages[page].FirstPageZoomFactor - currentZoomFactor > 0.1)
                {
                    stream.Seek(0);
                    using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        int m = 1;
                        if (!drawFirstPage)
                        {
                            var bmp = pages[page].Image as WriteableBitmap;
                            token.ThrowIfCancellationRequested();
                            buf2 = new Windows.Storage.Streams.Buffer(bmp.PixelBuffer.Capacity);
                            buf2.Length = bmp.PixelBuffer.Length;
                            await bmp.PixelBuffer.AsStream().CopyToAsync(buf2.AsStream());

                            m = 2;

                            token.ThrowIfCancellationRequested();
                        }

                        await Task.Run(() =>
                        {
                            token.ThrowIfCancellationRequested();

                            doc.DrawPage(2 * page - 1, buf1, 0, 0, width, height, false);

                            token.ThrowIfCancellationRequested();

                            var size = width * 4;
                            for (int x = 0; x < height; x++)
                            {
                                dataWriter.WriteBuffer(buf1, (uint)(x * size), (uint)size);
                                dataWriter.WriteBuffer(buf2, (uint)(m * x * size), (uint)size);

                                token.ThrowIfCancellationRequested();
                            }
                        });

                        await dataWriter.StoreAsync();
                        await dataWriter.FlushAsync();
                    }

                    token.ThrowIfCancellationRequested();

                    await stream.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                    image.Invalidate();

                    pages[page].SecondPageZoomFactor = currentZoomFactor;
                }

                if (drawFirstPage)
                {
                    pages[page].ZoomFactor = currentZoomFactor;
                }
            }
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task DrawTwoPagesForBufferAsync(Document doc, int page, int width, int height, CancellationToken token = default(CancellationToken), bool zoomed = false)
        {
            var image = new WriteableBitmap(width, height);
            IBuffer buf1 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf1.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            IBuffer buf2 = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
            buf2.Length = image.PixelBuffer.Length;

            token.ThrowIfCancellationRequested();

            image = new WriteableBitmap(2 * width, height);

            token.ThrowIfCancellationRequested();

            using (var stream = new InMemoryRandomAccessStream())
            using (var dataWriter = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();

                    doc.DrawPage(2 * page - 1, buf1, 0, 0, width, height, false);

                    token.ThrowIfCancellationRequested();

                    doc.DrawPage(2 * page, buf2, 0, 0, width, height, false);

                    token.ThrowIfCancellationRequested();

                    var size = width * 4;
                    for (int x = 0; x < height; x++)
                    {
                        dataWriter.WriteBuffer(buf1, (uint)(x * size), (uint)size);
                        dataWriter.WriteBuffer(buf2, (uint)(x * size), (uint)size);

                        token.ThrowIfCancellationRequested();
                    }
                });

                await dataWriter.StoreAsync();
                await dataWriter.FlushAsync();

                token.ThrowIfCancellationRequested();

                await stream.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                image.Invalidate();

                pages[page].Image = image;
            }
        }

        //===============================================================================

        private bool NeedToUpdatePages(int page, int numNeighbours)
        {
            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < pageCount ? page + numNeighbours : pageCount - 1;
            for (int p = start; p <= end; p++)
            {
                if (pages[p].ZoomFactor - currentZoomFactor < -0.1 ||
                    pages[p].ZoomFactor - currentZoomFactor > 0.1)
                    return true;
            }

            return false;
        }

        // Updates the pdf pages async
        // --------------------------------------------------------
        private async Task UpdatePages(int page, int numNeighbours, ScrollViewer scr, bool reversed = false)
        {
            // if we are already updating another page wait to finish then try again
            if (isBuffering || isBusyRedraw) return;

            if (currentZoomFactor - defaultZoomFactor > -0.12 &&
                currentZoomFactor - defaultZoomFactor < 0.12)
            {
                pages[pageNum].ZoomedImage = null;
                pages[pageNum].ZoomFactor = defaultZoomFactor;
                pages[pageNum].FirstPageZoomFactor = defaultZoomFactor;
                pages[pageNum].SecondPageZoomFactor = defaultZoomFactor;
                return;
            }

            isBusyRedraw = true;
            cancelRedraw = new CancellationTokenSource();

            Task redrawTask = UpdatePagesInternal(page, numNeighbours, reversed, cancelRedraw.Token);

            try
            {
                await redrawTask;
                isBusyRedraw = false;
                HandleRequests(page, NUM_NEIGHBOURS_BUFFER, numNeighbours, scr, reversed);

            }
            catch (OperationCanceledException e)
            {
                if (redrawTask.IsCanceled)
                {
                    isBusyRedraw = false;
                    HandleRequests(page, NUM_NEIGHBOURS_BUFFER, numNeighbours, scr, reversed);
                }
                else
                {
                    isBusyRedraw = false;
                }
            }
        }

        private async Task UpdatePagesInternal(int page, int numNeighbours, bool reversed, CancellationToken token = default(CancellationToken))
        {
            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < pageCount)
                    await RedrawPage(p, page, reversed, token);
                token.ThrowIfCancellationRequested();

                if (p != page && (2 * page - p) >= 0)
                    await RedrawPage(2 * page - p, page, reversed, token);
                token.ThrowIfCancellationRequested();
            }
        }

        private async Task RedrawPage(int p, int page, bool reversed, CancellationToken token = default(CancellationToken))
        {
            //if (pages[p].Image == null)
            //    pages[p].Loading = true;

            // we resize the image to the current zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * currentZoomFactor * offsetZF * 1.05);
            int height = (int)(size.Y * currentZoomFactor * offsetZF * 1.05);

            token.ThrowIfCancellationRequested();

            // load page to a bitmap buffer on a background thread

            if (p > 0 && p < pageCount - 1)
            {
                if (!reversed)
                {
                    await DrawTwoPagesToBufferAsync(document, p, width, height, token, true);
                }
                else
                {
                    await DrawTwoPagesToBufferReversedAsync(document, p, width, height, token, true);
                }
            }
            else if (p == 0)
            {
                var image = await DrawToBufferAsync(document, 0, width, height);
                pages[p].Image = image;
            }
            else if (p == pageCount - 1)
            {
                var image = await DrawToBufferAsync(document, 2 * p - 1, width, height);
                pages[p].Image = image;
            }

            //if (pages[p].Loading)
            //    pages[p].Loading = false;
        }


        // Buffers pdf pages in advance async
        // --------------------------------------------------------
        private bool NeedToBufferPages(int page, int numNeighbours)
        {
            if (pages[page].Image == null)
                return true;

            var abs = pageNum > pageBuffer ? pageNum - pageBuffer : pageBuffer - pageNum;
            if (pageNum == pageBuffer || abs < BUFFER_OFFSET) return false;

            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < pageCount ? page + numNeighbours : pageCount - 1;
            for (int p = start; p <= end; p++)
            {
                if (p > 0)
                {
                    if (pages[p].Image == null)
                        return true;
                }

                if (pages[p].Image == null)
                    return true;
            }

            return false;
        }

        private async Task BufferPages(int page, int numNeighbours)
        {
            if (isBuffering || isBusyRedraw) return;

            isBuffering = true;
            cancelBuffer = new CancellationTokenSource();

            Task bufferTask = BufferPagesInternal(page, numNeighbours, cancelBuffer.Token);

            try
            {
                await bufferTask;
                isBuffering = false;
                HandleRequests(page, numNeighbours, 0);

            }
            catch (OperationCanceledException e)
            {
                isBuffering = false;
                HandleRequests(page, numNeighbours, 0);
            }
        }

        private async Task BufferPagesInternal(int page, int numNeighbours, CancellationToken token = default(CancellationToken))
        {
            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < pageCount ? page + numNeighbours : pageCount;

            // we clear the images that are no longer in the buffered zone
            for (int p = 0; p < start; p++)
            {
                if (pages[p].Image != null)
                {
                    pages[p].Image = null;
                    pages[p].ZoomFactor = defaultZoomFactor;
                }
            }

            for (int p = end + 1; p < pageCount - 3; p++)
            {
                if (pages[p].Image != null)
                {
                    pages[p].Image = null;
                    pages[p].ZoomFactor = defaultZoomFactor;
                }
            }

            token.ThrowIfCancellationRequested();

            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < pageCount)
                    await BufferPage(p);
                token.ThrowIfCancellationRequested();

                pageBuffer = page;

                if (p != page && (2 * page - p) >= 0)
                    await BufferPage(2 * page - p);
                token.ThrowIfCancellationRequested();

                pageBuffer = page;
            }
        }

        private async Task BufferPage(int p, CancellationToken token = default(CancellationToken))
        {
            if (p > pageNum + NUM_NEIGHBOURS_BUFFER ||
                p < pageNum - NUM_NEIGHBOURS_BUFFER)
                return;

            if (pages[p].Image != null) return;

            //pages[p].Loading = true;

            // we draw the image at the default zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * currentZoomFactor * offsetZF * 1.05);
            int height = (int)(size.Y * currentZoomFactor * offsetZF * 1.05);
            token.ThrowIfCancellationRequested();

            // load page to a bitmap buffer on a background thread

            if (p > 0 && p < pageCount - 1)
            {
                if (p != pageNum)
                {
                    await DrawTwoPagesForBufferAsync(document, p, width, height, token);
                }
                else
                {
                    await DrawTwoPagesToBufferAsync(document, p, width, height, token);
                }
                //pages[p].Loading = false;
                //pages[p].Image = image;
            }
            else if (p == 0)
            {
                var image = await DrawToBufferAsync(document, 0, width, height);
                //pages[p].Loading = false;
                pages[p].Image = image;
            }
            else if (p == pageCount - 1)
            {
                var image = await DrawToBufferAsync(document, 2 * p - 1, width, height);
                pages[p].Image = image;
            }
        }


        // Handle zooming and scrolling
        void scrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var p = CalcPageNum();

                if (p != pageNum)
                {
                    RemovePageItems(pageNum);

                    pages[pageNum].ZoomedImage = null;
                    pages[pageNum].FirstPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].SecondPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].ZoomFactor = defaultZoomFactor;
                    var item = pagesListView.ItemContainerGenerator.ContainerFromIndex(pageNum) as GridViewItem;
                    var scr = findFirstInVisualTree<ScrollViewer>(item);
                    if (scr != null)
                    {
                        scr.ZoomToFactor(defaultZoomFactor);
                    }

                    pageNum = p;

                    var task1 = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                        {
                            await InitPageLink(pageNum);
                        });

                    if (isBusyRedraw)
                    {
                        cancelRedraw.Cancel();
                        document.CancelDraw();
                        return;
                    }
                    else if (isBuffering)
                    {
                        if (NeedToBufferPages(pageNum, NUM_NEIGHBOURS_BUFFER))
                        {
                            needBuffer = true;
                        }
                        return;
                    }
                    else if (NeedToBufferPages(pageNum, NUM_NEIGHBOURS_BUFFER))
                    {
                        var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
                        {
                            await BufferPages(pageNum, NUM_NEIGHBOURS_BUFFER);
                        });
                    }

                }
            }
        }

        private void ScrollViewer_ViewChanged_1(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate)
            {
                var scrView = sender as ScrollViewer;
                if (scrView.ZoomFactor == currentZoomFactor) return;

                currentZoomFactor = scrView.ZoomFactor;

                if (NeedToUpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW))
                {
                    if (isBusyRedraw)
                    {
                        cancelRedraw.Cancel();
                        document.CancelDraw();
                        needRedraw = true;
                        return;
                    }
                    else if (isBuffering)
                    {
                        cancelBuffer.Cancel();
                        document.CancelDraw();
                        needBuffer = true;
                        needRedraw = true;
                        return;
                    }

                    var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async() =>
                    {
                        if (scrView.HorizontalOffset <= scrView.VerticalOffset)
                        {
                            await UpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW, scrView);
                        }
                        else
                        {
                            await UpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW, scrView, true);
                        }
                     });
                }
            }
        }

        private void ScrollViewer_Loaded_1(object sender, RoutedEventArgs e)
        {
            SetScrollViewer(sender as ScrollViewer);
        }

        private void HandleRequests(int page, int bNumNeigh, int rNumNeigh, ScrollViewer scr = null, bool reversed = false)
        {
            if (needRedraw && !isBuffering && !isBusyRedraw)
            {
                needRedraw = false;
                Task task = UpdatePages(page, rNumNeigh, scr, reversed);
            }
            else if (needBuffer && !isBuffering && !isBusyRedraw)
            {
                needBuffer = false;
                Task task = BufferPages(page, bNumNeigh);
            }
        }

        private async Task InitPageLink(int page)
        {
            if (page == 0)
            {
                if (pages[0].Links == null)
                {
                    var links = document.GetLinks(0);

                    var linkVistor = new LinkInfoVisitor();
                    linkVistor.OnURILink += visitor_OnURILink;
                    visitorList.Add(new LinkInfo() { visitor = linkVistor, index = 0, count = links.Count, handled = 0 });

                    for (int j = 0; j < links.Count; j++)
                    {
                        links[j].AcceptVisitor(linkVistor);
                    }
                }

                await LoadLinks(0);

                return;
            }

            if (page == pageCount - 1)
            {
                if (pages[pageCount - 1].Links == null)
                {
                    var links = document.GetLinks(0);

                    var linkVistor = new LinkInfoVisitor();
                    linkVistor.OnURILink += visitor_OnURILink;
                    visitorList.Add(new LinkInfo() { visitor = linkVistor, index = 2 * (pageCount - 1) - 1, count = links.Count, handled = 0 });

                    for (int j = 0; j < links.Count; j++)
                    {
                        links[j].AcceptVisitor(linkVistor);
                    }
                }

                await LoadLinks(pageCount - 1);

                return;
            }

            if (pages[page].LinksLeft == null)
            {
                var links = document.GetLinks(2 * page - 1);
                LinkInfoVisitor vis = new LinkInfoVisitor();
                visitorList.Add(new LinkInfo() { visitor = vis, index = 2 * page - 1, count = links.Count, handled = 0 });
                vis.OnURILink += visitor_OnURILink;
                foreach (var link in links)
                {
                    link.AcceptVisitor(vis);
                }
            }

            if (pages[page].LinksRight == null)
            {
                var links = document.GetLinks(2 * page);
                LinkInfoVisitor vis = new LinkInfoVisitor();
                visitorList.Add(new LinkInfo() { visitor = vis, index = 2 * page, count = links.Count, handled = 0 });
                vis.OnURILink += visitor_OnURILink;
                foreach (var link in links)
                {
                    link.AcceptVisitor(vis);
                }
            }

            await LoadLinks(page);
        }

        private void visitor_OnURILink(LinkInfoVisitor __param0, LinkInfoURI __param1)
        {
            foreach (var visitor in visitorList)
            {
                if (visitor.visitor == __param0)
                {
                    int index = 0;

                    PageLink link = new PageLink();
                    link.rect = new Rect(__param1.Rect.Left, __param1.Rect.Top, __param1.Rect.Right - __param1.Rect.Left, __param1.Rect.Bottom - __param1.Rect.Top);
                    link.url = __param1.URI;

                    if (visitor.index == 0)
                    {
                        if (pages[index].Links == null)
                        {
                            pages[index].Links = new ObservableCollection<PageLink>();
                        }
                        pages[0].Links.Add(link);
                    }
                    else if (visitor.index % 2 == 0)
                    {
                        index = (int)(visitor.index / 2);
                        if (pages[index].LinksRight == null)
                        {
                            pages[index].LinksRight = new ObservableCollection<PageLink>();
                        }
                        pages[index].LinksRight.Add(link);
                    }
                    else if (visitor.index % 2 == 1)
                    {
                        index = (int)(visitor.index / 2) + 1;
                        if (pages[index].LinksLeft == null)
                        {
                            pages[index].LinksLeft = new ObservableCollection<PageLink>();
                        }
                        pages[index].LinksLeft.Add(link);
                    }

                    var vis = visitorList[visitorList.IndexOf(visitor)];
                    //vis.handled++;
                    //if (vis.handled == vis.count)
                    //{
                    //    LoadLinks(index);
                    //}
                }
            }
        }

        private async Task LoadLinks(int page)
        {
            visitorList.Clear();
            if (pages[page].Links != null)
            {
                foreach (var item in pages[page].Links)
                {
                    bool alreadyInserted = false;
                    foreach (var addon in pages[page].Addons)
                    {
                        if (addon.url == item.url)
                        {
                            alreadyInserted = true;
                            break;
                        }
                    }

                    if (alreadyInserted) continue;

                    var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
                    var scr = findFirstInVisualTree<ScrollViewer>(root);
                    var children = VisualTreeHelper.GetChild(scr, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    var grid = children as Grid;

                    var rect = new Rect(item.rect.Left, item.rect.Top, item.rect.Width, item.rect.Height);
                    if (DownloadManager.IsFullScreenButton(item.url))
                    {
                        var button = new PageButton();
                        button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        grid.Children.Add(button);
                        button.Clicked += button_Clicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url });
                    }
                    else if (DownloadManager.IsImage(item.url))
                    {
                        var slideShow = new SlideShow();
                        await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        grid.Children.Add(slideShow);
                        pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url });
                    }
                    else if (DownloadManager.IsVideo(item.url))
                    {
                        var videoPlayer = new VideoPlayer();
                        grid.Children.Add(videoPlayer);
                        await videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url });
                    }
                }
            }

            if (pages[page].LinksLeft != null)
            {
                foreach (var item in pages[page].LinksLeft)
                {
                    bool alreadyInserted = false;
                    foreach (var addon in pages[page].Addons)
                    {
                        if (addon.url == item.url)
                        {
                            alreadyInserted = true;
                            break;
                        }
                    }

                    if (alreadyInserted) continue;

                    var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
                    var scr = findFirstInVisualTree<ScrollViewer>(root);
                    var children = VisualTreeHelper.GetChild(scr, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    var grid = children as Grid;

                    var rect = new Rect(item.rect.Left, item.rect.Top, item.rect.Width, item.rect.Height);
                    if (DownloadManager.IsFullScreenButton(item.url))
                    {
                        var button = new PageButton();
                        button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        grid.Children.Add(button);
                        button.Clicked += button_Clicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url });
                    }
                    else if (DownloadManager.IsImage(item.url))
                    {
                        var slideShow = new SlideShow();
                        await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        grid.Children.Add(slideShow);
                        pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url });
                    }
                    else if (DownloadManager.IsVideo(item.url))
                    {
                        var videoPlayer = new VideoPlayer();
                        grid.Children.Add(videoPlayer);
                        await videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url });
                    }
                }
            }

            if (pages[page].LinksRight != null)
            {
                foreach (var item in pages[page].LinksRight)
                {
                    bool alreadyInserted = false;
                    foreach (var addon in pages[page].Addons)
                    {
                        if (addon.url == item.url)
                        {
                            alreadyInserted = true;
                            break;
                        }
                    }

                    if (alreadyInserted) continue;

                    var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
                    var scr = findFirstInVisualTree<ScrollViewer>(root);
                    var children = VisualTreeHelper.GetChild(scr, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    children = VisualTreeHelper.GetChild(children, 0);
                    var grid = children as Grid;

                    var rect = new Rect(item.rect.Left + (pages[page].PageWidth / 2 / (offsetZF * defaultZoomFactor)), item.rect.Top, item.rect.Width, item.rect.Height);
                    if (DownloadManager.IsFullScreenButton(item.url))
                    {
                        var button = new PageButton();
                        button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        grid.Children.Add(button);
                        button.Clicked += button_Clicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url });
                    }
                    else if (DownloadManager.IsImage(item.url))
                    {
                        var slideShow = new SlideShow();
                        await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        grid.Children.Add(slideShow);
                        pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url });
                    }
                    else if (DownloadManager.IsVideo(item.url))
                    {
                        var videoPlayer = new VideoPlayer();
                        grid.Children.Add(videoPlayer);
                        await videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url });
                    }
                }
            }
        }

        void RemovePageItems(int page)
        {
            var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
            var scr = findFirstInVisualTree<ScrollViewer>(root);
            var children = VisualTreeHelper.GetChild(scr, 0);
            children = VisualTreeHelper.GetChild(children, 0);
            children = VisualTreeHelper.GetChild(children, 0);
            children = VisualTreeHelper.GetChild(children, 0);
            var grid = children as Grid;
            foreach (var addon in pages[page].Addons)
            {
                grid.Children.Remove(addon.element);
            }
            pages[page].Addons.Clear();
        }

        void button_Clicked(string folderUrl, string url)
        {
            fullScreenContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            var task = fullScreenContainer.Load(folderUrl, url);
        }

        private void ScrollViewer_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            //foreach (var addon in pages[pageNum].Addons)
            //{
            //    var scr = sender as ScrollViewer;
            //    var transform = addon.element.TransformToVisual(scr);
            //    var absoluteBounds = transform.TransformBounds(new Rect());
            //    var element = addon.element as WindMagazine.SlideShow;
            //    absoluteBounds.Width = element.Width * scr.ZoomFactor;
            //    absoluteBounds.Height = element.Height * scr.ZoomFactor;
            //    var point = e.GetCurrentPoint(null);
            //    if (absoluteBounds.Contains(point.Position))
            //    {
            //        //scr.ManipulationMode = ManipulationModes.None;
            //        //scrollViewer.ManipulationMode = ManipulationModes.None;
            //        currentElement = element;
            //        currentElement.ManipulationMode = ManipulationModes.All;
            //        touchPoint = point.Position;
            //        controlPressed = true;
            //    }
            //}
        }

        private void ScrollViewer_PointerMoved_1(object sender, PointerRoutedEventArgs e)
        {
            //if (controlPressed)
            //{
            //    var point = e.GetCurrentPoint(null);

            //    if (point.Position.Y - touchPoint.Y > -60 &&
            //        point.Position.Y - touchPoint.Y < 60)
            //    {
            //        controlPressed = false;
            //        var scr = sender as ScrollViewer;
            //        currentElement.ManipulationMode = ManipulationModes.System;
            //        scr.ManipulationMode = ManipulationModes.System;
            //        scrollViewer.ManipulationMode = ManipulationModes.System;
            //    }
            //}
        }

        private void ScrollViewer_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            //if (controlPressed)
            //{
            //    var scr = sender as ScrollViewer;
            //    currentElement.ManipulationMode = ManipulationModes.System;
            //    scr.ManipulationMode = ManipulationModes.System;
            //    scrollViewer.ManipulationMode = ManipulationModes.System;
            //    controlPressed = false;
            //}
        }

        private void pageRoot_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (pages.Count > 0)
            {
                if (pages[0].Width != Window.Current.Bounds.Width ||
                    pages[0].Height != Window.Current.Bounds.Height)
                {
                    int width = 0;
                    int height = 0;
                    MuPDFWinRT.Point size = document.GetPageSize(0);
                    // calculate display zoom factor
                    defaultZoomFactor = CalculateZoomFactor(size.Y);
                    currentZoomFactor = defaultZoomFactor;

                    if (defaultZoomFactor > 1)
                    {
                        // if the screen is bigger the the document size we adjust with offsetZF
                        offsetZF = defaultZoomFactor;
                        defaultZoomFactor = currentZoomFactor = 1.0f;
                    }
                    else
                    {
                        offsetZF = 1.0f;
                    }

                    width = (int)(size.X * currentZoomFactor * offsetZF);
                    height = (int)(size.Y * currentZoomFactor * offsetZF);

                    for (int p = 0; p <= pageCount; p++)
                    {
                        pages[p].Width = Window.Current.Bounds.Width;
                        pages[p].Height = Window.Current.Bounds.Height;
                        if (p == 0 || p == pageCount - 1 || p == pageCount)
                        {
                            pages[p].PageWidth = width;
                        }
                        else
                        {
                            pages[p].PageWidth = 2 * width;
                        }
                        pages[p].PageHeight = height;
                        pages[p].Image = null;
                        pages[p].ZoomedImage = null;
                        pages[p].FirstPageZoomFactor = defaultZoomFactor;
                        pages[p].SecondPageZoomFactor = defaultZoomFactor;
                        pages[p].ZoomFactor = defaultZoomFactor;
                        var item = pagesListView.ItemContainerGenerator.ContainerFromIndex(pageNum) as GridViewItem;
                        var scr = findFirstInVisualTree<ScrollViewer>(item);
                        if (scr != null)
                        {
                            scr.ZoomToFactor(defaultZoomFactor);
                        }
                    }

                    var task = BufferPages(pageNum, NUM_NEIGHBOURS_BUFFER);
                }
            }
        }

    }
}
