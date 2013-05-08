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
using System.Threading;
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

        private bool fullScreen = false;
        private bool UiHidden = false;

        CancellationTokenSource cancel = null;

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

        public void Close()
        {
            webView.NavigateToString("<html><body></body></html>");
            webView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        async void Start()
        {
            started = true;
            frame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,0,0));
            content.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (DownloadManager.IsEmbedAsset(_url)) return;

            if (!error && loaded)
            {
                videoPlayer.Play();
                btnPlayPause.Content = "\xe103";
                paused = false;
            }

            if (DownloadManager.IsFullScreenAsset(_url)) return;

            await Task.Delay(5000);
            HideUI();
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

        private async void frame_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //e.Handled = true;

            if (!started && !error)
            {
                Start();
            }
            else if (fullScreen)
            {
                CloseFullScreen();
            }
            else if (UiHidden)
            {
                ShowUI();
                UiHidden = false;

                cancel = new CancellationTokenSource();

                try
                {
                    await Task.Delay(5000, cancel.Token);
                }
                catch
                {
                }

                if (!fullScreen && cancel != null && !cancel.Token.IsCancellationRequested)
                {
                    HideUI();
                }
                cancel = null;
            }
            else if (!fullScreen && !UiHidden)
            {
                HideUI();
                if (cancel != null)
                {
                    cancel.Cancel();
                }
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
                btnFullScreen.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            videoPlayer.Width = this.Width;
            videoPlayer.Height = this.Height;

            webView.Width = this.Width;
            webView.Height = this.Height;

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

                    var tmp = await ApplicationData.Current.LocalFolder.CreateFileAsync("tmp.mp4", CreationCollisionOption.ReplaceExisting);
                    using (var tmpStream = await tmp.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        await RandomAccessStream.CopyAndCloseAsync(unprotected.GetInputStreamAt(0), tmpStream.GetOutputStreamAt(0));
                        await tmpStream.FlushAsync();
                    }
                    tmp = null;
                    tmp = await ApplicationData.Current.LocalFolder.GetFileAsync("tmp.mp4");
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
            else
            {
                Start();
                videoPlayer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                webView.Visibility = Windows.UI.Xaml.Visibility.Visible;
                webView.Navigate(new Uri(_url));
                HideUI();
            }
        }

        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (!fullScreen)
            {
                SetFullScreen();
            }
            else
            {
                CloseFullScreen();
            }
        }

        private void SetFullScreen()
        {
            this.Margin = new Thickness(0);
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            videoPlayer.Width = Window.Current.Bounds.Width;
            videoPlayer.Height = Window.Current.Bounds.Height;
            btnFullScreen.Content = "\xe1D8";
            fullScreen = true;

            if (UiHidden)
            {
                ShowUI();
            }
        }

        async void CloseFullScreen()
        {
            this.Margin = new Thickness(_rect.Left * _offset, (_rect.Top + 1.5) * _offset, 0, 0);
            this.Width = _rect.Width * _offset;
            this.Height = _rect.Height * _offset;
            videoPlayer.Width = this.Width;
            videoPlayer.Height = this.Height;
            btnFullScreen.Content = "\xe1D9";

            fullScreen = false;
            UiHidden = false;

            cancel = new CancellationTokenSource();

            try
            {
                await Task.Delay(5000, cancel.Token);
            }
            catch
            {
            }

            if (!fullScreen && cancel != null && !cancel.Token.IsCancellationRequested)
            {
                HideUI();
            }
            cancel = null;
        }

        private void ShowUI()
        {
            if (DownloadManager.IsEmbedAsset(_url)) return;

            controlsFrame.Visibility = Windows.UI.Xaml.Visibility.Visible;
            UiHidden = false;
        }

        private void HideUI()
        {
            controlsFrame.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            UiHidden = true;
        }

        private void frame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
