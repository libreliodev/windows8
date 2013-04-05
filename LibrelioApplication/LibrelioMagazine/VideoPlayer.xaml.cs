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
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public sealed partial class VideoPlayer : UserControl
    {
        private bool paused = false;
        private bool error = false;
        private bool started = false;
        private bool loaded = false;

        private bool init = false;
        private Rect _rect = new Rect();
        string _folderUrl = "";
        string _url = "";
        float _offset = 0f;

        public VideoPlayer()
        {
            this.InitializeComponent();
        }

        public void SetRect(Rect rect, string folderUrl, string url, float offset)
        {
            if (init) return;

            _rect = rect;
            _folderUrl = folderUrl;
            _url = url;
            _offset = offset;
        }

        async void Start()
        {
            started = true;
            frame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,0,0));
            content.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (!error && loaded)
            {
                videoPlayer.Play();
                btnPlayPause.Content = "\xe103";
                paused = false;
            }
        }

        void videoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            paused = true;
            btnPlayPause.Content = "\xe102";
        }

        void videoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            error = true;
            noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        void videoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            loaded = true;
            if (started)
            {
                videoPlayer.Play();
                btnPlayPause.Content = "\xe103";
                paused = false;
            }
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (!paused && videoPlayer.CanPause && !error)
            {
                videoPlayer.Pause();
                btnPlayPause.Content = "\xe102";
                paused = true;
            }
            else
            {
                videoPlayer.Play();
                btnPlayPause.Content = "\xe103";
                paused = false;
            }
        }

        private void frame_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //e.Handled = true;

            if (!started && !error)
            {
                Start();
            }
        }

        private async void frame_Loaded(object sender, RoutedEventArgs e)
        {
            init = true;

            if (!DownloadManager.IsFullScreenAsset(_url))
            {
                this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                this.Margin = new Thickness(_rect.Left * _offset, (_rect.Top + 1.5) * _offset, 0, 0);
                this.Width = _rect.Width * _offset;
                this.Height = _rect.Height * _offset;
            }
            else
            {
                this.Width = _rect.Width;
                this.Height = _rect.Height;
                this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            }
            videoPlayer.Width = this.Width;
            videoPlayer.Height = this.Height;

            if (DownloadManager.IsLocalAsset(_url))
            {
                StorageFolder folder = null;
                try
                {
                    folder = await StorageFolder.GetFolderFromPathAsync(_folderUrl);
                }
                catch
                {
                    error = true;
                    noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (DownloadManager.IsFullScreenAsset(_url) || DownloadManager.IsAutoPlay(_url))
                    {
                        Start();
                    }
                    return;
                }

                var pos = _url.IndexOf(".mp4");

                StorageFile file = null;
                try
                {
                    var str = folder.Path + "\\" + _url.Substring(0, pos).Replace("http://localhost/", "") + ".mp4";
                    file = await StorageFile.GetFileFromPathAsync(str);

                    var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                    var unprotected = await DownloadManager.UnprotectPDFStream(stream);

                    var tmp = await KnownFolders.DocumentsLibrary.CreateFileAsync("tmp.mp4", CreationCollisionOption.ReplaceExisting);
                    using (var tmpStream = await tmp.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(unprotected.GetInputStreamAt(0), tmpStream.GetOutputStreamAt(0));
                        await tmpStream.FlushAsync();
                    }
                    tmp = null;
                    tmp = await KnownFolders.DocumentsLibrary.GetFileAsync("tmp.mp4");
                    stream = await tmp.OpenAsync(FileAccessMode.Read);

                    videoPlayer.SetSource(stream, file.ContentType); 

                    if (DownloadManager.IsFullScreenAsset(_url) || DownloadManager.IsAutoPlay(_url))
                    {
                        Start();
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (DownloadManager.IsFullScreenAsset(_url) || DownloadManager.IsAutoPlay(_url))
                    {
                        Start();
                    }
                    return;
                }
            }
        }
    }
}
