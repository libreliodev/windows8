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

        public async void Start(int interval, StorageFolder folder, List<string> links)
        {
            int i = 0;
            while (true)
            {
                await Task.Delay(interval);

                i++;
                if (i == links.Count)
                    i = 0;

                var file = await folder.GetFileAsync(links[i]).AsTask();
                image.Source = new BitmapImage(new Uri(links[i]));
            }
        }
    }
}
