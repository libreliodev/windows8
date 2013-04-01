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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WindMagazine
{
    public sealed partial class SlideShow : UserControl
    {
        public SlideShow()
        {
            this.InitializeComponent();
        }

        public async void Start(int interval)//, StorageFolder folder, List<string> links)
        {
            int i = 1;
            while (true)
            {
                var str = "ms-appx:///Assets/test/img/sample_" + i + ".jpg";
                image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage(new Uri(str));

                await Task.Delay(interval);

                i++;
                if (i == 6)
                    i = 1;

                //var file = await folder.GetFileAsync(links[i]).AsTask();
                //image.Source = new BitmapImage(new Uri(links[i]));
            }
        }

        public void SetRect(Rect rect, float offset)
        {
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
            this.Width = rect.Width * offset;
            this.Height = rect.Height * offset;
            image.Width = rect.Width * offset;
            image.Height = rect.Height * offset;
        }

        private void frame_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            int x = 0;
        }

        private void frame_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            int x = 0;
        }
    }
}
