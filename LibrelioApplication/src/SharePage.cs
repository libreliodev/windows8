
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;


namespace LibrelioApplication.Common
{
    public abstract class SharePage : LibrelioApplication.Common.LayoutAwarePage
    {
        private DataTransferManager dataTransferManager;
        private bool sharingEnabled = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var app = Application.Current as App;

            if (app.SharingTitle != "")
            {
                // Register the current page as a share source.
                this.dataTransferManager = DataTransferManager.GetForCurrentView();
                this.dataTransferManager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
                sharingEnabled = true;
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (sharingEnabled)
            {
                // Unregister the current page as a share source.
                this.dataTransferManager.DataRequested -= new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(this.OnDataRequested);
            }
            sharingEnabled = false;

            base.OnNavigatedFrom(e);
        }

        // When share is invoked (by the user or programatically) the event handler we registered will be called to populate the datapackage with the
        // data to be shared.
        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            // Call the scenario specific function to populate the datapackage with the data to be shared.
            if (GetShareContent(e.Request))
            {
                // Out of the datapackage properties, the title is required. If the scenario completed successfully, we need
                // to make sure the title is valid since the sample scenario gets the title from the user.
                if (String.IsNullOrEmpty(e.Request.Data.Properties.Title))
                {
                }
            }
        }

        // This function is implemented by each scenario to share the content specific to that scenario (text, link, image, etc.).
        protected bool GetShareContent(DataRequest request)
        {
            bool succeeded = false;

            var app = Application.Current as App;
            if (!String.IsNullOrEmpty(app.SharingTitle))
            {
                DataPackage requestData = request.Data;
                requestData.Properties.Title = app.SharingTitle;
                requestData.Properties.Description = app.SharingText; // The description is optional.
                //string htmlFormat = HtmlFormatHelper.CreateHtmlFormat(app.SharingText);
                //requestData.SetHtmlFormat(htmlFormat);
                requestData.SetUri(new Uri(app.SharingLink));
                succeeded = true;
            }
            else
            {
                request.FailWithDisplayText("nothing to share");
            }
            return succeeded;
        }
    }
}