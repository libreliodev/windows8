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
using Windows.UI.Xaml.Media.Animation;
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
        private bool _internalLink = false;
        private int _pageNum = -1;

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

        public bool IsInternalLink { get { return _internalLink; } }

        public int PageNumber
        {
            get { return _pageNum; }
            set
            {
                _internalLink = true;
                _pageNum = value;
                OnPropertyChanged("PageNumber");
            }
        }
    }

    public enum UIType
    {
        SlideShow,
        VideoPlayer,
        PageButton
    };

    public enum ActivePage
    {
        Left,
        Right
    };

    public struct UIAddon
    {
        public UIElement element { get; set; }
        public UIType type { get; set; }
        public string url { get; set; }
        public int PageNum { get; set; }
        public ActivePage page { get; set; }
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
    public sealed partial class PdfViewPage : Common.LayoutAwarePage
    {

        static string LOCAL_HOST = "localhost";

        MagazineData pdfStream = null;

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
        const int NUM_NEIGHBOURS_BUFFER = 3;
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

        bool cancelDraw = false;

        IList<LinkInfo> visitorList = new List<LinkInfo>();

        
        Windows.Foundation.Point touchPoint;
        bool controlPressed = false;
        UIElement currentElement = null;

        int pageBuffer = 0;

        private bool loadedFirstPage = false;

        private bool switchOrientation = false;

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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            pdfStream = navigationParameter as MagazineData;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        //protected override void OnNavigatedTo(NavigationEventArgs e)
        //{
            //Object navigationParameter = e.Parameter;
            //pdfStream = navigationParameter as MagazineData;
            
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

        //}

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

            pagesListView.ItemsSource = pages;
            startRing.IsActive = false;

            SetScrollViewer();

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

        //string getPageName(int page) {
        //    return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + ".png"));
        //}

        //string getThumbName(int page)
        //{
        //    return String.Format("ms-appdata:///local/{0}", (pdfFileName.Replace(".pdf", "") + "_page" + page + "_thumb" + ".png"));
        //}

        float CalculateZoomFactor(int height)
        {
            var rect = Window.Current.Bounds;
            return (float)(rect.Height-6) / height;
        }

        float CalculateZoomFactor1(int width)
        {
            var rect = Window.Current.Bounds;
            return (float)(rect.Width) / width;
        }

        //void PdfViewPage_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    if (sender is MyListViewItem) {
        //        var p = e.GetCurrentPoint((UIElement)sender);
        //        PageData data = (PageData)(((MyListViewItem)sender).Content);
        //        Windows.Foundation.Point pos = p.Position;
        //        for (int i = 0; i < data.Links.Count; i++)
        //        {
        //            if (data.Links[i].rect.Contains(p.Position))
        //            {
        //                Uri uri = new Uri(data.Links[i].url);
        //                if (uri.Host == LOCAL_HOST)
        //                {
        //                    if (uri.LocalPath.EndsWith(".png") || uri.LocalPath.EndsWith(".jpg"))
        //                    {
        //                        if (uri.Query.Contains("warect=full"))
        //                        {
        //                            Utils.Utils.navigateTo(typeof(LibrelioApplication.SlideShowPage), data.Links[i].url);
        //                        }
        //                        else
        //                        {
        //                            //TODO continue with real links - right now coord and size worked incorrectly
        //                            //embdedFrame.Width = data.Links[i].rect.Width * scrollViewer.ZoomFactor;
        //                            //embdedFrame.Height = data.Links[i].rect.Height * scrollViewer.ZoomFactor;
        //                            //embdedFrame.Margin = new Thickness(30+data.Links[i].rect.Left * scrollViewer.ZoomFactor, data.Links[i].rect.Top * scrollViewer.ZoomFactor, 0, 0);
        //                            //embdedFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
        //                            //embdedFrame.Navigate(typeof(LibrelioApplication.SlideShowPage), data.Links[i].url);
        //                        }
        //                    }
        //                    if (uri.LocalPath.EndsWith(".mov"))
        //                    {
        //                    }
        //                }
        //            }
        //        }

        //    }
        //}


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
            if (ApplicationView.Value == ApplicationViewState.FullScreenPortrait)
            {
                if (idx > 0)
                {
                    idx = 2 * idx - 1;
                }
            }
            pagesListView.ScrollIntoView(pages[idx]);
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

            image = await DrawToBufferAsync(thumbsDoc, 2 * (pageCount - 1) - 1, width, height);
            thumbs[pageCount - 1].Image = image;

            thumbsDoc = null;
        }

        // Load initial pages async
        private async Task LoadPagesAsync()
        {
            int width = 0;
            int height = 0;
            MuPDFWinRT.Point size = document.GetPageSize(0);
            
            if (Window.Current.Bounds.Width > Window.Current.Bounds.Height)
            {
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
                //data = new PageData()
                //{
                //    Image = null,
                //    Width = Window.Current.Bounds.Width,
                //    Height = Window.Current.Bounds.Height,
                //    Idx = pageCount + 1,
                //    ZoomFactor = defaultZoomFactor,
                //    FirstPageZoomFactor = defaultZoomFactor,
                //    SecondPageZoomFactor = defaultZoomFactor,
                //    PageWidth = width,
                //    PageHeight = height,
                //    Loading = false
                //};
                //pages.Add(data);

                // load page to a bitmap buffer on a background thread
                var image = await DrawToBufferAsync(document, 0, width, height);

                pages[0].Image = image;
                pages[0].PageWidth = width;
                pages[0].ZoomFactor = currentZoomFactor;

                for (int p = 1; p < 3; p++)
                {
                    pages[p].PageWidth = 2 * width;

                    // load page to a bitmap buffer on a background thread
                    await DrawTwoPagesForBufferAsync(document, p, width, height);
                }
            }
            else
            {
                // calculate display zoom factor
                defaultZoomFactor = CalculateZoomFactor1(size.X);
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

                for (int p = 0; p <= 2 * (pageCount - 1) - 1; p++)
                {
                    // add the loading DataTemplate to the UI       
                    var data = new PageData()
                    {
                        Image = null,
                        Width = Window.Current.Bounds.Width,
                        Height = Window.Current.Bounds.Height,
                        Idx = p + 1,
                        ZoomFactor = defaultZoomFactor,
                        FirstPageZoomFactor = defaultZoomFactor,
                        SecondPageZoomFactor = defaultZoomFactor,
                        PageWidth = width,
                        PageHeight = height,
                        Loading = false
                    };
                    pages.Add(data);
                }

                // load page to a bitmap buffer on a background thread
                var image = await DrawToBufferAsync(document, 0, width, height);

                pages[0].Image = image;
                pages[0].PageWidth = width;
                pages[0].ZoomFactor = currentZoomFactor;

                for (int p = 1; p < 3; p++)
                {
                    pages[p].PageWidth = width;

                    image = await DrawToBufferAsync(document, p, width, height);
                    image.Invalidate();
                    pages[p].Image = image;
                }
            }

            pageNum = 0;
            //pageBuffer = pageNum;
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
                scrollViewer.AddHandler(PointerWheelChangedEvent, new PointerEventHandler(Bubble_PointerWheelChanged), true);
                Binding b = new Binding();
                b.Source = scrollViewer;
                b.Mode = BindingMode.TwoWay;
                Mediator.SetBinding(Common.ScrollViewerOffsetMediator.ScrollViewerProperty, b);
            }
        }

        private void Bubble_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (e.KeyModifiers == Windows.System.VirtualKeyModifiers.Control) return;
            if (pages[pageNum].ZoomFactor != defaultZoomFactor) return;
            var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(pageNum);
            var scr = findFirstInVisualTree<ScrollViewer>(root);
            if (scr != null)
            {
                if (scr.ZoomFactor != defaultZoomFactor) return;
            }

            //var width = scrollViewer.ExtentWidth / (pageCount + 1);
            //var width = pages[pageNum].Width;
            //var offset = (int)(scrollViewer.HorizontalOffset / width);
            var start = (int)(scrollViewer.HorizontalOffset + 0.5);
            var wheelDelta = e.GetCurrentPoint(null).Properties.MouseWheelDelta;

            if (start <= pageCount && wheelDelta < -100)
            {
                var ee = new ExponentialEase();
                ee.EasingMode = EasingMode.EaseInOut;
                var sb = new Storyboard();
                var da = new DoubleAnimation
                {
                    From = start,
                    To = start + 1,//(width * offset) + width,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5d)),
                    EasingFunction = ee,
                    EnableDependentAnimation = true
                };

                sb.Children.Add(da);
                Storyboard.SetTargetProperty(da, "HorizontalOffset");
                Storyboard.SetTarget(sb, Mediator);
                sb.Begin();
            }
            else if (start > 0 && wheelDelta > 100)
            {
                var ee = new ExponentialEase();
                ee.EasingMode = EasingMode.EaseInOut;
                var sb = new Storyboard();
                var da = new DoubleAnimation
                {
                    From = start,
                    To = start - 1,//(width * (offset - 1)),
                    Duration = new Duration(TimeSpan.FromSeconds(0.5d)),
                    EasingFunction = ee,
                    EnableDependentAnimation = true
                };

                sb.Children.Add(da);
                Storyboard.SetTargetProperty(da, "HorizontalOffset");
                Storyboard.SetTarget(sb, Mediator);
                sb.Begin();
            }
        }

        private void SetScrollViewer(ScrollViewer scr)
        {
            //if (currentZoomFactor < 1)
            //{
            //    scrollViewer.ZoomToFactor(currentZoomFactor);
            //}

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

            //int count = pageCount;
            //ApplicationViewState currentViewState = ApplicationView.Value;
            //if (currentViewState == ApplicationViewState.FullScreenPortrait)
            //{
            //    count = 2 * (pageCount - 1);
            //}

            //var x = scrollViewer.HorizontalOffset + (scrollViewer.ViewportWidth / 2);

            //var pageWidth = scrollViewer.ExtentWidth / (count + 1);

            return (int)(scrollViewer.HorizontalOffset + 0.5) - 2;//(x / pageWidth);
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
            //pages[page].ZoomedImage = null;

            var image = new WriteableBitmap(2 * width, height);
            var capacity = image.PixelBuffer.Capacity;
            var length = image.PixelBuffer.Length;
            image = null;
            var buffer = new Windows.Storage.Streams.Buffer(capacity);
            buffer.Length = length;
            bool f = false;

            if (cancelDraw) return;

            if (Math.Abs(pages[page].FirstPageZoomFactor - currentZoomFactor) > 0.1 || pages[page].Image == null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        doc.DrawFirtPageConcurrent(2 * page - 1, buffer, width, height);
                    }
                    catch { }
                });

                if (cancelDraw) return;
                image = new WriteableBitmap(2 * width, height);
                await buffer.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                image.Invalidate();

                pages[page].FirstPageZoomFactor = currentZoomFactor;
                pages[page].ZoomFactor = currentZoomFactor;
                if (pages[page].Image != null)
                    pages[page].ZoomedImage = image;
                else
                {
                    pages[page].Image = image;
                    f = true;
                }
            }

            if (Math.Abs(pages[page].SecondPageZoomFactor - currentZoomFactor) > 0.1 || f)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        doc.DrawSecondPageConcurrent(2 * page - 1, buffer, width, height);
                    }
                    catch { }
                });

                if (cancelDraw) return;
                image = new WriteableBitmap(2 * width, height);
                await buffer.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                image.Invalidate();

                pages[page].SecondPageZoomFactor = currentZoomFactor;
                if (pages[page].Image != null && !f)
                    pages[page].ZoomedImage = image;
                else
                    pages[page].Image = image;
            }

            pages[page].ZoomFactor = currentZoomFactor;
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task DrawTwoPagesToBufferReversedAsync(Document doc, int page, int width, int height, CancellationToken token = default(CancellationToken), bool zoomed = false)
        {
            //pages[page].ZoomedImage = null;

            var image = new WriteableBitmap(2 * width, height);
            var capacity = image.PixelBuffer.Capacity;
            var length = image.PixelBuffer.Length;
            image = null;
            var buffer = new Windows.Storage.Streams.Buffer(capacity);
            buffer.Length = length;
            bool f = false;

            if (token.IsCancellationRequested) return;

            if (Math.Abs(pages[page].SecondPageZoomFactor - currentZoomFactor) > 0.1 || pages[page].Image == null)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        doc.DrawSecondPageConcurrent(2 * page - 1, buffer, width, height);
                    }
                    catch { }
                });

                if (cancelDraw) return;
                image = new WriteableBitmap(2 * width, height);
                await buffer.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                image.Invalidate();

                pages[page].SecondPageZoomFactor = currentZoomFactor;
                pages[page].ZoomFactor = currentZoomFactor;
                if (pages[page].Image != null)
                    pages[page].ZoomedImage = image;
                else
                {
                    pages[page].Image = image;
                    f = true;
                }
            }

            if (Math.Abs(pages[page].FirstPageZoomFactor - currentZoomFactor) > 0.1 || f)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        doc.DrawFirtPageConcurrent(2 * page - 1, buffer, width, height);
                    }
                    catch { }
                });

                if (cancelDraw) return;
                image = new WriteableBitmap(2 * width, height);
                await buffer.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
                image.Invalidate();

                pages[page].FirstPageZoomFactor = currentZoomFactor;
                if (pages[page].Image != null && !f)
                    pages[page].ZoomedImage = image;
                else
                    pages[page].Image = image;
            }
        }

        // Draw the pdf page to a WritableBitmap (UI Thread)
        private async Task DrawTwoPagesForBufferAsync(Document doc, int page, int width, int height, CancellationToken token = default(CancellationToken), bool zoomed = false)
        {
            pages[page].Image = null;

            var image = new WriteableBitmap(2 * width, height);
            var capacity = image.PixelBuffer.Capacity;
            var length = image.PixelBuffer.Length;
            image = null;
            var buffer = new Windows.Storage.Streams.Buffer(capacity);
            buffer.Length = length;

            if (cancelDraw) return;

            await Task.Run(() =>
            {
                try
                {
                    doc.DrawTwoPagesConcurrent(2 * page - 1, buffer, width, height);
                }
                catch { }
            });

            if (cancelDraw) return;
            image = new WriteableBitmap(2 * width, height);
            await buffer.AsStream().CopyToAsync(image.PixelBuffer.AsStream());
            image.Invalidate();

            pages[page].Image = image;

            pages[page].ZoomFactor = pages[page].FirstPageZoomFactor = pages[page].SecondPageZoomFactor = defaultZoomFactor;
        }

        //===============================================================================

        private bool NeedToUpdatePages(int page, int numNeighbours)
        {
            int count = pageCount;
            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.FullScreenPortrait)
            {
                count = 2 * (pageCount - 1);
            }

            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < count ? page + numNeighbours : count - 1;
            for (int p = start; p <= end; p++)
            {
                if (Math.Abs(pages[p].ZoomFactor - currentZoomFactor) > 0.1 ||
                    Math.Abs(pages[p].FirstPageZoomFactor - currentZoomFactor) > 0.1 ||
                    Math.Abs(pages[p].SecondPageZoomFactor - currentZoomFactor) > 0.1)
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
                currentZoomFactor - defaultZoomFactor < 0.12 &&
                ApplicationView.Value != ApplicationViewState.FullScreenPortrait)
            {
                pages[page].ZoomedImage = null;
                pages[page].ZoomFactor = defaultZoomFactor;
                pages[page].FirstPageZoomFactor = defaultZoomFactor;
                pages[page].SecondPageZoomFactor = defaultZoomFactor;
                return;
            }

            isBusyRedraw = true;

            Task redrawTask = UpdatePagesInternal(page, numNeighbours, reversed);

            await redrawTask;
            isBusyRedraw = false;
            HandleRequests(page, NUM_NEIGHBOURS_BUFFER, numNeighbours, scr, reversed);
        }

        private async Task UpdatePagesInternal(int page, int numNeighbours, bool reversed, CancellationToken token = default(CancellationToken))
        {
            int count = pageCount;
            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.FullScreenPortrait)
            {
                count = 2 * (pageCount - 1);
            }
            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < count)
                {
                    try
                    {
                        await RedrawPage(p, page, reversed, token);
                    }
                    catch { }
                }

                if (cancelDraw) return;

                if (p != page && (2 * page - p) >= 0)
                {
                    try
                    {
                        await RedrawPage(2 * page - p, page, reversed, token);
                    }
                    catch { }
                }

                if (cancelDraw) return;
            }
        }

        private async Task RedrawPage(int p, int page, bool reversed, CancellationToken token = default(CancellationToken))
        {
            //if (pages[p].Image == null)
            //    pages[p].Loading = true;

            // we resize the image to the current zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * currentZoomFactor * offsetZF);
            int height = (int)(size.Y * currentZoomFactor * offsetZF);

            if (cancelDraw) return;

            // load page to a bitmap buffer on a background thread

            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState != ApplicationViewState.FullScreenPortrait)
            {
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
            }
            else
            {
                if (p >= 0 && p < 2 * (pageCount - 1))
                {
                    var image = await DrawToBufferAsync(document, p, width, height);
                    pages[p].ZoomFactor = currentZoomFactor;
                    pages[p].Image = image;
                }
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

            //var abs = pageNum > pageBuffer ? pageNum - pageBuffer : pageBuffer - pageNum;
            //if (pageNum == pageBuffer || abs < BUFFER_OFFSET) return false;

            int count = pageCount;
            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.FullScreenPortrait)
            {
                count = 2 * (pageCount - 1);
            }

            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < count ? page + numNeighbours : count - 1;
            for (int p = start; p <= end; p++)
            {
                if (pages[p].Image == null)
                    return true;
            }

            return false;
        }

        private async Task BufferPages(int page, int numNeighbours)
        {
            if (isBuffering || isBusyRedraw) return;

            isBuffering = true;

            Task bufferTask = BufferPagesInternal(page, numNeighbours);

             await bufferTask;
             isBuffering = false;
             HandleRequests(page, numNeighbours, 0);
        }

        private async Task BufferPagesInternal(int page, int numNeighbours, CancellationToken token = default(CancellationToken))
        {
            int count = pageCount;
            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.FullScreenPortrait)
            {
                count = 2 * (pageCount - 1);
            }

            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < count ? page + numNeighbours : count;

            // we clear the images that are no longer in the buffered zone
            for (int p = 0; p < start; p++)
            {
                if (pages[p].Image != null)
                {
                    pages[p].Image = null;
                    pages[p].ZoomFactor = defaultZoomFactor;
                }
            }

            for (int p = end + 1; p < count; p++)
            {
                if (pages[p].Image != null)
                {
                    pages[p].Image = null;
                    pages[p].ZoomFactor = defaultZoomFactor;
                }
            }

            if (cancelDraw) return;

            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < count)
                {
                    try
                    {
                        await BufferPage(p);
                    }
                    catch { }
                }

                if (cancelDraw) return;

                if (Math.Abs(page - pageNum) > 3)
                    return;

                //pageBuffer = page;

                if (p != page && (2 * page - p) >= 0)
                {
                    try
                    {
                        await BufferPage(2 * page - p);
                    }
                    catch { }
                }

                if (cancelDraw) return;

                if (Math.Abs(page - pageNum) > 3)
                    return;

                //pageBuffer = page;
            }
        }

        private async Task BufferPage(int p, CancellationToken token = default(CancellationToken))
        {
            int count = pageCount;
            ApplicationViewState currentViewState = ApplicationView.Value;
            if (currentViewState == ApplicationViewState.FullScreenPortrait)
            {
                count = 2 * (pageCount - 1);
            }

            if (p > pageNum + NUM_NEIGHBOURS_BUFFER ||
                p < pageNum - NUM_NEIGHBOURS_BUFFER)
                return;

            if (pages[p].Image != null) return;

            //pages[p].Loading = true;

            // we draw the image at the default zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * defaultZoomFactor * offsetZF);
            int height = (int)(size.Y * defaultZoomFactor * offsetZF);
            if (cancelDraw) return;

            // load page to a bitmap buffer on a background thread

            if (currentViewState != ApplicationViewState.FullScreenPortrait)
            {
                if (p > 0 && p < count - 1)
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
            else
            {
                var image = await DrawToBufferAsync(document, p, width, height);
                //pages[p].Loading = false;
                pages[p].Image = image;
            }
        }


        // Handle zooming and scrolling
        void scrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (switchOrientation) return;

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
                    if (scr != null && Math.Abs(scr.ZoomFactor - defaultZoomFactor) > 0.04)
                    {
                        scr.ZoomToFactor(defaultZoomFactor);
                    }

                    pageNum = p;

                    pages[pageNum].ZoomedImage = null;
                    pages[pageNum].FirstPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].SecondPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].ZoomFactor = defaultZoomFactor;
                    item = pagesListView.ItemContainerGenerator.ContainerFromIndex(pageNum) as GridViewItem;
                    scr = findFirstInVisualTree<ScrollViewer>(item);
                    if (scr != null && Math.Abs(scr.ZoomFactor - defaultZoomFactor) > 0.04)
                    {
                        scr.ZoomToFactor(defaultZoomFactor);
                    }

                    var task1 = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            await InitPageLink(pageNum);
                        }
                        catch { }
                    });

                    currentZoomFactor = defaultZoomFactor;

                    if (cancelDraw) return;
                    if (isBusyRedraw && !cancelDraw)
                    {
                        cancelDraw = true;
                        document.CancelDraw();
                        return;
                    }
                    else if (!isBuffering && NeedToBufferPages(pageNum, NUM_NEIGHBOURS_BUFFER - 1))
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
            if (switchOrientation) return;

            if (!e.IsIntermediate)
            {
                var scrView = sender as ScrollViewer;
                if (isBusyRedraw && Math.Abs(currentZoomFactor - scrView.ZoomFactor) < 0.1) return;
                currentZoomFactor = scrView.ZoomFactor;

                if (Math.Abs(scrView.ZoomFactor - pages[pageNum].ZoomFactor) < 0.1) return;

                if (cancelDraw)
                {
                    needRedraw = true;
                    return;
                }

                if (Math.Abs(currentZoomFactor - defaultZoomFactor) < 0.1 && ApplicationView.Value != ApplicationViewState.FullScreenPortrait)
                {
                    if (isBusyRedraw && !cancelDraw)
                    {
                        cancelDraw = true;
                        needRedraw = false;
                        document.CancelDraw();
                    }
                    pages[pageNum].ZoomedImage = null;
                    pages[pageNum].FirstPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].SecondPageZoomFactor = defaultZoomFactor;
                    pages[pageNum].ZoomFactor = defaultZoomFactor;
                    return;
                }

                if (NeedToUpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW))
                {
                    if ((isBusyRedraw || isBuffering) && !cancelDraw)
                    {
                        cancelDraw = true;
                        document.CancelDraw();
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
            cancelDraw = false;
            if (needRedraw && !isBuffering && !isBusyRedraw)
            {
                needRedraw = false;
                var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(pageNum);
                var scroll = findFirstInVisualTree<ScrollViewer>(root);
                if (scroll != null)
                {
                    ScrollViewer_ViewChanged_1(scroll, new ScrollViewerViewChangedEventArgs());
                }
            }
            else if (!isBuffering && !isBusyRedraw)
            {
                needBuffer = false;
                if (NeedToBufferPages(pageNum, NUM_NEIGHBOURS_BUFFER - 1))
                {
                    Task task = BufferPages(pageNum, NUM_NEIGHBOURS_BUFFER);
                }
            }
        }

        private async Task InitPageLink(int page)
        {
            await Task.Delay(550);

            if (page != pageNum) return;

            if (page == 0 || ApplicationView.Value == ApplicationViewState.FullScreenPortrait)
            {
                if (pages[page].Links == null)
                {
                    var links = document.GetLinks(page);

                    var linkVistor = new LinkInfoVisitor();
                    linkVistor.OnURILink += visitor_OnURILink;
                    linkVistor.OnInternalLink += vis_OnInternalLink;
                    visitorList.Add(new LinkInfo() { visitor = linkVistor, index = page, count = links.Count, handled = 0 });

                    pages[page].Links = new ObservableCollection<PageLink>();

                    await Task.Run(() =>
                    {
                        for (int j = 0; j < links.Count; j++)
                        {
                            links[j].AcceptVisitor(linkVistor);
                        }
                    });
                }

                await LoadLinks(page);

                return;
            }

            if (page == pageCount - 1)
            {
                if (pages[pageCount - 1].Links == null)
                {
                    var links = document.GetLinks(0);

                    var linkVistor = new LinkInfoVisitor();
                    linkVistor.OnURILink += visitor_OnURILink;
                    linkVistor.OnInternalLink += vis_OnInternalLink;
                    visitorList.Add(new LinkInfo() { visitor = linkVistor, index = pageCount - 1, count = links.Count, handled = 0 });

                    pages[pageCount - 1].Links = new ObservableCollection<PageLink>();

                    await Task.Run(() =>
                    {
                        for (int j = 0; j < links.Count; j++)
                        {
                            links[j].AcceptVisitor(linkVistor);
                        }
                    });
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
                vis.OnInternalLink += vis_OnInternalLink;

                pages[page].LinksLeft = new ObservableCollection<PageLink>();

                await Task.Run(() =>
                {
                    foreach (var link in links)
                    {
                        link.AcceptVisitor(vis);
                    }
                });
            }

            if (pages[page].LinksRight == null)
            {
                var links = document.GetLinks(2 * page);
                LinkInfoVisitor vis = new LinkInfoVisitor();
                visitorList.Add(new LinkInfo() { visitor = vis, index = 2 * page, count = links.Count, handled = 0 });
                vis.OnURILink += visitor_OnURILink;
                vis.OnInternalLink += vis_OnInternalLink;

                pages[page].LinksRight = new ObservableCollection<PageLink>();

                await Task.Run(() =>
                {
                    foreach (var link in links)
                    {
                        link.AcceptVisitor(vis);
                    }
                });
            }

            await LoadLinks(page);
        }

        void vis_OnInternalLink(LinkInfoVisitor __param0, LinkInfoInternal __param1)
        {
            foreach (var visitor in visitorList)
            {
                if (visitor.visitor == __param0)
                {
                    int index = 0;

                    PageLink link = new PageLink();
                    link.rect = new Rect(__param1.Rect.Left, __param1.Rect.Top, __param1.Rect.Right - __param1.Rect.Left, __param1.Rect.Bottom - __param1.Rect.Top);
                    link.PageNumber = __param1.PageNumber;

                    if (visitor.index == 0 || visitor.index == pageCount - 1 || ApplicationView.Value == ApplicationViewState.FullScreenPortrait)
                    {
                        pages[visitor.index].Links.Add(link);
                    }
                    else if (visitor.index == pageCount - 1)
                    {
                        pages[pageCount - 1].Links.Add(link);
                    }
                    else if (visitor.index % 2 == 0)
                    {
                        index = (int)(visitor.index / 2);
                        pages[index].LinksRight.Add(link);
                    }
                    else if (visitor.index % 2 == 1)
                    {
                        index = (int)(visitor.index / 2) + 1;
                        pages[index].LinksLeft.Add(link);
                    }
                }
            }
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

                    if (visitor.index == 0 || visitor.index == pageCount - 1 || ApplicationView.Value == ApplicationViewState.FullScreenPortrait)
                    {
                        //if (pages[index].Links == null)
                        //{
                        //    pages[index].Links = new ObservableCollection<PageLink>();
                        //}
                        pages[visitor.index].Links.Add(link);
                    }
                    else if (visitor.index == pageCount - 1)
                    {
                        pages[pageCount - 1].Links.Add(link);
                    }
                    else if (visitor.index % 2 == 0)
                    {
                        index = (int)(visitor.index / 2);
                        //if (pages[index].LinksRight == null)
                        //{
                        //    pages[index].LinksRight = new ObservableCollection<PageLink>();
                        //}
                        pages[index].LinksRight.Add(link);
                    }
                    else if (visitor.index % 2 == 1)
                    {
                        index = (int)(visitor.index / 2) + 1;
                        //if (pages[index].LinksLeft == null)
                        //{
                        //    pages[index].LinksLeft = new ObservableCollection<PageLink>();
                        //}
                        pages[index].LinksLeft.Add(link);
                    }

                    //var vis = visitorList[visitorList.IndexOf(visitor)];
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

            if (page != pageNum) return;

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
                    if (root == null) return;
                    var scr = findFirstInVisualTree<ScrollViewer>(root);
                    if (scr == null) return;
                    var children = VisualTreeHelper.GetChild(scr, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    var grid = children as Grid;

                    var rect = new Rect(item.rect.Left, item.rect.Top, item.rect.Width, item.rect.Height);
                    if (item.IsInternalLink)
                    {
                        var button = new PageButton();
                        button.SetRect(rect, item.PageNumber, offsetZF);
                        if (page != pageNum) return;
                        grid.Children.Add(button);
                        button.InternalClicked += button_InternalClicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = "InternalLink", PageNum = item.PageNumber });
                    }
                    else if (DownloadManager.IsFullScreenButton(item.url) || DownloadManager.IsLink(item.url))
                    {
                        var button = new PageButton();
                        button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        if (page != pageNum) return;
                        grid.Children.Add(button);
                        button.Clicked += button_Clicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url });
                    }
                    else if (DownloadManager.IsImage(item.url))
                    {
                        var slideShow = new SlideShow();
                        await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        if (page != pageNum) return;
                        grid.Children.Add(slideShow);
                        pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url });
                    }
                    else if (DownloadManager.IsVideo(item.url))
                    {
                        var videoPlayer = new VideoPlayer();
                        videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF);
                        if (page != pageNum) return;
                        grid.Children.Add(videoPlayer);
                        pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url });
                    }
                }
            }

            if (pages[page].LinksLeft != null)
            {
                if (page != pageNum) return;

                foreach (var item in pages[page].LinksLeft)
                {
                    bool alreadyInserted = false;
                    foreach (var addon in pages[page].Addons)
                    {
                        if (addon.page == ActivePage.Left &&
                            addon.url == item.url)
                        {
                            alreadyInserted = true;
                            break;
                        }
                    }

                    if (alreadyInserted) continue;

                    var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
                    if (root == null) return;
                    var scr = findFirstInVisualTree<ScrollViewer>(root);
                    if (scr == null) return;
                    var children = VisualTreeHelper.GetChild(scr, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    children = VisualTreeHelper.GetChild(children, 0);
                    if (children == null) return;
                    var grid = children as Grid;

                    var rect = new Rect(item.rect.Left, item.rect.Top, item.rect.Width, item.rect.Height);
                    if (item.IsInternalLink)
                    {
                        var button = new PageButton();
                        button.SetRect(rect, item.PageNumber, offsetZF);
                        if (page != pageNum) return;
                        grid.Children.Add(button);
                        button.InternalClicked += button_InternalClicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = "InternalLink", PageNum = item.PageNumber });
                    }
                    else if (DownloadManager.IsFullScreenButton(item.url) || DownloadManager.IsLink(item.url))
                    {
                        var button = new PageButton();
                        button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        if (page != pageNum) return;
                        grid.Children.Add(button);
                        button.Clicked += button_Clicked;
                        pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url, page = ActivePage.Left });
                    }
                    else if (DownloadManager.IsImage(item.url))
                    {
                        var slideShow = new SlideShow();
                        await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        if (page != pageNum) return;
                        grid.Children.Add(slideShow);
                        pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url, page = ActivePage.Left });
                    }
                    else if (DownloadManager.IsVideo(item.url))
                    {
                        var videoPlayer = new VideoPlayer();
                        videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                        if (page != pageNum) return;
                        grid.Children.Add(videoPlayer);
                        pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url, page = ActivePage.Left });
                    }
                }
            }

            if (pages[page].LinksRight != null)
            {
                if (page != pageNum) return;

                foreach (var item in pages[page].LinksRight)
                {
                    bool alreadyInserted = false;
                    foreach (var addon in pages[page].Addons)
                    {
                        if (addon.page == ActivePage.Right && 
                            addon.url == item.url)
                        {
                            alreadyInserted = true;
                            break;
                        }
                    }

                    if (alreadyInserted) continue;

                        var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
                        if (root == null) return;
                        var scr = findFirstInVisualTree<ScrollViewer>(root);
                        if (scr == null) return;
                        var children = VisualTreeHelper.GetChild(scr, 0);
                        if (children == null) return;
                        children = VisualTreeHelper.GetChild(children, 0);
                        if (children == null) return;
                        children = VisualTreeHelper.GetChild(children, 0);
                        if (children == null) return;
                        children = VisualTreeHelper.GetChild(children, 0);
                        if (children == null) return;
                        var grid = children as Grid;

                        var rect = new Rect(item.rect.Left + (pages[page].PageWidth / 2 / (offsetZF * defaultZoomFactor)), item.rect.Top, item.rect.Width, item.rect.Height);
                        if (item.IsInternalLink)
                        {
                            var button = new PageButton();
                            button.SetRect(rect, item.PageNumber, offsetZF);
                            if (page != pageNum) return;
                            grid.Children.Add(button);
                            button.InternalClicked += button_InternalClicked;
                            pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = "InternalLink", PageNum = item.PageNumber });
                        }
                        else if (DownloadManager.IsFullScreenButton(item.url) || DownloadManager.IsLink(item.url))
                        {
                            var button = new PageButton();
                            button.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                            if (page != pageNum) return;
                            grid.Children.Add(button);
                            button.Clicked += button_Clicked;
                            pages[page].Addons.Add(new UIAddon { element = button, type = UIType.PageButton, url = item.url, page = ActivePage.Right });
                        }
                        else if (DownloadManager.IsImage(item.url))
                        {
                            var slideShow = new SlideShow();
                            await slideShow.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                            if (page != pageNum) return;
                            grid.Children.Add(slideShow);
                            pages[page].Addons.Add(new UIAddon { element = slideShow, type = UIType.SlideShow, url = item.url, page = ActivePage.Right });
                        }
                        else if (DownloadManager.IsVideo(item.url))
                        {
                            var videoPlayer = new VideoPlayer();
                            videoPlayer.SetRect(rect, pdfStream.folderUrl, item.url, offsetZF * defaultZoomFactor);
                            if (page != pageNum) return;
                            grid.Children.Add(videoPlayer);
                            pages[page].Addons.Add(new UIAddon { element = videoPlayer, type = UIType.VideoPlayer, url = item.url, page = ActivePage.Right });
                        }
                }
            }
        }

        void button_InternalClicked(int pageNum)
        {
            if (ApplicationView.Value != ApplicationViewState.FullScreenPortrait)
            {
                if (pageNum > 0)
                {
                    pageNum = (pageNum + 1) / 2;
                }
            }
            if (pageNum > 0)
            {
                pagesListView.ScrollIntoView(pages[pageNum]);
            }
        }

        void RemovePageItems(int page)
        {
            if (page < 0 || page >= pages.Count)  return;
            foreach (var addon in pages[page].Addons)
            {
                var video = addon.element as VideoPlayer;
                if (video != null)
                {
                    video.Close();
                }
                addon.element.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            var root = pagesListView.ItemContainerGenerator.ContainerFromIndex(page);
            if (root == null) { pages[page].Addons.Clear(); return; }
            var scr = findFirstInVisualTree<ScrollViewer>(root);
            if (scr == null) { pages[page].Addons.Clear(); return; }
            var children = VisualTreeHelper.GetChild(scr, 0);
            if (children == null) { pages[page].Addons.Clear(); return; }
            children = VisualTreeHelper.GetChild(children, 0);
            if (children == null) { pages[page].Addons.Clear(); return; }
            children = VisualTreeHelper.GetChild(children, 0);
            if (children == null) { pages[page].Addons.Clear(); return; }
            children = VisualTreeHelper.GetChild(children, 0);
            if (children == null) { pages[page].Addons.Clear(); return; }
            var grid = children as Grid;
            foreach (var addon in pages[page].Addons)
            {
                grid.Children.Remove(addon.element);
            }
            pages[page].Addons.Clear();
        }

        void button_Clicked(string folderUrl, string url)
        {
            if (!DownloadManager.IsLink(url))
            {
                fullScreenContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                var task = fullScreenContainer.Load(folderUrl, url);
            }
            else
            {
                var task = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
            }
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

                    ApplicationViewState currentViewState = ApplicationView.Value;

                    if (currentViewState != ApplicationViewState.FullScreenPortrait)
                    {
                        if (pages != null && pages.Count > 1)
                        {
                            if (pages[1].Width < pages[1].Height)
                            {
                                pageNum = (pageNum + 1) / 2;
                                switchOrientation = true;
                            }
                        }

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

                        pagesListView.ItemsSource = null;
                        pages.Clear();

                        for (int p = 0; p < pageCount; p++)
                        {
                            if (p == 0 || p == pageCount - 1)
                            {
                                // add the loading DataTemplate to the UI       
                                var data = new PageData()
                                {
                                    Image = null,
                                    Width = Window.Current.Bounds.Width,
                                    Height = Window.Current.Bounds.Height,
                                    Idx = p + 1,
                                    ZoomFactor = defaultZoomFactor,
                                    FirstPageZoomFactor = defaultZoomFactor,
                                    SecondPageZoomFactor = defaultZoomFactor,
                                    PageWidth = width,
                                    PageHeight = height,
                                    Loading = false
                                };
                                pages.Add(data);
                            }
                            else
                            {
                                // add the loading DataTemplate to the UI       
                                var data = new PageData()
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
                            var item = pagesListView.ItemContainerGenerator.ContainerFromIndex(p) as GridViewItem;
                            var scr = findFirstInVisualTree<ScrollViewer>(item);
                            if (scr != null)
                            {
                                scr.ZoomToFactor(defaultZoomFactor);
                            }
                        }
                        pagesListView.ItemsSource = pages;
                    }
                    else
                    {
                        if (pages != null && pages.Count > 1)
                        {
                            if (pages[1].Width > pages[1].Height)
                            {
                                if (pageNum > 0)
                                {
                                    pageNum = pageNum * 2 - 1;
                                    switchOrientation = true;
                                }
                            }
                        }
                        // calculate display zoom factor
                        defaultZoomFactor = CalculateZoomFactor1(size.X);
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

                        pagesListView.ItemsSource = null;
                        pages.Clear();
                        for (int p = 0; p <= 2 * (pageCount - 1) - 1; p++)
                        {
                            // add the loading DataTemplate to the UI       
                            var data = new PageData()
                            {
                                Image = null,
                                Width = Window.Current.Bounds.Width,
                                Height = Window.Current.Bounds.Height,
                                Idx = p + 1,
                                ZoomFactor = defaultZoomFactor,
                                FirstPageZoomFactor = defaultZoomFactor,
                                SecondPageZoomFactor = defaultZoomFactor,
                                PageWidth = width,
                                PageHeight = height,
                                Loading = false
                            };
                            pages.Add(data);

                            var item = pagesListView.ItemContainerGenerator.ContainerFromIndex(p) as GridViewItem;
                            var scr = findFirstInVisualTree<ScrollViewer>(item);
                            if (scr != null)
                            {
                                scr.ZoomToFactor(defaultZoomFactor);
                            }
                        }
                        pagesListView.ItemsSource = pages;
                    }
                }

                var task = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (cancelDraw)
                    {
                        needBuffer = true;
                        return;
                    }

                    if ((isBusyRedraw || isBuffering) && !cancelDraw)
                    {
                        cancelDraw = true;
                        document.CancelDraw();
                        needBuffer = true;
                        return;
                    }

                    pagesListView.ScrollIntoView(pages[pageNum]);
                    await BufferPages(pageNum, NUM_NEIGHBOURS_BUFFER);

                    switchOrientation = false;

                    await InitPageLink(pageNum);
                });
            }
        }

    }
}