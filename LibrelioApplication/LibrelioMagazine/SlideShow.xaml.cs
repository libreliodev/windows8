using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.ObjectModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindMagazine
{
    public class ImageData : LibrelioApplication.Common.BindableBase
    {
        //public string Url { get; set; }
        private ImageSource _image = null;
        private double _width = 0;
        private double _height = 0;
        bool _notDownloaded = false;
        bool _hidden = false;

        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public bool NotDownloaded
        {
            get { return _notDownloaded; }
            set
            {
                _notDownloaded = value;
                OnPropertyChanged("NotDownloaded");
            }
        }

        public bool Hidden
        {
            get { return _hidden; }
            set
            {
                _hidden = value;
                OnPropertyChanged("Hidden");
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
    }

    public sealed partial class SlideShow : UserControl
    {
        private Point initialPoint;
        bool isSwiping = false;

        int length = 0;
        int currentImage = 0;

        ObservableCollection<ImageData> images = new ObservableCollection<ImageData>();

        ScrollViewer scrollViewer;

        public SlideShow()
        {
            this.InitializeComponent();
        }

        public async void Start(int interval)//, StorageFolder folder, List<string> links)
        {
            
            //int i = 1;
            //while (true)
            //{
            //    var str = "ms-appx:///Assets/test/img/sample_" + i + ".jpg";
            //    //image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(str));
            //    images.Add(new ImageData() { Image = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(str)), Width = this.Width, Height = this.Height } );

            //    //await Task.Delay(interval);

            //    i++;
            //    if (i == 6)
            //        break;

            //    //var file = await folder.GetFileAsync(links[i]).AsTask();
            //    //image.Source = new BitmapImage(new Uri(links[i]));
            //}

            //itemListView.ItemsSource = images;
        }

        public async Task SetRect(Rect rect, string folderUrl, string url, float offset)
        {
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
            this.Width = rect.Width * offset;
            this.Height = rect.Height * offset;

            string startMame = null;
            string endName = null;
            images.Add(new ImageData() { Image = null, Width = this.Width, Height = this.Height });
            StorageFolder folder = null;
            try
            {
                folder = KnownFolders.DocumentsLibrary;
                foreach (var fld in folderUrl.Split('\\'))
                {
                    if (fld != "")
                    {
                        folder = await folder.GetFolderAsync(fld);
                    }
                }

                int start = 0;
                int end = 0;
                if (url.Contains(".jpg"))
                {
                    start = url.IndexOf('_');
                    end = url.IndexOf(".jpg");

                    startMame = url.Substring(0, start + 1);
                    startMame = startMame.Replace("http://localhost/", "");
                    endName = ".jpg";
                }
                else if (url.Contains(".png"))
                {
                    start = url.IndexOf('_');
                    end = url.IndexOf(".png");

                    startMame = url.Substring(0, start + 1);
                    endName = ".png";
                }

                if (start == -1 || end == -1)
                {
                    throw new Exception();
                }

                var test = url.Substring(start + 1, end - start);
                length = Convert.ToInt32(url.Substring(start + 1, end - start - 1));

            }
            catch
            {
                images.Add(new ImageData() { Image = null, Width = this.Width, Height = this.Height });

                for (int p = 0; p < 7; p++)
                {
                    images.Add(new ImageData() { Image = null, NotDownloaded = true, Width = this.Width, Height = this.Height });
                }
                return;
            }

            for (int i = 1; i <= length; i++)
            {
                StorageFile file = null;
                try
                {
                    var str = folder.Path + "\\" + startMame + i + endName;
                    file = await StorageFile.GetFileFromPathAsync(folder.Path + "\\" + startMame + i + endName);
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var unprotected = await LibrelioApplication.DownloadManager.UnprotectPDFStream(stream);
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(unprotected);
                        images.Add(new ImageData() { Image = bitmap, Hidden = true, Width = this.Width, Height = this.Height });
                    }
                }
                catch
                {
                    images.Add(new ImageData() { Image = null, NotDownloaded = true, Width = this.Width, Height = this.Height });
                }
            }

            images.Add(new ImageData() { Image = null, Width = this.Width, Height = this.Height });
            itemListView.ItemsSource = images;
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

        private void itemListView_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = findFirstInVisualTree<ScrollViewer>(itemListView);
            if (scrollViewer != null)
            {
                scrollViewer.HorizontalSnapPointsType = SnapPointsType.MandatorySingle;
                scrollViewer.HorizontalSnapPointsAlignment = SnapPointsAlignment.Near;
            }
        }

    }
}
