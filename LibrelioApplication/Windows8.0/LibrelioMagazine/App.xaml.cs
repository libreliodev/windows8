using LibrelioApplication.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using System.Xml.Linq;
using Windows.Networking.BackgroundTransfer;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Windows.UI.ApplicationSettings;
using Windows.System;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;

// The Split App template is documented at http://go.microsoft.com/fwlink/?LinkId=234228

namespace LibrelioApplication
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public MagazineManager Manager;
        public List<DownloadOperation> activeDownloads;
        public bool loadedPList = false;
        public bool needToDownload = false;
        public bool needToRedirectDownload = false;
        public DownloadMagazine RedirectParam = null;
        public string ClientName = "";
        public string MagazineName = "";
        public string Color = "";
        public bool NoMagazines = false;
        public ObservableCollection<Data.MagazineViewModel> snappedCollection = null;
        public string SharingTitle = "";
        public string SharingText = "";
        public string SharingLink = "";
        public string PrivacyLink = "";

        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Debug.WriteLine("App started");
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                var licenseInformation = CurrentApp.LicenseInformation;

                //await LibrelioApplication.Utils.Utils.prepareTestData();
                SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;

                var fileHandle =
                    await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"CustomizationAssets\application_.xml");

                var xml = await XmlDocument.LoadFromFileAsync(fileHandle);
                var appName = xml.SelectSingleNode("/resources/string[@name='app_name']");
                ClientName = xml.SelectSingleNode("/resources/string[@name='client_name']").InnerText;
                MagazineName = xml.SelectSingleNode("/resources/string[@name='magazine_name']").InnerText;

                SharingTitle = xml.SelectSingleNode("/resources/string[@name='sharing_title']").InnerText;
                SharingText = xml.SelectSingleNode("/resources/string[@name='sharing_text']").InnerText;
                SharingLink = xml.SelectSingleNode("/resources/string[@name='sharing_link']").InnerText;

                PrivacyLink = xml.SelectSingleNode("/resources/string[@name='privacy_link']").InnerText;

                var node = xml.SelectSingleNode("/resources/hex[@name='background_color']");
                Color = node.InnerText;

                SetupCustomColors();

                Application.Current.Resources["AppName"] = appName.InnerText;

                Manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/" + ClientName + "/" + MagazineName + "/", "Magazines");

                await DiscoverActiveDownloadsAsync(); 
 
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(LibrelioApplication.ItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            if (!args.Request.ApplicationCommands.Any(command => command.Id.Equals("privacypolicy")))
            {
                var loader = new ResourceLoader();
                var privacy = new SettingsCommand("privacypolicy", loader.GetString("privacy_policy"), privacy_Handler);
                args.Request.ApplicationCommands.Add(privacy);
            }

        }

        private async void privacy_Handler(Windows.UI.Popups.IUICommand command)
        {
            Uri uri = new Uri(PrivacyLink);
            await Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            //var deferral = e.SuspendingOperation.GetDeferral();
            //await SuspensionManager.SaveAsync();
            //deferral.Complete();
        }

        private bool ConnectedToInternet() {

            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            if (InternetConnectionProfile == null) {

                return false;
            }

            var level = InternetConnectionProfile.GetNetworkConnectivityLevel();

            return level == NetworkConnectivityLevel.InternetAccess;
        }

        // Enumerate the downloads that were going on in the background while the app was closed. 
        private async Task DiscoverActiveDownloadsAsync() {

            if (!ConnectedToInternet()) return;

            activeDownloads = new List<DownloadOperation>();

            IReadOnlyList<DownloadOperation> downloads = null;
            try
            {
                downloads = await BackgroundDownloader.GetCurrentDownloadsAsync();
            }
            catch (Exception ex)
            {
                return;
            }

            if (downloads.Count > 0)
            {
                List<Task> tasks = new List<Task>();
                foreach (DownloadOperation download in downloads)
                {
                    // Attach progress and completion handlers. 
                    tasks.Add(HandleDownloadAsync(download, false));
                }

                // Don't await HandleDownloadAsync() in the foreach loop since we would attach to the second 
                // download only when the first one completed; attach to the third download when the second one 
                // completes etc. We want to attach to all downloads immediately. 
                // If there are actions that need to be taken once downloads complete, await tasks here, outside 
                // the loop. 
                await Task.WhenAll(tasks);
            }
        }

        private async Task HandleDownloadAsync(DownloadOperation download, bool start)
        {
            try
            {
                // Store the download so we can pause/resume. 
                activeDownloads.Add(download);

                if (start)
                {
                    // Start the download and attach a progress handler. 
                    await download.StartAsync();
                }
                else
                {
                    // The download was already running when the application started, re-attach the progress handler. 
                    await download.AttachAsync();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
                activeDownloads.Remove(download);
            }
        }

        private void SetupCustomColors()
        {
            if (this.Resources.ContainsKey("ProgressBarIndeterminateForegroundThemeBrush"))
            {
                var brush = this.Resources["ProgressBarIndeterminateForegroundThemeBrush"] as SolidColorBrush;
                brush.Color = Utils.Utils.ColorFromHex(Color).Color;
            }
        }

    }
}
