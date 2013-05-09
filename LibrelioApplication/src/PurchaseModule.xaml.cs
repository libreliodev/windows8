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
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace LibrelioApplication
{
    public delegate void BoughtEventHandler(object sender, string url);
    public delegate void MagazineEventHandler(object sender, Data.MagazineViewModel item);

    public sealed partial class PurchaseModule : UserControl
    {
        private LicenseInformation licenseInformation = null;
        private string product_id = "";
        private string relativePath = "";
        private Data.MagazineViewModel _item = null;

        public event BoughtEventHandler Bought;
        public event MagazineEventHandler GetSample;
        public event MagazineEventHandler Open;
        public event MagazineEventHandler Delete;

        public PurchaseModule()
        {
            this.InitializeComponent();
        }

        public async Task Init(Data.MagazineViewModel mag, bool local = false, bool intern = false)
        {
            bool needToUpdateImage = false;
            if (mag.Image != null)
            {
                thumbnail.Source = mag.Image;
            }
            else
            {
                needToUpdateImage = true;
            }

            title.Text = mag.Title;
            subtitle.Text = mag.Subtitle;
            var loader = new ResourceLoader();

            if (local)
            {
                _item = mag;

                noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                noOptions.Text = loader.GetString("no_options");
                subscribeBtnContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                subscribeBtn1Container.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                getSampleContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                open.Text = loader.GetString("read");
                openContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                delete.Text = loader.GetString("delete");
                deleteContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (_item.IsSampleDownloaded)
                    subtitle.Text += " SAMPLE";

                this.Visibility = Windows.UI.Xaml.Visibility.Visible;

                return;
            }

            if (mag.IsSampleDownloaded)
            {
                getSample.Text = loader.GetString("open_sample");
                getSampleButton.Content = "\xe16f";
            }
            else
            {
                getSample.Text = loader.GetString("download_sample");
                getSampleButton.Content = "\xe118";
            }

            _item = mag;
            product_id = mag.FileName.Replace("_.pdf", "");
            relativePath = mag.RelativePath;
            licenseInformation = CurrentApp.LicenseInformation;
            IReadOnlyDictionary<string, ProductListing> productListings = null;

            try
            {
                var appListing = await CurrentApp.LoadListingInformationAsync();
                productListings = appListing.ProductListings;
            }
            catch
            {
                subscribeBtnContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                subscribeBtn1Container.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                if (!intern)
                    getSampleContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                else
                    getSampleContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                openContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                deleteContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                noOptions.Visibility = Windows.UI.Xaml.Visibility.Visible;
                noOptions.Text = loader.GetString("no_options");
                this.Visibility = Windows.UI.Xaml.Visibility.Visible;

                if (needToUpdateImage)
                {
                    var task = LoadImage(mag);
                }

                return;
            }

            ProductListing product = null;
            
            //statusContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            //buttonContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            subscribeBtnContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            subscribeBtn1Container.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            if (!intern)
                getSampleContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                getSampleContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            openContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            deleteContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            this.Visibility = Windows.UI.Xaml.Visibility.Visible;

            try
            {
                product = productListings[product_id];

            }
            catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try
                    {
                        receipt = await CurrentApp.GetAppReceiptAsync().AsTask();
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
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            buyMag.Text = loader.GetString("buy_number") + " " + product.FormattedPrice;
                            buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            buyMag.Text = loader.GetString("buy_number") + " " + product.FormattedPrice;
                            buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                                receipt = await CurrentApp.GetAppReceiptAsync().AsTask();

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
                        var messageDialog = new MessageDialog(loader.GetString("purchase_successfull"));
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            product = null;

            try {

                product = productListings["yearlysubscription"];

            } catch { }

            if (product != null)
            {
                if (!licenseInformation.ProductLicenses[product.ProductId].IsActive)
                {
                    string receipt = "";
                    try {
                        receipt = await CurrentApp.GetAppReceiptAsync().AsTask();
                        receipt = DownloadManager.GetProductReceiptFromAppReceipt(product.ProductId, receipt);

                    } catch { }
                    if (receipt != "")
                    {
                        await DownloadManager.StoreReceiptAsync("yearlysubscription", receipt);
                        var app = Application.Current as App;
                        var url = DownloadManager.GetUrl("yearlysubscription", receipt, relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            subscribeBtn.Text = loader.GetString("subscribe_one_year") + " " + product.FormattedPrice;
                            subscribeBtnContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    else 
                    {
                        var app = Application.Current as App;
                        var url = await DownloadManager.GetUrl("yearlysubscription", relativePath, app.ClientName, app.MagazineName);
                        if (!url.Equals("NoReceipt"))
                        {
                            Bought(this, url);
                            return;
                        }
                        else
                        {
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            subscribeBtn.Text = loader.GetString("subscribe_one_year") + " " + product.FormattedPrice;
                            subscribeBtnContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                        var url = await DownloadManager.GetUrl("yearlysubscription", relativePath, app.ClientName, app.MagazineName);
                        if (url.Equals("NoReceipt"))
                        {
                            string receipt = "";
                            try {
                                receipt = await CurrentApp.GetAppReceiptAsync().AsTask();

                            }  catch { }
                            if (receipt != "")
                            {
                                await DownloadManager.StoreReceiptAsync("yearlysubscription", receipt);
                                url = DownloadManager.GetUrl("yearlysubscription", receipt, relativePath, app.ClientName, app.MagazineName);
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
                        var messageDialog = new MessageDialog(loader.GetString("purchase_successfull"));
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            product = null;

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
                        receipt = await CurrentApp.GetAppReceiptAsync().AsTask();
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
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            subscribeBtn1.Text = loader.GetString("subscribe_one_month") + " " + product.FormattedPrice;
                            subscribeBtn1Container.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                            noOptions.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                            subscribeBtn1.Text = loader.GetString("subscribe_one_month") + " " + product.FormattedPrice;
                            subscribeBtn1Container.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
                                receipt = await CurrentApp.GetAppReceiptAsync().AsTask();

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
                        var messageDialog = new MessageDialog(loader.GetString("purchase_successfull"));
                        var task = messageDialog.ShowAsync().AsTask();
                    }
                }
            }

            if (buyMagContainer.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
            {
                if (subscribeBtnContainer.Visibility == Windows.UI.Xaml.Visibility.Visible)
                {
                    //subscribeBtnContainer.Margin = new Thickness(0, 22, 0, 10);
                }
                else if (subscribeBtn1Container.Visibility == Windows.UI.Xaml.Visibility.Visible)
                {
                    //subscribeBtn1.Margin = new Thickness(0, 22, 0, 10);
                }
                else
                {
                    noOptions.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    noOptions.Text = loader.GetString("no_options");
                }
            }

            if (needToUpdateImage)
            {
                await LoadImage(mag);
            }

            //statusContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //buttonContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async Task LoadImage(Data.MagazineViewModel mag)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(mag.PngPath);
                var image = new BitmapImage();
                await image.SetSourceAsync(await file.OpenReadAsync());
                mag.Image = image;
                thumbnail.Source = mag.Image;
            }
            catch { }
        }

        public Data.MagazineViewModel GetCurrentItem()
        {
            return _item;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //subscribeBtn.Content = "Subscribe";
            //subscribeBtn.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //buyMag.Content = "";
            //buyMag.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private async void subscribeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;

            var loader = new ResourceLoader();

            if (!licenseInformation.ProductLicenses["yearlysubscription"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentApp.RequestProductPurchaseAsync("yearlysubscription", true);
                    //var b = DownloadManager.ReceiptExpired(receipt);
                    if (!licenseInformation.ProductLicenses["yearlysubscription"].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync("yearlysubscription", receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    //var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    //var xml = new XmlDocument();
                    //xml = await XmlDocument.LoadFromFileAsync(f);
                    //var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    //item.SetAttribute("ProductId", "yearlysubscription");
                    //var date = new DateTimeOffset(DateTime.Now);
                    //date = date.AddMinutes(3);
                    //var str = date.ToString("u");
                    //str = str.Replace(" ", "T");
                    //item.SetAttribute("ExpirationDate", str);
                    //receipt = xml.GetXml();
                    //if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl("yearlysubscription", receipt, relativePath, app.ClientName, app.MagazineName));
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog(loader.GetString(loader.GetString("purchase_successfull")));
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
                var messageDialog = new MessageDialog(loader.GetString("already_subscribed"));
                var task = messageDialog.ShowAsync().AsTask();
            }
        }

        private async void subscribeBtn1_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;
            var loader = new ResourceLoader();

            if (!licenseInformation.ProductLicenses["monthlysubscription"].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentApp.RequestProductPurchaseAsync("monthlysubscription", true);
                    if (!licenseInformation.ProductLicenses["monthlysubscription"].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync("monthlysubscription", receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    //var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    //var xml = new XmlDocument();
                    //xml = await XmlDocument.LoadFromFileAsync(f);
                    //var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    //item.SetAttribute("ProductId", "monthlysubscription");
                    //var date = new DateTimeOffset(DateTime.Now);
                    //date = date.AddMinutes(3);
                    //var str = date.ToString("u");
                    //str = str.Replace(" ", "T");
                    //item.SetAttribute("ExpirationDate", str);
                    //receipt = xml.GetXml();
                    //if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl("monthlysubscription", receipt, relativePath, app.ClientName, app.MagazineName));
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog(loader.GetString("purchase_successfull"));
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
                var messageDialog = new MessageDialog(loader.GetString("already_subscribed"));
                var task = messageDialog.ShowAsync().AsTask();
            }
        }

        private async void buyMag_Click(object sender, RoutedEventArgs e)
        {
            if (licenseInformation == null) return;
            var loader = new ResourceLoader();

            if (!licenseInformation.ProductLicenses[product_id].IsActive)
            {
                try
                {
                    // The customer doesn't own this feature, so 
                    // show the purchase dialog.

                    var receipt = await CurrentApp.RequestProductPurchaseAsync(product_id, true);
                    if (!licenseInformation.ProductLicenses[product_id].IsActive || receipt == "") return;
                    await DownloadManager.StoreReceiptAsync(product_id, receipt);
                    // the in-app purchase was successful

                    // TEST ONLY
                    // =================================================
                    //var f = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test\receipt.pmd");
                    //var xml = new XmlDocument();
                    //xml = await XmlDocument.LoadFromFileAsync(f);
                    //var item = xml.GetElementsByTagName("ProductReceipt")[0] as XmlElement;
                    //item.SetAttribute("ProductId", product_id);
                    //var date = new DateTimeOffset(DateTime.Now);
                    //date = date.AddMinutes(3);
                    //var str = date.ToString("u");
                    //str = str.Replace(" ", "T");
                    //item.SetAttribute("ExpirationDate", str);
                    //receipt = xml.GetXml();
                    //if (DownloadManager.ReceiptExpired(receipt)) return;
                    // =================================================

                    buyMagContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    if (Bought != null)
                    {
                        var app = Application.Current as App;
                        Bought(this, DownloadManager.GetUrl(product_id, receipt, relativePath, app.ClientName, app.MagazineName));
                        this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                    else
                    {
                        var messageDialog = new MessageDialog(loader.GetString("purchase_successfull"));
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
                var messageDialog = new MessageDialog(loader.GetString("already_subscribed"));
                var task = messageDialog.ShowAsync().AsTask();
            }
        }

        private void getSample_Click(object sender, RoutedEventArgs e)
        {
            GetSample(this, _item);
            this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void open_Click(object sender, RoutedEventArgs e)
        {
            Open(this, _item);
            this.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            var loader = new ResourceLoader();
            openContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            deleteContainer.Margin = new Thickness(0, 25, 0, 0);
            delete.Text = loader.GetString("deleting");
            Delete(this, _item);
        }

        private void Grid_PointerReleased_1(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Grid_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Grid_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
