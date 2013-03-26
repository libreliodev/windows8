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

        public string PageNumber
        {
            get { return _idx.ToString(); }
        }

        public float ZoomFactor { get; set; }

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

        IRandomAccessStream pdfStream = null;

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
        const int NUM_NEIGHBOURS_BUFFER = 8;
        // we don't buffer unless we jumped BUFFER_OFFSET pages
        const int BUFFER_OFFSET = 4;

        ScrollViewer scrollViewer = null;
        float defaultZoomFactor = 1.0f;
        float currentZoomFactor = 1.0f;
        float offsetZF = 1.0f;

        bool needUpdateView = false;
        bool cancelBuffering = false;
        bool isBusy = false;
        bool isBuffering = false;

        int pageBuffer = 0;

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
            pdfStream = navigationParameter as IRandomAccessStream;
            
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
            isBusy = true;

            var buffer = await GetPDFFileData();
            if (buffer == null) return;

            pdfStream = null;

            // create the MuPDF Document on a background thread
            document = await CreateDocumentAsync(buffer);

            pageCount = document.PageCount;

            await LoadPagesAsync();

            thumbsDoc = await CreateThumbsDocumentAsync(buffer);

            SetScrollViewer();

            pagesListView.ItemsSource = pages;
            startRing.IsActive = false;

            isBusy = false;

            var thumbsOp = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async() =>
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
            return (float)(rect.Height - 30) / height;
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
            if (pdfStream == null) return null;

            try
            {

                //var fileHandle =
                //    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\testmagazine.pdf");

                //using (IRandomAccessStream randomAccessStream = await fileHandle.OpenReadAsync())

                using (IInputStream inputStreamAt = pdfStream.GetInputStreamAt(0))
                using (var dataReader = new DataReader(inputStreamAt))
                {
                    uint u = await dataReader.LoadAsync((uint)pdfStream.Size);
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

            for (int p = 0; p < pageCount; p++)
            {
                thumbs.Add(new PageData() { Image = null, Idx = p + 1, ZoomFactor = 1.0f });
            }

            for (int p = 0; p < pageCount; p++)
            {
                MuPDFWinRT.Point size = thumbsDoc.GetPageSize(p);
                var width = 75;
                var height = size.Y * 75 / size.X;

                // load page to a bitmap buffer on a background thread
                var image = await DrawToBufferAsync(thumbsDoc, p, width, height);

                thumbs[p].Image = image;
            }
        }

        // Load initial pages async
        private async Task LoadPagesAsync()
        {
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

            for (int p = 0; p < pageCount; p++)
            {
                size = document.GetPageSize(p);
                int width = (int)(size.X * offsetZF);
                int height = (int)(size.Y * offsetZF);

                // add the loading DataTemplate to the UI       
                PageData data = new PageData() { Image = null, Idx = p + 1, Width = width, Height = height, ZoomFactor = defaultZoomFactor, Loading = false };
                pages.Add(data);
            }

            for (int p = 0; p < 6; p++)
            {
                size = document.GetPageSize(p);
                int width = size.X;
                int height = size.Y;

                width = (int)(size.X * currentZoomFactor * offsetZF);
                height = (int)(size.Y * currentZoomFactor * offsetZF);

                // load page to a bitmap buffer on a background thread
                var image = await DrawToBufferAsync(document, p, width, height);

                pages[p].Image = image;
                pages[p].ZoomFactor = currentZoomFactor;

                //data.Links = new List<PageLink>();
                PageLink link = new PageLink();
                //link.rect = new Rect(1400, 2800, 350, 400);
                link.rect = new Rect(0, 0, 800, 1000);
                //link.url = "http://localhost/sample_5.jpg?warect=full&waplay=auto1&wadelay=3000&wabgcolor=white";
                link.url = "http://localhost/sample_5.jpg?waplay=auto&wadelay=3000&wabgcolor=white";
                pages[p].Links.Add(link);
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
                scrollViewer.ViewChanged += scrollviewer_ViewChanged;

                if (currentZoomFactor < 1)
                {
                    scrollViewer.MinZoomFactor = (float)(currentZoomFactor * 0.8);
                    scrollViewer.MaxZoomFactor = 5.0f;
                    scrollViewer.ZoomToFactor(currentZoomFactor);
                }
                else
                {
                    scrollViewer.MinZoomFactor = 0.8f;
                }
            }
        }

        // Determine the page in the center screen
        private int CalcPageNum()
        {
            if (scrollViewer == null) return 0;

            var x = scrollViewer.HorizontalOffset + (scrollViewer.ViewportWidth / 2);

            var pageWidth = scrollViewer.ExtentWidth / pageCount;

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

        private bool NeedToUpdatePages(int page, int numNeighbours)
        {
            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < pageCount ? page + numNeighbours : pageCount - 1;
            for (int p = start; p <= end; p++)
            {
                if (pages[p].ZoomFactor != currentZoomFactor)
                    return true;
            }

            return false;
        }

        // Updates the pdf pages async
        // --------------------------------------------------------
        private void UpdatePages(int page, int numNeighbours)
        {
            // if we are already updating another page wait to finish then try again
            if (isBusy)
            {
                needUpdateView = true;
                return;
            }

            // if we are buffering pages cancel that operation
            if (isBuffering)
            {
                cancelBuffering = true;
                return;
            }

            isBusy = true;

            var op = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                await UpdatePagesInternal(page, numNeighbours);

                isBusy = false;
                // if we need to do something else fire the ViewChanged Event, else we're done
                if (needUpdateView || NeedToBufferPages(page, NUM_NEIGHBOURS_BUFFER))
                {
                    needUpdateView = false;
                    scrollviewer_ViewChanged(this, new ScrollViewerViewChangedEventArgs());
                }
            });
        }

        private async Task UpdatePagesInternal(int page, int numNeighbours)
        {
            // if the page no longer has focus we don't need to update it
            if (pageNum != page)
            {
                needUpdateView = true;
                return;
            }

            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < pageCount)
                    await RedrawPage(p, page);
                if (p != page && (2 * page - p) >= 0)
                    await RedrawPage(2 * page - p, page);
            }
        }

        private async Task RedrawPage(int p, int page)
        {
            // if the page no longer has focus we don't need to update it
            if (pageNum != page)
            {
                needUpdateView = true;
                return;
            }

            if (pages[p].ZoomFactor == currentZoomFactor)
                return;

            if (p == page && pages[p].Image == null)
            {
                await BufferPage(p);

                if (pageNum != page)
                {
                    needUpdateView = true;
                    return;
                }

                if (pages[p].ZoomFactor == currentZoomFactor)
                    return;
            }

            if (pages[p].Image == null)
                pages[p].Loading = true;

            // we resize the image to the current zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * currentZoomFactor * offsetZF);
            int height = (int)(size.Y * currentZoomFactor * offsetZF);

            // load page to a bitmap buffer on a background thread
            var image = await DrawToBufferAsync(document, p, width, height);

            if (pageNum == page)
            {
                pages[p].Image = image;
                pages[p].ZoomFactor = currentZoomFactor;
            }

            if (pages[p].Loading)
                pages[p].Loading = false;
        }


        // Buffers pdf pages in advance async
        // --------------------------------------------------------
        private bool NeedToBufferPages(int page, int numNeighbours)
        {
            var abs = pageNum > pageBuffer ? pageNum - pageBuffer : pageBuffer - pageNum;
            if (pageNum == pageBuffer || abs < BUFFER_OFFSET) return false;

            var start = page - numNeighbours >= 0 ? page - numNeighbours : 0;
            var end = page + numNeighbours < pageCount ? page + numNeighbours : pageCount - 1;
            for (int p = start; p <= end; p++)
            {
                if (pages[p].Image == null)
                    return true;
            }

            return false;
        }

        private void BufferPages(int page, int numNeighbours)
        {
            // if we are doing something else wait to finish
            if (isBusy || isBuffering)
            {
                needUpdateView = true;
                return;
            }

            isBuffering = true;

            var op = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () =>
            {
                await BufferPagesInternal(page, numNeighbours);

                if (cancelBuffering)
                {
                    cancelBuffering = false;
                    needUpdateView = true;
                }
                isBuffering = false;
                if (needUpdateView)
                {
                    needUpdateView = false;
                    scrollviewer_ViewChanged(this, new ScrollViewerViewChangedEventArgs());
                }
                else
                {
                    pageBuffer = pageNum;
                }
            });
        }

        private async Task BufferPagesInternal(int page, int numNeighbours)
        {
            if (cancelBuffering)
            {
                cancelBuffering = false;
                needUpdateView = true;
                return;
            }

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

            for (int p = end + 1; p < pageCount; p++)
            {
                if (pages[p].Image != null)
                {
                    pages[p].Image = null;
                    pages[p].ZoomFactor = defaultZoomFactor;
                }
            }

            for (int p = page; p <= page + numNeighbours; p++)
            {
                if (p < pageCount)
                    await BufferPage(p);
                if (p != page && (2 * page - p) >= 0)
                    await BufferPage(2 * page - p);
            }
        }

        private async Task BufferPage(int p)
        {
            if (p > pageNum + NUM_NEIGHBOURS_BUFFER ||
                p < pageNum - NUM_NEIGHBOURS_BUFFER)
                return;

            if (cancelBuffering)
            {
                cancelBuffering = false;
                needUpdateView = true;
                return;
            }

            if (pages[p].Image != null) return;

            pages[p].Loading = true;

            // we draw the image at the default zoom factor
            MuPDFWinRT.Point size = document.GetPageSize(p);
            int width = (int)(size.X * defaultZoomFactor);
            int height = (int)(size.Y * defaultZoomFactor);

            // load page to a bitmap buffer on a background thread
            var image = await DrawToBufferAsync(document, p, width, height);

            pages[p].Loading = false;
            pages[p].Image = image;
        }


        // Handle zooming and scrolling
        void scrollviewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            pageNum = CalcPageNum();
            currentZoomFactor = scrollViewer.ZoomFactor;

            if (!e.IsIntermediate && NeedToUpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW))
            {
                UpdatePages(pageNum, NUM_NEIGHBOURS_REDRAW);
            }
            else if (NeedToBufferPages(pageNum, NUM_NEIGHBOURS_BUFFER))
            {
                BufferPages(pageNum, NUM_NEIGHBOURS_BUFFER);
            }
        }
    }
}
