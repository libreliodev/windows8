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

        public VideoPlayer()
        {
            this.InitializeComponent();
        }

        public async Task SetRect(Rect rect, string folderUrl, string url, float offset)
        {
            if (!DownloadManager.IsFullScreenAsset(url))
            {
                this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
                this.Width = rect.Width * offset;
                this.Height = rect.Height * offset;
            }
            else
            {
                this.Width = rect.Width;
                this.Height = rect.Height;
                this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            }
            videoPlayer.Width = this.Width;
            videoPlayer.Height = this.Height;

            if (DownloadManager.IsLocalAsset(url))
            {
                StorageFolder folder = null;
                try
                {
                    folder = await StorageFolder.GetFolderFromPathAsync(folderUrl);
                }
                catch
                {
                    error = true;
                    noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (DownloadManager.IsFullScreenAsset(url) || DownloadManager.IsAutoPlay(url))
                    {
                        Start();
                    }
                    return;
                }

                var pos = url.IndexOf(".mp4");

                StorageFile file = null;
                try
                {
                    var str = folder.Path + "\\" + url.Substring(0, pos).Replace("http://localhost/", "") + ".mp4";
                    file = await StorageFile.GetFileFromPathAsync(str);
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        var unprotected = await DownloadManager.UnprotectPDFStream(stream);

                        videoPlayer.MediaOpened += videoPlayer_MediaOpened;
                        videoPlayer.MediaFailed += videoPlayer_MediaFailed;
                        videoPlayer.MediaEnded += videoPlayer_MediaEnded;
                        videoPlayer.SetSource(unprotected, file.ContentType);

                        if (DownloadManager.IsFullScreenAsset(url) || DownloadManager.IsAutoPlay(url))
                        {
                            Start();
                        }
                    }
                }
                catch
                {
                    error = true;
                    noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (DownloadManager.IsFullScreenAsset(url) || DownloadManager.IsAutoPlay(url))
                    {
                        Start();
                    }
                    return;
                }
            }
        }

        async void Start()
        {
            started = true;
            frame.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255,0,0,0));
            content.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (!error)
            {
                videoPlayer.Play();
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
            btnPlayPause.Content = "\xe103";
            //videoPlayer.Play();
            //paused = false;
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
            e.Handled = true;

            if (!started && !error)
            {
                Start();
            }
        }
    }
}
