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

            StorageFolder folder = null;
            try
            {
                folder = await StorageFolder.GetFolderFromPathAsync(folderUrl);
            }
            catch
            {
                noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
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

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            videoPlayer.Width = this.Width;
                            videoPlayer.Height = this.Height;
                            videoPlayer.SetSource(unprotected, file.ContentType);
                            videoPlayer.Play();
                        });
                }
            }
            catch
            {
                noAsset.Visibility = Windows.UI.Xaml.Visibility.Visible;
                return;
            }

            //autoSlide = true;
            //interval = 4000;

            //noTranstions = true;
        }
    }
}
