using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
namespace LibrelioApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SlideShowPage : Page
    {
        const int SLIDESHOW_AUTO_INTERVAL = 3000;

        private ObservableCollection<string> images;
        private int index = 0;
        bool bAuto = false;


        public SlideShowPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
		//TODO process NavigationEventArgs
            //string params = e.Uri;
            //e.Uri.
            //Fill images
            
            int dellay = SLIDESHOW_AUTO_INTERVAL;


            Uri uri = new Uri((string)e.Parameter);

            string[] _params = uri.Query.Replace("?", "").Split('&');
            //TODO case "warect"
            //http://localhost/photo_5.png?warect=full&waplay=auto&wadelay=2000&wabgcolor=white"
            for (int i = 0; i < _params.Length; i++) {
                string[] fields = _params[i].Split('=');
                string _name = fields[0];
                string _val = fields[1];
                switch (_name) { 
                    case "waplay":
                        if (_val == "auto")
                            bAuto = true;
                        break;
                    case "wadelay":
                        dellay = Int32.Parse(_val);
                        break;
                    case "wabgcolor":
                        if (_val == "white")
                            layoutRoot.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                        break;
                }
            }

            string path = "img/";
            string fileName = uri.LocalPath.Replace("/", "");
            string name = fileName.Split('_')[0];
            int from = 1;
            int to = Int32.Parse( fileName.Split('_')[1].Split('.')[0] );
            string sfx = "." + fileName.Split('_')[1].Split('.')[1];

            images = new ObservableCollection<string>();
            for (int i = from; i <= to; i++)
            {
                //string img = String.Format("ms-appdata:///local/{0}", (path + name + i));
                string img = String.Format("ms-appdata:///local/{0}", (path + name + "_"+ i + sfx));
                images.Add( img );
            }

            SetImageSource( images[index] );

            if (bAuto) {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(dellay);
                timer.Tick += timer_Tick;
                timer.Start();
            }
        }


        private void SetImageSource(string imageName)
        {
            //string name = /*"Images/" + */imageName;
            BitmapImage bmi = new BitmapImage(new Uri(imageName, UriKind.RelativeOrAbsolute));
            imgMain.Source = bmi;
            //imgMain.Source = imageName;
        }

        void nextImage() {
            index = (index + 1) % images.Count;
            SetImageSource(images[index]);
        }

        void timer_Tick(object sender, object e)
        {
            nextImage();
        }


        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Utils.Utils.navigateTo(typeof(ItemsPage));
        }

        private void imgMain_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            if (!bAuto)
                nextImage();
        }
    }
}
