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
using LibrelioApplication.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public delegate void BoughtEventHandler(object sender, string url);

    public sealed partial class PurchaseModule : UserControl
    {
        private LicenseInformation licenseInformation = null;
        private string product_id = "";
        private string relativePath = "";
        private Data.MagazineViewModel _item = null;

        public event BoughtEventHandler Bought;

        public PurchaseModule()
        {
            this.InitializeComponent();
        }

        public async Task Init(Data.MagazineViewModel mag)
        {
            _item = mag;
            product_id = mag.FileName.Replace(".pdf", "");
            relativePath = mag.RelativePath;
            licenseInformation = CurrentAppSimulator.LicenseInformation;

            var appListing = await CurrentAppSimulator.LoadListingInformationAsync();
            var productListings = appListing.ProductListings;
            ProductListing product = null;
            try {
                product = productListings["yearlysubscritpion"];

            } catch { }

            statusContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            buttonContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            subscribeBtn1.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try {
                        receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();
                        receipt = DownloadManager.GetProductReceiptFromAppReceipt(product.ProductId, receipt);

                    } catch { }
                    if (receipt != "")
                    {
                        await DownloadManager.StoreReceiptAsync("yearlysubscritpion", receipt);
                        var app = Application.Current as App;
                        var url = DownloadManager.GetUrl("yearlysubscritpion", receipt, relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            subscribeBtn.Content = "Subscribe to " + Application.Current.Resources["AppName"] + " for 1 year: " + product.FormattedPrice;
                            subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    else 
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl("yearlysubscritpion", relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            subscribeBtn.Content = "Subscribe to " + Application.Current.Resources["AppName"] + " for 1 year: " + product.FormattedPrice;
                            subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                }
                else
                {
                    //var productLicense1 = licenseInformation.ProductLicenses["Subscription1"];
                    //var longdateTemplate = new Windows.Globalization.DateTimeFormatting.DateTimeFormatter("longdate");
                    //var text = "Subscription1 expires on: " + longdateTemplate.Format(productLicense1.ExpirationDate);
                    //var remainingDays = (productLicense1.ExpirationDate - DateTime.Now).Days;
                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl("yearlysubscritpion", relativePath, app.ClientName, app.MagazineName);
                        if (url.Equals("NoReceipt"))
                        {
                            string receipt = "";
                            try {
                                receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();

                            }  catch { }
                            if (receipt != "")
                            {
                                await DownloadManager.StoreReceiptAsync("yearlysubscritpion", receipt);
                                url = DownloadManager.GetUrl("yearlysubscritpion", receipt, relativePath, app.ClientName, app.MagazineName);
                                if (!url.Equals("NoReceipt"))
                                {
                                    Bought(this, url);
                                    return;
                                }
                                else
                                {
                                    var messageDialog = new MessageDialog("No Receipt");
                                    var task = messageDialog.ShowAsync().AsTask();
                                }
                            }
                            else
                            {
                                var messageDialog = new MessageDialog("No Receipt");
                                var task = messageDialog.ShowAsync().AsTask();
                            }
                        }
                        else
                        {
                            Bought(this, url);
                            return;
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            try {
                product = productListings["monthlysubscription"];

            } catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try
                    {
                        receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();
                        receipt = DownloadManager.GetProductReceiptFromAppReceipt(product.ProductId, receipt);

                    }
                    catch { }
                    if (receipt != "")
                    {
                        await DownloadManager.StoreReceiptAsync("monthlysubscription", receipt);
                        var app = Application.Current as App;
                        var url = DownloadManager.GetUrl("monthlysubscription", receipt, relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            subscribeBtn.Content = "Subscribe to " + Application.Current.Resources["AppName"] + " for 1 month: " + product.FormattedPrice;
                            subscribeBtn1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    else
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl("monthlysubscription", relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            subscribeBtn.Content = "Subscribe to " + Application.Current.Resources["AppName"] + " for 1 month: " + product.FormattedPrice;
                            subscribeBtn1.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                }
                else
                {
                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl("monthlysubscription", relativePath, app.ClientName, app.MagazineName);
                        if (url.Equals("NoReceipt"))
                        {
                            string receipt = "";
                            try
                            {
                                receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();

                            }
                            catch { }
                            if (receipt != "")
                            {
                                await DownloadManager.StoreReceiptAsync("monthlysubscription", receipt);
                                url = DownloadManager.GetUrl("monthlysubscription", receipt, relativePath, app.ClientName, app.MagazineName);
                                if (!url.Equals("NoReceipt"))
                                {
                                    Bought(this, url);
                                    return;
                                }
                                else
                                {
                                    var messageDialog = new MessageDialog("No Receipt");
                                    var task = messageDialog.ShowAsync().AsTask();
                                }
                            }
                            else
                            {
                                var messageDialog = new MessageDialog("No Receipt");
                                var task = messageDialog.ShowAsync().AsTask();
                            }
                        }
                        else
                        {
                            Bought(this, url);
                            return;
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            try {
                product = productListings[product_id];

            } catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try
                    {
                        receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();
                        receipt = DownloadManager.GetProductReceiptFromAppReceipt(product.ProductId, receipt);

                    }
                    catch { }
                    if (receipt != "")
                    {
                        await DownloadManager.StoreReceiptAsync(product_id, receipt);
                        var app = Application.Current as App;
                        var url = DownloadManager.GetUrl(product_id, receipt, relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            buyMag.Content = "Buy " + mag.Title + " for: " + product.FormattedPrice;
                            buyMag.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    else
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl(product_id, relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            buyMag.Content = "Buy " + mag.Title + " for: " + product.FormattedPrice;
                            buyMag.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                }
                else
                {
                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl(product_id, relativePath, app.ClientName, app.MagazineName);
                        if (url.Equals("NoReceipt"))
                        {
                            string receipt = "";
                            try
                            {
                                receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();

                            }
                            catch { }
                            if (receipt != "")
                            {
                                await DownloadManager.StoreReceiptAsync(product_id, receipt);
                                url = DownloadManager.GetUrl(product_id, receipt, relativePath, app.ClientName, app.MagazineName);
                                if (!url.Equals("NoReceipt"))
                                {
                                    Bought(this, url);
                                    return;
                                }
                                else
                                {
                                    var messageDialog = new MessageDialog("No Receipt");
                                    var task = messageDialog.ShowAsync().AsTask();
                                }
                            }
                            else
                            {
                                var messageDialog = new MessageDialog("No Receipt");
                                var task = messageDialog.ShowAsync().AsTask();
                            }
                        }
                        else
                        {
                            Bought(this, url);
                            return;
                        }
                    }
                    else
                    {
                        var messageDialog = new MessageDialog("Purchase successfull");
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            if (product != null)
            {
                var app = Application.Current as App;
                var url = await DownloadManager.GetUrl(product_id, relativePath, app.ClientName, app.MagazineName);
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try
                    {
                        receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();
                        receipt = DownloadManager.GetProductReceiptFromAppReceipt(product.ProductId, receipt);

                    }
                    catch { }
                    if (receipt != "")
                    {
                        Bought(this, url);
                    }
                    else
                    {
                        buyMag.Content = "Buy " + mag.Title + " for: " + product.FormattedPrice;
                        buyMag.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                }
                else
                {
                    if (Bought != null)
                    {
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        if (url.Equals("NoReceipt"))
                        {
                            string receipt = "";
                            try
                            {
                                receipt = await CurrentAppSimulator.GetAppReceiptAsync().AsTask();

                            }
                            catch { }
                            if (receipt != null)
                            {
                                Bought(this, url);
                            }
                            else
                            {
                                var messageDialog = new MessageDialog("No Receipt");
                                var task = messageDialog.ShowAsync().AsTask();
                            }
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

            statusContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            buttonContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        public Data.MagazineViewModel GetCurrentItem()
        {
            return _item;
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

            if (!licenseInformation.ProductLicenses["yearlysubscritpion"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentAppSimulator.RequestProductPurchaseAsync("yearlysubscritpion", true);
                    //var b = DownloadManager.ReceiptExpired(receipt);
                    if (!licenseInformation.ProductLicenses["yearlysubscritpion"].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync("yearlysubscritpion", receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    var xml = new XmlDocument();
                    xml = await XmlDocument.LoadFromFileAsync(f);
                    var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    item.SetAttribute("ProductId", "yearlysubscritpion");
                    var date = new DateTimeOffset(DateTime.Now);
                    date = date.AddMinutes(3);
                    var str = date.ToString("u");
                    str = str.Replace(" ", "T");
                    item.SetAttribute("ExpirationDate", str);
                    receipt = xml.GetXml();
                    if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl("yearlysubscritpion", receipt, relativePath, app.ClientName, app.MagazineName));
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

        private async void subscribeBtn1_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;

            if (!licenseInformation.ProductLicenses["monthlysubscription"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentAppSimulator.RequestProductPurchaseAsync("monthlysubscription", true);
                    if (!licenseInformation.ProductLicenses["monthlysubscription"].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync("monthlysubscription", receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    var xml = new XmlDocument();
                    xml = await XmlDocument.LoadFromFileAsync(f);
                    var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    item.SetAttribute("ProductId", "monthlysubscription");
                    var date = new DateTimeOffset(DateTime.Now);
                    date = date.AddMinutes(3);
                    var str = date.ToString("u");
                    str = str.Replace(" ", "T");
                    item.SetAttribute("ExpirationDate", str);
                    receipt = xml.GetXml();
                    if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl("monthlysubscription", receipt, relativePath, app.ClientName, app.MagazineName));
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
                    var date = new DateTimeOffset(DateTime.Now);
                    date = date.AddMinutes(3);
                    var str = date.ToString("u");
                    str = str.Replace(" ", "T");
                    item.SetAttribute("ExpirationDate", str);
                    receipt = xml.GetXml();
                    if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl(product_id, receipt, relativePath, app.ClientName, app.MagazineName));
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
