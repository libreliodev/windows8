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
    public delegate void CustomClickedEventHandler(string folderUrl, string url);
    public delegate void CustomInternalClickedEventHandler(int pageNum);
    public delegate void CustomBuyClickedEventHandler(string url);

    public sealed partial class PageButton : UserControl
    {
        private string _folderUrl;
        private string _url;

        bool isPressed = false;

        bool _isInternalLink = false;
        bool _isBuyLink = false;
        int _pageNum = -1;

        public event CustomClickedEventHandler Clicked;
        public event CustomInternalClickedEventHandler InternalClicked;
        public event CustomBuyClickedEventHandler BuyClicked;

        public PageButton()
        {
            this.InitializeComponent();
        }

        public void SetRect(Rect rect, string folderUrl, string url, float offset)
        {
            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
            this.Width = rect.Width * offset;
            this.Height = rect.Height * offset;

            _url = url;
            _folderUrl = folderUrl;
        }

        public void SetRect(Rect rect, int PageNum, float offset)
        {
            _isInternalLink = true;

            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
            this.Width = rect.Width * offset;
            this.Height = rect.Height * offset;

            _pageNum = PageNum;
        }

        public void SetRect(Rect rect, int PageNum, string url, float offset)
        {
            _isBuyLink = true;

            this.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
            this.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            this.Margin = new Thickness(rect.Left * offset, (rect.Top + 1.5) * offset, 0, 0);
            this.Width = rect.Width * offset;
            this.Height = rect.Height * offset;

            _pageNum = PageNum;
            _url = url;
        }

        //private void frame_PointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    isPressed = true;
        //}

        //private void frame_PointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    if (isPressed && !_isInternalLink)
        //    {
        //        Clicked(_folderUrl, _url);
        //    }
        //    else if (_isInternalLink)
        //    {
        //        InternalClicked(_pageNum);
        //    }
        //    isPressed = false;
        //}

        //private void frame_PointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    isPressed = false;
        //}

        private void frame_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            if (!_isInternalLink && !_isBuyLink)
            {
                Clicked(_folderUrl, _url);
            }
            else if (_isInternalLink)
            {
                InternalClicked(_pageNum);
            }
            else if (_isBuyLink)
            {
                BuyClicked(_url);
            }
        }
    }
}
