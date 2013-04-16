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
using Windows.ApplicationModel.Store;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Popups;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public delegate void BoughtEventHandler(object sender, string url);

    public sealed partial class PurchaseModule : UserControl
    {
        private LicenseInformation licenseInformation = null;
        private string product_id = "";
        private string relativePath = "";
        private Button _button = null;

        public event BoughtEventHandler Bought;

        public PurchaseModule()
        {
            this.InitializeComponent();
        }

        public async Task Init(Data.MagazineViewModel mag, Button button)
        {
            _button = button;
            product_id = mag.FileName.Replace(".pdf", "");
            relativePath = mag.RelativePath;
            licenseInformation = CurrentAppSimulator.LicenseInformation;

            var appListing = await CurrentAppSimulator.LoadListingInformationAsync();
            var productListings = appListing.ProductListings;
            ProductListing product = null;
            try {
                product = productListings["Subscription"];

            } catch { }

            this.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    subscribeBtn.Content += " : " + product.FormattedPrice;
                    subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (Bought != null)
                    {
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        var url = await DownloadManager.GetUrl("Subscription", relativePath);
                        if (url.Equals("NoReceipt"))
                        {
                            var messageDialog = new MessageDialog("No Receipt");
                            var task = messageDialog.ShowAsync().AsTask();
                        }
                        else
                        {
                            Bought(this, url);
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            productListings = appListing.ProductListings;
            product = null;
            try {
                product = productListings[product_id];

            } catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    buyMag.Content += mag.Title + " : " + product.FormattedPrice;
                    buyMag.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    if (Bought != null)
                    {
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        var url = await DownloadManager.GetUrl(product_id, relativePath);
                        if (url.Equals("NoReceipt"))
                        {
                            var messageDialog = new MessageDialog("No Receipt");
                            var task = messageDialog.ShowAsync().AsTask();
                        }
                        else
                        {
                            Bought(this, url);
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }
        }

        public Button GetCurrentButton()
        {
            return _button;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            subscribeBtn.Content = "Subscribe";
            subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            buyMag.Content = "";
            buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void subscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;

            if (!licenseInformation.ProductLicenses["Subscription"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentAppSimulator.RequestProductPurchaseAsync("Subscription", true);
                    if (!licenseInformation.ProductLicenses["Subscription"].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync("Subscription", receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    var xml = new XmlDocument();
                    xml = await XmlDocument.LoadFromFileAsync(f);
                    var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    item.SetAttribute("ProductId", "Subscription");
                    receipt = xml.GetXml();
                    // =================================================

                    buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (Bought != null)
                    {
                        Bought(this, DownloadManager.GetUrl("Subscription", receipt, relativePath));
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                    var messageDialog = new MessageDialog("Unexpected error");
                    var task = messageDialog.ShowAsync().AsTask();
                }
            }
            else
            {
                var messageDialog = new MessageDialog("You are already subscribed");
                var task = messageDialog.ShowAsync().AsTask();
            }
        }

        private async void buyMag_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;

            if (!licenseInformation.ProductLicenses[product_id].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentAppSimulator.RequestProductPurchaseAsync(product_id, true);
                    if (!licenseInformation.ProductLicenses[product_id].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync(product_id, receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    var xml = new XmlDocument();
                    xml = await XmlDocument.LoadFromFileAsync(f);
                    var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    item.SetAttribute("ProductId", product_id);
                    receipt = xml.GetXml();
                    // =================================================

                    buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (Bought != null)
                    {
                        Bought(this, DownloadManager.GetUrl(product_id, receipt, relativePath));
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                    var messageDialog = new MessageDialog("Unexpected error");
                    var task = messageDialog.ShowAsync().AsTask();
                }
            }
            else
            {
                var messageDialog = new MessageDialog("You already purchased this app");
                var task = messageDialog.ShowAsync().AsTask();
            }
        }
    }
}
