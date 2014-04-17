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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public sealed partial class FullScreenContainer : UserControl
    {
        private SlideShow slideShow = null;
        private VideoPlayer videoPlayer = null;

        public FullScreenContainer()
        {
            this.InitializeComponent();
        }

        public async Task Load(string folderUrl, string url)
        {
            var rect = new Rect(0, 0, 500, 300);
            if (DownloadManager.IsImage(url))
            {
                slideShow = new SlideShow();
                await slideShow.SetRect(rect, folderUrl, url, 1f);
                progressLoad.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                frame.Children.Add(slideShow);
            }
            else if (DownloadManager.IsVideo(url))
            {
                rect = new Rect(0, 0, Window.Current.Bounds.Width, Window.Current.Bounds.Height);
                videoPlayer = new VideoPlayer();
                videoPlayer.SetRect(rect, folderUrl, url, 1f);
                progressLoad.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                frame.Children.Add(videoPlayer);
            }
        }

        private void Grid_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            if (slideShow != null)
            {
                frame.Children.Remove(slideShow);
            }

            if (videoPlayer != null)
            {
                frame.Children.Remove(videoPlayer);
            }

            progressLoad.Visibility = Windows.UI.Xaml.Visibility.Visible;
            this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
