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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace LibrelioApplication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoPage : Page
    {
        public VideoPage()
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
            Uri uri = new Uri((string)e.Parameter);
	        bool bAuto = false;

            string[] _params = uri.Query.Replace("?", "").Split('&');
            //TODO case "warect"
            //
	    
            for (int i = 0; i < _params.Length; i++) {
                string[] fields = _params[i].Split('=');
                string _name = fields[0];
                string _val = fields[1];
                switch (_name) { 
                    case "waplay":
                        if (_val == "auto")
                            bAuto = true;
                        break;
                }
            }

            string path = "mov/";
            string fileName = uri.LocalPath.Replace("/", "");
            string name = fileName;

            mediaMain.AutoPlay = bAuto;
            string fullPath = String.Format("ms-appdata:///local/{0}", (path + name ));
            Uri videoUrl = new Uri(fullPath);
            mediaMain.Source = videoUrl;
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Utils.Utils.navigateTo(typeof(ItemsPage));
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediaMain.CurrentState == MediaElementState.Playing)
                mediaMain.Pause();
            else
                mediaMain.Play();
        }
        

        private void mediaMain_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            if (mediaMain.CurrentState == MediaElementState.Playing)
                mediaMain.Pause();
            else
                mediaMain.Play();
        }

        private void mediaMain_CurrentStateChanged(object sender, RoutedEventArgs e)
        {

            playButton.Content = (mediaMain.CurrentState == MediaElementState.Playing) ? "Pause" : "Play";
        }

        

    }
}
