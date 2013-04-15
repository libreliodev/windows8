using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using LibrelioApplication.Utils;
using Windows.ApplicationModel.Resources;
using System.Reflection;
using Windows.Storage;
using System.Threading.Tasks;
using System.Diagnostics;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace LibrelioApplication.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class MagazineDataCommon : LibrelioApplication.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public MagazineDataCommon(String uniqueId, String title, String subtitle)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            //this._description = description;
            //this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        //private string _description = string.Empty;
        //public string Description
        //{
        //    get { return this._description; }
        //    set { this.SetProperty(ref this._description, value); }
        //}

        //private ImageSource _image = null;
        //private String _imagePath = null;
        //public ImageSource Image
        //{
        //    get
        //    {
        //        if (this._image == null && this._imagePath != null)
        //        {
        //            this._image = new BitmapImage(new Uri(MagazineDataCommon._baseUri, this._imagePath));
        //        }
        //        return this._image;
        //    }

        //    set
        //    {
        //        this._imagePath = null;
        //        this.SetProperty(ref this._image, value);
        //    }
        //}

        //public void SetImage(String path)
        //{
        //    this._image = null;
        //    this._imagePath = path;
        //    this.OnPropertyChanged("Image");
        //}

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class MagazineModel : LibrelioApplication.Common.BindableBase
    {
        private const String TAG = "MagazineModel";
        private const String COMPLETE_FILE = ".complete";
        private const String COMPLETE_SAMPLE_FILE = ".sample_complete";
        private const String PAYED_FILE = ".payed";

        private const String FILE_NAME_KEY = "FileName";
        private const String TITLE_KEY = "Title";
        private const String SUBTITLE_KEY = "Subtitle";


        public String Title { get; set; }
        public String Subtitle { get; set; }
        public String fileName { get; set; }
        public String pdfPath { get; set; }
        public String pngPath { get; set; }
        public String samplePath { get; set; }
        public String pdfUrl { get; set; }
        public String pngUrl { get; set; }
        public String sampleUrl { get; set; }
        public bool isPaid { get; set; }
        public bool isDowloaded { get; set; }
        public bool isSampleDowloaded = false;
        public String assetsDir { get; set; }
        public String downloadDate { get; set; }

        public MagazineModel(Dictionary<string, dynamic> dict)
        {
            fileName = (string)dict[FILE_NAME_KEY];
            Title = (string)dict[TITLE_KEY];
            Subtitle = (string)dict[SUBTITLE_KEY];
            valuesInit(fileName);
        }

        public MagazineModel(String title, String subtitle, String fileName)
        {
            this.fileName = fileName;
            this.Title = title;
            this.Subtitle = subtitle;

            valuesInit(fileName);
        }

        async public void valuesInit(String fileName)
        {
            if ((fileName == "") || (fileName == null)) {
                return ;
            }

            isPaid = fileName.Contains("_.");
            int startNameIndex = fileName.IndexOf("/") + 1;
            string appDataPath = "";
            StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            appDataPath = folder.Path + "/";


            //String png = appDataPath + fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
            String png = fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
            string baseURL = appDataPath + "pdf/";
            pdfUrl = baseURL + fileName;
            pdfPath = getMagazineDir() + fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
            if (isPaid)
            {
                pngUrl = pdfUrl.Replace("_.pdf", ".png");
                pngPath = png.Replace("_.pdf", ".png");
                sampleUrl = pdfUrl.Replace("_.", ".");
                samplePath = pdfPath.Replace("_.", ".");
                isSampleDowloaded = await Utils.Utils.fileExistAsync(folder, getMagazineDir() + COMPLETE_SAMPLE_FILE);
            }
            else
            {
                pngUrl = pdfUrl.Replace(".pdf", ".png");
                pngPath = png.Replace(".pdf", ".png");
            }
            isDowloaded = await Utils.Utils.fileExistAsync(folder, getMagazineDir() + COMPLETE_FILE);

            assetsDir = getMagazineDir();
        }

        public MagazineModel(LibrelioLocalUrl url)
        {
            this.fileName = url.FullName;
            this.Title = url.Title;
            this.Subtitle = url.Subtitle;

            valuesInit(url);
        }

        public void valuesInit(LibrelioLocalUrl url)
        {
            if ((url.FolderPath == "") || (url.FolderPath == "ND")) {
                return;
            }

            isPaid = fileName.Contains("_.");

            pdfPath = url.FolderPath + url.FullName;
            if (isPaid)
            {
                pngPath = pdfPath.Replace("_.pdf", ".png");
                samplePath = pdfPath.Replace("_.", ".");
                isSampleDowloaded = url.IsDownloaded;
            }
            else
            {
                pngPath = pdfPath.Replace(".pdf", ".png");
            }
            isDowloaded = url.IsDownloaded;
        }

        public String getMagazineDir()
        {
            if( fileName == "" ) return "";
            int finishNameIndex = fileName.IndexOf("/");
            if (finishNameIndex <= 0) return "";
            return getStoragePath() + fileName.Substring(0, finishNameIndex) + "/";
        }

        public String getStoragePath() {
            return "";//TODODEBUG? Possible will be empty so as we used StorageFolder - not full path!
        }

        public MagazineModel(LibrelioUrl url)
        {
            this.fileName = url.FullName;
            this.Title = url.Title;
            this.Subtitle = url.Subtitle;

            valuesInit(url);
        }

        public void valuesInit(LibrelioUrl url)
        {
            isPaid = fileName.Contains("_.");

            pdfUrl = url.AbsoluteUrl;
            if (isPaid)
            {
                pngUrl = pdfUrl.Replace("_.pdf", ".png");
                //samplePath = pdfPath.Replace("_.", ".");
                isSampleDowloaded = false;
            }
            else
            {
                pngUrl = pdfUrl.Replace(".pdf", ".png");
            }
            isDowloaded = false;
        }

        /*
            public MagazineModel(Cursor cursor, Context context) {
                int titleColumnId = cursor.getColumnIndex(Magazines.FIELD_TITLE);
                int subitleColumnId = cursor.getColumnIndex(Magazines.FIELD_SUBTITLE);
                int fileNameColumnId = cursor.getColumnIndex(Magazines.FIELD_FILE_NAME);
                int dateColumnId = cursor.getColumnIndex(Magazines.FIELD_DOWNLOAD_DATE);

                this.fileName = cursor.getString(fileNameColumnId);
                this.title = cursor.getString(titleColumnId);
                this.subtitle = cursor.getString(subitleColumnId);
                this.downloadDate = cursor.getString(dateColumnId);
                this.context = context;

                valuesInit(fileName);
            }


            public static String getAssetsBaseURL(String fileName){
                int finishNameIndex = fileName.indexOf("/");
                return LibrelioApplication.BASE_URL+fileName.substring(0,finishNameIndex)+"/";
            }

            public void makeMagazineDir(){
                File assets = new File(getMagazineDir());
                if(!assets.exists()){
                    assets.mkdirs();
                }
            }

            public void clearMagazineDir(){
                File dir = new File(getMagazineDir());
                if (dir.exists()) {
                    if (dir.isDirectory()) {
                        for (File c : dir.listFiles()) c.delete();
                    }
                    dir.delete();
                }
            }

            public void delete(){
                Log.d(TAG,"Deleting magazine has been initiated");
                clearMagazineDir();
                Intent intentInvalidate = new Intent(MainMagazineActivity.BROADCAST_ACTION_IVALIDATE);
                context.sendBroadcast(intentInvalidate);
            }

            public synchronized void saveInBase() {
                SQLiteDatabase db;
                DataBaseHelper dbhelp = new DataBaseHelper(context);
                db = dbhelp.getWritableDatabase();
                ContentValues cv = new ContentValues();
                cv.put(Magazines.FIELD_FILE_NAME, fileName);
                cv.put(Magazines.FIELD_DOWNLOAD_DATE, downloadDate);
                cv.put(Magazines.FIELD_TITLE, title);
                cv.put(Magazines.FIELD_SUBTITLE, subtitle);
                db.insert(Magazines.TABLE_NAME, null, cv);
                db.close();
            }



            public void makeCompleteFile(boolean isSample){
                String completeModificator = COMPLETE_FILE;
                if(isSample){
                    completeModificator = COMPLETE_SAMPLE_FILE;
                }
                File file = new File(getMagazineDir()+completeModificator);
                boolean create = false;
                try {
                    create = file.createNewFile();
                } catch (IOException e) {
                    Log.d(TAG,"Problem with create "+completeModificator+", createNewFile() return "+create,e);
                }
            }
            public void makePayedFile(){
                File file = new File(getMagazineDir()+PAYED_FILE);
                boolean create = false;
                if(file.exists()){
                    return;
                }
                try {
                    create = file.createNewFile();
                } catch (IOException e) {
                    Log.d(TAG,"Problem with create "+PAYED_FILE+", createNewFile() return "+create,e);
                }
            }


        */



    }

    public class MagazineViewModel : MagazineDataCommon
    {
        public const string TAG_READ  = "READ";
        public const string TAG_DEL  = "DEL";
        public const string TAG_SAMPLE  = "SAMPLE";
        public const string TAG_DOWNLOAD  = "DOWNLOAD";

        private MagazineDataGroup _group;
        public MagazineDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }

        //public String Title { get; set; }
        //public String Subtitle { get; set; }
        public String Thumbnail { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsPaid { get; set; }
        public bool SecondButtonVisible { get; set; }
        public String FileName { get; set; }
        public ImageSource Image { get; set; }
        public String DownloadOrReadButton { get; set; }
        public String SampleOrDeleteButton { get; set; }
        public String Button1Tag { get; set; }
        public String Button2Tag { get; set; }
        public String MagazineTag { get; set; }
        public MagazineViewModel(String uniqueId, String title, String subtitle, MagazineDataGroup group, string tumb, string b1, string b2)
            : base(uniqueId, title, subtitle)
        {
            Title = title;
            Subtitle = subtitle;
            Thumbnail=tumb;
            DownloadOrReadButton=b1;
            SampleOrDeleteButton=b2;
            Button1Tag = "t1";
            Button2Tag = "t2";
            MagazineTag = "m1";

            this._group = group;
        }

        public MagazineViewModel(String uniqueId, String title, String subtitle, ImageSource img, MagazineDataGroup group, MagazineModel m)
            : base(uniqueId, title, subtitle)
        {
            Title = m.Title;
            Subtitle = m.Subtitle;
            IsDownloaded = m.isDowloaded;
            IsPaid = m.isPaid;
            FileName = m.fileName;
            SecondButtonVisible = true;
            if (img != null)
            {
                Image = img;
            }
            else 
            {
                Image = new BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", m.pngPath)));
            }
            //Thumbnail = String.Format("ms-appdata:///local/{0}", m.pngPath);
            var resourceLoader = new ResourceLoader();
            MagazineTag = m.fileName;

            if (m.isDowloaded)
            {
                DownloadOrReadButton = resourceLoader.GetString("read");
                SampleOrDeleteButton = resourceLoader.GetString("delete");
                Button1Tag = TAG_READ;
                Button2Tag = TAG_DEL;
            }
            else {
                DownloadOrReadButton = resourceLoader.GetString("download");
                SampleOrDeleteButton = resourceLoader.GetString("sample");
                if (!m.isDowloaded && !m.isPaid)
                {
                    SecondButtonVisible = false;
                }
                Button1Tag = TAG_DOWNLOAD;
                Button2Tag = TAG_SAMPLE;
            }

            this._group = group;
        }

    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class MagazineDataGroup : MagazineDataCommon
    {
        public MagazineDataGroup(String uniqueId, String title, String subtitle)
            : base(uniqueId, title, subtitle)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 22)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 22)
                        {
                            TopItems.RemoveAt(22);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 22 && e.NewStartingIndex < 22)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 22)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[21]);
                    }
                    else if (e.NewStartingIndex < 22)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 22)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 22)
                        {
                            TopItems.Add(Items[21]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 22)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 22)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<MagazineViewModel> _items = new ObservableCollection<MagazineViewModel>();
        public ObservableCollection<MagazineViewModel> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<MagazineViewModel> _topItem = new ObservableCollection<MagazineViewModel>();
        public ObservableCollection<MagazineViewModel> TopItems
        {
            get { return this._topItem; }
            set { _topItem = value; }
        }
    }

    /// <summary>
    /// Creates a collection of Magazines. Hardcoded (for design time) and from plist (runtime).
    /// </summary>
    public sealed class MagazineDataSource
    {
        private static MagazineDataSource _sampleDataSource = new MagazineDataSource();

        private ObservableCollection<MagazineDataGroup> _allGroups = new ObservableCollection<MagazineDataGroup>();
        public ObservableCollection<MagazineDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<MagazineDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static MagazineDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static void RemoveGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) _sampleDataSource.AllGroups.Remove(matches.First());
        }

        public static MagazineViewModel GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public MagazineDataSource() {
            //PList list = new PList("Assets/data/magazines.plist");
            ////TODODEBUG try
            //{
            //    List<dynamic> arr = list[""];
            //    var group = new MagazineDataGroup("My Magazines", "My Magazines", "");
            //    for (int i = 0; i < arr.Count; i++)
            //    {
            //        var m = new MagazineModel((Dictionary<string, dynamic>)arr[i]);
            //        group.Items.Add(new MagazineViewModel("", "", "", group, m));
            //    }
            //    this.AllGroups.Add(group);

            //    group = new MagazineDataGroup("All Magazines", "All Magazines", "");
            //    for (int i = 0; i < arr.Count; i++)
            //    {
            //        var m = new MagazineModel((Dictionary<string, dynamic>)arr[i]);
            //        group.Items.Add(new MagazineViewModel("", "", "", group, m));
            //    }
            //    this.AllGroups.Add(group);

            //}
            //catch (Exception e)
            //{
            //    throw new Exception("Failed to load list");
            //}
        }

        public static async Task<MagazineManager> LoadMagazinesAsync()
        {
            //if (_sampleDataSource.AllGroups.Count > 0) return;

            var manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/niveales/wind/", "Magazines");

            await manager.LoadPLISTAsync();
            await manager.LoadLocalMagazineList();

            bool newGroup = false;
            MagazineDataGroup group = null;
            if (GetGroup("My Magazines") == null) {

                group = new MagazineDataGroup("My Magazines", "My Magazines", "");
                newGroup = true;

            } else {

                group = GetGroup("My Magazines");
            }
            for (int i = 0; i < manager.MagazineLocalUrl.Count; i++) {

                if (manager.MagazineLocalUrl[i].IsDownloaded) {

                    var m = new MagazineModel(manager.MagazineLocalUrl[i]);
                    if (GetItem(m.Title + m.Subtitle) != null) continue;
                    BitmapImage image = null;
                    try {

                        var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
                        image = new BitmapImage();
                        await image.SetSourceAsync(await file.OpenReadAsync());

                    } catch { }
                    group.Items.Add(new MagazineViewModel(m.Title + m.Subtitle, m.Title, m.Subtitle, image, group, m));
                }
            }
            if (newGroup && group.Items.Count > 0) {

                _sampleDataSource.AllGroups.Insert(0, group);
                newGroup = false;
            }

            newGroup = false;

            if (GetGroup("All Magazines") == null) {

                group = new MagazineDataGroup("All Magazines", "All Magazines", "");
                newGroup = true;

            } else {

                group = GetGroup("All Magazines");
            }

            for (int i = 0; i < manager.MagazineUrl.Count; i++) {

                var localUrl = manager.FindInMetadata(manager.MagazineUrl[i]);
                MagazineModel m = null;
                if (localUrl != null && localUrl.IsDownloaded)
                    m = new MagazineModel(localUrl);
                else
                    m = new MagazineModel(manager.MagazineUrl[i]);
                    

                if (GetItem(m.Title + m.Subtitle + "1") != null) continue;
                BitmapImage image = null;
                try {

                    if (localUrl != null && localUrl.IsDownloaded) {

                        var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
                        image = new BitmapImage();
                        await image.SetSourceAsync(await file.OpenReadAsync());

                    } else {

                        image = new BitmapImage(new Uri(m.pngUrl));
                    }

                } catch { }
                var item = new MagazineViewModel(m.Title + m.Subtitle + "1", m.Title, m.Subtitle, image, group, m);
                if (localUrl != null && localUrl.IsDownloaded)
                    item.SecondButtonVisible = false;
                group.Items.Add(item);
            }
            group.TopItems = new ObservableCollection<MagazineViewModel>(group.Items.OrderBy(item => item.Title));
            if (newGroup && group.Items.Count > 0) {

                _sampleDataSource.AllGroups.Add(group);
                newGroup = false;
            }

            //if (GetGroup("All Magazines") != null) return null;

            //PList list = new PList("Assets/data/magazines.plist");
            ////TODODEBUG try

            //    List<dynamic> arr = list[""];
            //    group = new MagazineDataGroup("All Magazines", "All Magazines", "");
            //    for (int i = 0; i < arr.Count; i++)
            //    {
            //        var m = new MagazineModel((Dictionary<string, dynamic>)arr[i]);
            //        group.Items.Add(new MagazineViewModel("", "", "", null, group, m));
            //    }
            //    _sampleDataSource.AllGroups.Add(group);

            return manager;
        }
        
        //public MagazineDataSource()
        //{
        //    _allMagazines.Add(new MagazineDataGroup("aaa", "bbb", "vccc", "buy", "sample"));
        //    _allMagazines.Add(new MagazineDataGroup("aaa1", "bbb2", "vccc", "buy", "sample"));
            //_allMagazines.Add(new MagazineDataGroup("aaa2", "bbb3", "vccc", "buy", "sample"));
            //_allMagazines.Add(new MagazineDataGroup("aaa3", "bbb4", "vccc", "buy", "sample"));
        //}
    }
}
