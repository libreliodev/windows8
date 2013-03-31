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

        public void SetRect(Rect rect)
        {
            frame.Margin = new Thickness(rect.Top, rect.Left, rect.Right, rect.Bottom);
            image.Width = rect.Width;
            image.Height = rect.Height;
        }
    }
}
