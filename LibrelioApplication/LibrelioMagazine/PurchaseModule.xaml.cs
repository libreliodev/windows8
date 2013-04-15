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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public sealed partial class PurchaseModule : UserControl
    {
        private LicenseInformation licenseInformation = null;
        private string product_id = "";

        public PurchaseModule()
        {
            this.InitializeComponent();
        }

        public async Task Init(Data.MagazineViewModel mag)
        {
            product_id = mag.FileName.Replace(".pdf", "");
            licenseInformation = CurrentAppSimulator.LicenseInformation;

            var appListing = await CurrentAppSimulator.LoadListingInformationAsync();
            var productListings = appListing.ProductListings;
            ProductListing product = null;
            try {
                product = productListings["Subscription"];

            } catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    subscribeBtn.Content += " : " + product.FormattedPrice;
                    subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
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
                }
            }
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
                    await DownloadManager.StoreReceiptAsync(receipt);
                    // the in-app purchase was successful
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                }
            }
            else
            {
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
                    await DownloadManager.StoreReceiptAsync(receipt);
                    // the in-app purchase was successful
                }
                catch (Exception)
                {
                    // The in-app purchase was not completed because 
                    // an error occurred.
                }
            }
            else
            {
            }
        }
    }
}
