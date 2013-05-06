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
using Windows.UI.Xaml;

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
        public MagazineDataCommon(String uniqueId, String title, String subtitle)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
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

        public override string ToString()
        {
            return this.Title;
        }
    }

    public class MagazineModel : LibrelioApplication.Common.BindableBase
    {
        //private const String TAG = "MagazineModel";
        //private const String COMPLETE_FILE = ".complete";
        //private const String COMPLETE_SAMPLE_FILE = ".sample_complete";
        //private const String PAYED_FILE = ".payed";

        //private const String FILE_NAME_KEY = "FileName";
        //private const String TITLE_KEY = "Title";
        //private const String SUBTITLE_KEY = "Subtitle";


        public String Title { get; set; }
        public String Subtitle { get; set; }
        public String fileName { get; set; }
        public String relativePath { get; set; }
        public String pdfPath { get; set; }
        public String pngPath { get; set; }
        public int Index { get; set; }
        public String samplePath { get; set; }
        public String pdfUrl { get; set; }
        public String pngUrl { get; set; }
        public String sampleUrl { get; set; }
        public bool isPaid { get; set; }
        public bool isDowloaded { get; set; }
        public bool isSampleDowloaded = false;
        public String assetsDir { get; set; }
        //public String downloadDate { get; set; }

        //public MagazineModel(Dictionary<string, dynamic> dict)
        //{
        //    fileName = (string)dict[FILE_NAME_KEY];
        //    Title = (string)dict[TITLE_KEY];
        //    Subtitle = (string)dict[SUBTITLE_KEY];
        //    valuesInit(fileName);
        //}

        //public MagazineModel(String title, String subtitle, String fileName)
        //{
        //    this.fileName = fileName;
        //    this.Title = title;
        //    this.Subtitle = subtitle;

        //    valuesInit(fileName);
        //}

        //async public void valuesInit(String fileName)
        //{
        //    if ((fileName == "") || (fileName == null)) {
        //        return ;
        //    }

        //    isPaid = fileName.Contains("_.");
        //    int startNameIndex = fileName.IndexOf("/") + 1;
        //    string appDataPath = "";
        //    StorageFolder folder = Windows.Storage.ApplicationData.Current.LocalFolder;
        //    appDataPath = folder.Path + "/";


        //    //String png = appDataPath + fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
        //    String png = fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
        //    string baseURL = appDataPath + "pdf/";
        //    pdfUrl = baseURL + fileName;
        //    pdfPath = getMagazineDir() + fileName.Substring(startNameIndex, fileName.Length - startNameIndex);
        //    if (isPaid)
        //    {
        //        pngUrl = pdfUrl.Replace("_.pdf", ".png");
        //        pngPath = png.Replace("_.pdf", ".png");
        //        sampleUrl = pdfUrl.Replace("_.", ".");
        //        samplePath = pdfPath.Replace("_.", ".");
        //        isSampleDowloaded = await Utils.Utils.fileExistAsync(folder, getMagazineDir() + COMPLETE_SAMPLE_FILE);
        //    }
        //    else
        //    {
        //        pngUrl = pdfUrl.Replace(".pdf", ".png");
        //        pngPath = png.Replace(".pdf", ".png");
        //    }
        //    isDowloaded = await Utils.Utils.fileExistAsync(folder, getMagazineDir() + COMPLETE_FILE);

        //    assetsDir = getMagazineDir();
        //}

        public MagazineModel(LibrelioLocalUrl url, int index)
        {
            this.fileName = url.FullName;
            this.Title = url.Title;
            this.Subtitle = url.Subtitle;
            this.Index = index;

            valuesInit(url);
        }

        public void valuesInit(LibrelioLocalUrl url)
        {
            relativePath = url.RelativePath;

            isPaid = fileName.Contains("_.");

            pdfUrl = url.Url;
            isDowloaded = false;

            pdfPath = url.FolderPath + url.FullName;
            if (isPaid)
            {
                pngUrl = pdfUrl.Replace("_.pdf", ".png");
                pngPath = url.FullName.Replace("_.pdf", ".png");
                sampleUrl = pdfUrl.Replace("_.", ".");
                samplePath = pdfPath.Replace("_.", ".");
                isSampleDowloaded = url.IsSampleDownloaded;
            }
            else
            {
                pngUrl = pdfUrl.Replace(".pdf", ".png");
                pngPath = url.FullName.Replace(".pdf", ".png");
            }
            assetsDir = url.FolderPath;
            isDowloaded = url.IsDownloaded;
        }

        //public String getMagazineDir()
        //{
        //    if( fileName == "" ) return "";
        //    int finishNameIndex = fileName.IndexOf("/");
        //    if (finishNameIndex <= 0) return "";
        //    return getStoragePath() + fileName.Substring(0, finishNameIndex) + "/";
        //}

        //public String getStoragePath() {
        //    return "";//TODODEBUG? Possible will be empty so as we used StorageFolder - not full path!
        //}

        //public MagazineModel(LibrelioUrl url)
        //{
        //    this.fileName = url.FullName;
        //    this.Title = url.Title;
        //    this.Subtitle = url.Subtitle;
        //    this.Index = url.Index;

        //    valuesInit(url);
        //}

        //public void valuesInit(LibrelioUrl url)
        //{
        //    relativePath = url.RelativeUrl;

        //    isPaid = fileName.Contains("_.");

        //    pdfUrl = url.AbsoluteUrl;
        //    if (isPaid)
        //    {
        //        pngUrl = pdfUrl.Replace("_.pdf", ".png");
        //        //samplePath = pdfPath.Replace("_.", ".");
        //        isSampleDowloaded = false;
        //    }
        //    else
        //    {
        //        pngUrl = pdfUrl.Replace(".pdf", ".png");
        //    }
        //    isDowloaded = false;
        //}

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
        private bool _isDownloaded = false;
        private bool _isOpening = false;
        private string _downloadOrReadButton = "";
        private string _sampleOrDeleteButton = "";
        private ImageSource _image = null;
        private int _rowSpan = 1;
        private int _colSpan = 1;
        private int _width = 180;
        private int _height = 238;
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

        public int RowSpan
        {
            get { return _rowSpan; }
            set
            {
                _rowSpan = value;
                OnPropertyChanged("RowSpan");
            }
        }

        public int ColSpan
        {
            get { return _colSpan; }
            set
            {
                _colSpan = value;
                OnPropertyChanged("ColSpan");
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                _width = value;
                OnPropertyChanged("Width");
            }
        }

        public int Height
        {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        public String Thumbnail { get; set; }
        public String PngPath { get; set; }
        public String PngUrl { get; set; }
        public String PngFile { get; set; }
        public bool IsSampleDownloaded { get; set; }
        public bool IsDownloaded 
        {
            get { return _isDownloaded; }
            set
            {
                _isDownloaded = value;
                var resourceLoader = new ResourceLoader();
                if (_isDownloaded)
                {
                    DownloadOrReadButton = resourceLoader.GetString("read");
                    SampleOrDeleteButton = resourceLoader.GetString("delete");
                    Button1Tag = TAG_READ;
                    Button2Tag = TAG_DEL;
                }
                else
                {
                    DownloadOrReadButton = resourceLoader.GetString("download");
                    SampleOrDeleteButton = resourceLoader.GetString("sample");
                    if (!_isDownloaded && !IsPaid)
                    {
                        SecondButtonVisible = false;
                    }
                    else
                    {
                        SecondButtonVisible = true;
                    }
                    Button1Tag = TAG_DOWNLOAD;
                    Button2Tag = TAG_SAMPLE;
                }
                OnPropertyChanged("Button1Tag");
                OnPropertyChanged("Button2Tag");
                OnPropertyChanged("SecondButtonVisible");
                OnPropertyChanged("DownloadOrReadButton");
                OnPropertyChanged("SampleOrDeleteButton");
                OnPropertyChanged("IsDownloaded");
                OnPropertyChanged("IsFree");
            }
        }
        public bool IsFree
        {
            get
            {
                return !IsPaid && !IsDownloaded;
            }
        }

        public int Index { get; set; }
        public bool IsPaid { get; set; }
        public bool SecondButtonVisible { get; set; }
        public bool IsOpening 
        {
            get { return _isOpening; }
            set
            {
                _isOpening = value;
                OnPropertyChanged("IsOpening");
                //var resourceLoader = new ResourceLoader();
                //if (_isOpening)
                //{
                //    DownloadOrReadButton = resourceLoader.GetString("opening");
                //}
                //else
                //{
                //    DownloadOrReadButton = resourceLoader.GetString("read");
                //}
                //OnPropertyChanged("DownloadOrReadButton");
            }
        }
        public String FileName { get; set; }
        public String RelativePath { get; set; }
        public ImageSource Image 
        {
            get { return _image; }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }
        public String DownloadOrReadButton 
        {
            get { return _downloadOrReadButton;  }
            set
            {
                _downloadOrReadButton = value;
                OnPropertyChanged("DownloadOrReadButton");
            }
        }
        public String SampleOrDeleteButton 
        {
            get { return _sampleOrDeleteButton; }
            set
            {
                _sampleOrDeleteButton = value;
                OnPropertyChanged("SampleOrDeleteButton");
            }
        }
        public String Button1Tag { get; set; }
        public String Button2Tag { get; set; }
        public String MagazineTag { get; set; }
        //public MagazineViewModel(String uniqueId, String title, String subtitle, MagazineDataGroup group, string tumb, string b1, string b2)
        //    : base(uniqueId, title, subtitle)
        //{
        //    Title = title;
        //    Subtitle = subtitle;
        //    Thumbnail=tumb;
        //    DownloadOrReadButton=b1;
        //    SampleOrDeleteButton=b2;
        //    Button1Tag = "t1";
        //    Button2Tag = "t2";
        //    MagazineTag = "m1";

        //    this._group = group;
        //}

        public MagazineViewModel(String uniqueId, int colSpan, int rowSpan, int width, String title, String subtitle, MagazineDataGroup group, MagazineModel m)
            : base(uniqueId, title, subtitle)
        {
            _width = width;
            Title = m.Title;
            Subtitle = m.Subtitle;
            IsDownloaded = m.isDowloaded;
            IsSampleDownloaded = m.isSampleDowloaded;
            IsPaid = m.isPaid;
            FileName = m.fileName;
            RelativePath = m.relativePath;
            Thumbnail = String.Format("ms-appdata:///local/Covers/{0}", m.pngPath);
            PngFile = m.pngPath;
            PngUrl = m.pngUrl;
            PngPath = m.assetsDir + m.pngPath;
            SecondButtonVisible = true;
            Index = m.Index;
            ColSpan = colSpan;
            RowSpan = rowSpan;
            _width *= ColSpan;
            _height *= RowSpan;
            if (ColSpan > 1 && RowSpan > 1)
            {
                Thumbnail = Thumbnail.Replace(".png", "_newsstand.png");
                PngFile = PngFile.Replace(".png", "_newsstand.png");
                PngUrl = PngUrl.Replace(".png", "_newsstand.png");
                _width += 10;// *ColSpan;
                _height += 10;// *RowSpan;
            }
            //if (img != null)
            //{
            //    Image = img;
            //}
            //else 
            //{
            //    Image = new BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", m.pngPath)));
            //}
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

        public bool Update(int colSpan, int rowSpan, int width, MagazineModel m)
        {
            bool needUpdateLayout = false;

            if (Title != m.Title)
                Title = m.Title;
            if (Subtitle != m.Subtitle)
                Subtitle = m.Subtitle;
            if (IsDownloaded != m.isDowloaded)
            {
                IsDownloaded = m.isDowloaded;
                var resourceLoader = new ResourceLoader();
                if (m.isDowloaded)
                {
                    DownloadOrReadButton = resourceLoader.GetString("read");
                    SampleOrDeleteButton = resourceLoader.GetString("delete");
                    Button1Tag = TAG_READ;
                    Button2Tag = TAG_DEL;
                }
                else
                {
                    DownloadOrReadButton = resourceLoader.GetString("download");
                    SampleOrDeleteButton = resourceLoader.GetString("sample");
                    if (!m.isDowloaded && !m.isPaid)
                    {
                        SecondButtonVisible = false;
                    }
                    else
                    {
                        SecondButtonVisible = true;
                    }
                    Button1Tag = TAG_DOWNLOAD;
                    Button2Tag = TAG_SAMPLE;
                }
            }
            if (IsSampleDownloaded == m.isSampleDowloaded)
                IsSampleDownloaded = m.isSampleDowloaded;
            if (IsPaid != m.isPaid)
                IsPaid = m.isPaid;
            if (FileName != m.fileName)
                FileName = m.fileName;
            if (RelativePath != m.relativePath)
                RelativePath = m.relativePath;
            if (Thumbnail != String.Format("ms-appdata:///local/Covers/{0}", m.pngPath))
            {
                needUpdateLayout = true;
                Thumbnail = String.Format("ms-appdata:///local/Covers/{0}", m.pngPath);
            }
            if (PngFile != m.pngPath)
                PngFile = m.pngPath;
            if (PngUrl != m.pngUrl)
                PngUrl = m.pngUrl;
            if (PngPath != m.assetsDir + m.pngPath)
                PngPath = m.assetsDir + m.pngPath;

            ColSpan = colSpan;
            RowSpan = rowSpan;
            var w = width;
            var h = 238;
            w *= ColSpan;
            h *= RowSpan;
            if (ColSpan > 1 && RowSpan > 1)
            {
                Thumbnail = Thumbnail.Replace(".png", "_newsstand.png");
                PngFile = PngFile.Replace(".png", "_newsstand.png");
                PngUrl = PngUrl.Replace(".png", "_newsstand.png");
                w += 10;// * ColSpan;
                h += 10;// * RowSpan;
            }
            if (w != Width || h != Height)
            {
                needUpdateLayout = true;
                Width = w;
                Height = h;
            }
            //if (img != null)
            //{
            //    Image = img;
            //}
            //else 
            //{
            //    Image = new BitmapImage(new Uri(String.Format("ms-appdata:///local/{0}", m.pngPath)));
            //}
            //Thumbnail = String.Format("ms-appdata:///local/{0}", m.pngPath);
            if (MagazineTag != m.fileName)
            {
                var resourceLoader = new ResourceLoader();
                MagazineTag = m.fileName;
            }

            return needUpdateLayout;
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
                    if (e.NewStartingIndex < 96)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        if (TopItems.Count > 96)
                        {
                            TopItems.RemoveAt(96);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 96 && e.NewStartingIndex < 96)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 96)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[21]);
                    }
                    else if (e.NewStartingIndex < 96)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(96);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 96)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 96)
                        {
                            TopItems.Add(Items[95]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 96)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 96)
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
            set { _items = value; }
        }

        private ObservableCollection<MagazineViewModel> _topItem = new ObservableCollection<MagazineViewModel>();
        public ObservableCollection<MagazineViewModel> TopItems
        {
            get { return this._topItem; }
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

        public static void RemoveGroups()
        {
            _sampleDataSource = new MagazineDataSource();
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

        public MagazineDataSource() {  }

        public static IReadOnlyList<MagazineViewModel> LoadMagazines(IList<LibrelioLocalUrl> magazines, int width) {

           var list = new List<MagazineViewModel>();

            bool newGroup = false;
            MagazineDataGroup group = null;
            var loader = new ResourceLoader();
            if (GetGroup(loader.GetString("my_magazines")) == null) {

                group = new MagazineDataGroup(loader.GetString("my_magazines"), loader.GetString("my_magazines"), "");
                newGroup = true;

            } else {

                group = GetGroup(loader.GetString("my_magazines"));
            }

            if (newGroup) {

                _sampleDataSource.AllGroups.Add(group);
                newGroup = false;
            }

            for (int i = 0; i < magazines.Count; i++) {

                if (magazines[i].IsDownloaded) {

                    var m = new MagazineModel(magazines[i], i);
                    var item = GetItem(m.Title + m.Subtitle);
                    if (item != null)
                    {
                        var b = item.Update(1, 1, width, m);
                        if (b) list.Add(item);
                        continue;
                    }

                    group.Items.Add(new MagazineViewModel(m.Title + m.Subtitle, 1, 1, width, m.Title, m.Subtitle, group, m));
                }
            }

            newGroup = false;

            if (GetGroup(loader.GetString("all_magazines")) == null) {

                group = new MagazineDataGroup(loader.GetString("all_magazines"), loader.GetString("all_magazines"), "");
                newGroup = true;

            } else {

                group = GetGroup(loader.GetString("all_magazines"));
            }

            if (newGroup) {

                _sampleDataSource.AllGroups.Add(group);
                newGroup = false;
            }

            for (int i = 0; i < magazines.Count; i++) {

                var m = new MagazineModel(magazines[i], magazines[i].Index);
                var item = GetItem(m.Title + m.Subtitle + "1");
                if (item != null)
                {
                    if (m.Index == 0)
                    {
                        var b = item.Update(2, 2, width, m);
                        if (b) list.Add(item);
                    }
                    else
                    {
                        var b = item.Update(1, 1, width, m);
                        if (b) list.Add(item);
                    }

                    continue;
                }

                if (m.Index == 0) {

                    var it = new MagazineViewModel(m.Title + m.Subtitle + "1", 2, 2, width, m.Title, m.Subtitle, group, m);
                    if (it.IsDownloaded)
                        it.SecondButtonVisible = false;
                    if (it.Index < group.Items.Count)
                        group.Items.Insert(it.Index, it);
                    else
                        group.Items.Add(it);

                    list.Add(it);

                } else {

                    var it = new MagazineViewModel(m.Title + m.Subtitle + "1", 1, 1, width, m.Title, m.Subtitle, group, m);
                    if (it.IsDownloaded)
                        it.SecondButtonVisible = false;
                    if (it.Index < group.Items.Count)
                        group.Items.Insert(it.Index, it);
                    else
                        group.Items.Add(it);

                    list.Add(it);
                }
            }

            return list;
        }

        //public static async Task<MagazineManager> LoadMagazinesAsync()
        //{
        //    if (_sampleDataSource.AllGroups.Count > 0) return null;

        //    var app = Application.Current as App;
        //    var manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/" + app.ClientName + "/" + app.MagazineName + "/", "Magazines");

        //    await manager.LoadLocalMagazineList();

        //    //if (manager.MagazineLocalUrl.Count == 0)
        //    //{
        //        //await manager.LoadPLISTAsync();
        //    //}

        //    bool newGroup = false;
        //    MagazineDataGroup group = null;
        //    if (GetGroup("My Magazines") == null) {

        //        group = new MagazineDataGroup("My Magazines", "My Magazines", "");
        //        newGroup = true;

        //    } else {

        //        group = GetGroup("My Magazines");
        //    }

        //    if (newGroup) {

        //        _sampleDataSource.AllGroups.Add(group);
        //        newGroup = false;
        //    }

        //    for (int i = 0; i < manager.MagazineLocalUrl.Count; i++)
        //    {

        //        if (manager.MagazineLocalUrl[i].IsDownloaded)
        //        {

        //            var m = new MagazineModel(manager.MagazineLocalUrl[i], i);
        //            if (GetItem(m.Title + m.Subtitle) != null) continue;
        //            BitmapImage image = null;
        //            try
        //            {
        //                var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
        //                image = new BitmapImage();
        //                await image.SetSourceAsync(await file.OpenReadAsync());
        //            }
        //            catch { }
  
        //            group.Items.Add(new MagazineViewModel(m.Title + m.Subtitle, 1, 1, m.Title, m.Subtitle, group, m));
        //        }
        //    }

        //    newGroup = false;

        //    if (GetGroup("All Magazines") == null) {

        //        group = new MagazineDataGroup("All Magazines", "All Magazines", "");
        //        newGroup = true;

        //    } else {

        //        group = GetGroup("All Magazines");
        //    }

        //    if (newGroup)
        //    {
        //        _sampleDataSource.AllGroups.Add(group);
        //        newGroup = false;
        //    }

        //    await manager.LoadPLISTAsync();

        //    var count = manager.MagazineLocalUrl.Count;
        //    if (count == 0)
        //        count = manager.MagazineUrl.Count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        LibrelioLocalUrl localUrl = null;
        //        MagazineModel m = null;
        //        //if (manager.MagazineLocalUrl.Count == 0)
        //        //{
        //            localUrl = manager.FindInMetadata(manager.MagazineUrl[i]);
        //            if (localUrl != null && localUrl.IsDownloaded)
        //                m = new MagazineModel(localUrl, manager.MagazineUrl[i].Index);
        //            else
        //                m = new MagazineModel(manager.MagazineUrl[i]);
        //        //}
        //        //else
        //        //{
        //            //localUrl = manager.MagazineLocalUrl[i];
        //            //m = new MagazineModel(localUrl, i);
        //        //}

        //        if (GetItem(m.Title + m.Subtitle + "1") != null) continue;
        //        BitmapImage image = null;
        //        try
        //        {
        //            if (group.Items.Count == 0)
        //                image = new BitmapImage(new Uri(m.pngUrl.Replace(".png", "_newsstand.png")));
        //            else
        //                image = new BitmapImage(new Uri(m.pngUrl));

        //            //if (localUrl != null && localUrl.IsDownloaded)
        //            //{

        //            //    var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
        //            //    image = new BitmapImage();
        //            //    await image.SetSourceAsync(await file.OpenReadAsync());

        //            //}
        //            //else
        //            //{

        //            //    image = new BitmapImage(new Uri(m.pngUrl));
        //            //}

        //        }
        //        catch { }
        //        MagazineViewModel item = null;
        //        if (group.Items.Count == 0)
        //            item = new MagazineViewModel(m.Title + m.Subtitle + "1", 2, 2, m.Title, m.Subtitle, image, group, m);
        //        else
        //            item = new MagazineViewModel(m.Title + m.Subtitle + "1", 1, 1, m.Title, m.Subtitle, image, group, m);

        //        if (localUrl != null && localUrl.IsDownloaded)
        //            item.SecondButtonVisible = false;
        //        group.Items.Add(item);
        //    }

        //    group.Items = new ObservableCollection<MagazineViewModel>(group.Items.OrderBy(item => item.Index));
            
        //    //if (GetGroup("All Magazines") != null) return null;

        //    //PList list = new PList("Assets/data/magazines.plist");
        //    ////TODODEBUG try

        //    //    List<dynamic> arr = list[""];
        //    //    group = new MagazineDataGroup("All Magazines", "All Magazines", "");
        //    //    for (int i = 0; i < arr.Count; i++)
        //    //    {
        //    //        var m = new MagazineModel((Dictionary<string, dynamic>)arr[i]);
        //    //        group.Items.Add(new MagazineViewModel("", "", "", null, group, m));
        //    //    }
        //    //    _sampleDataSource.AllGroups.Add(group);

        //    return manager;
        //}

        //public static async Task<MagazineManager> LoadLocalMagazinesAsync()
        //{
        //    if (_sampleDataSource.AllGroups.Count > 0) return null;

        //    var app = Application.Current as App;
        //    var manager = new MagazineManager("http://librelio-europe.s3.amazonaws.com/" + app.ClientName + "/" + app.MagazineName + "/", "Magazines");

        //    await manager.LoadLocalMagazineList();

        //    bool newGroup = false;
        //    MagazineDataGroup group = null;
        //    if (GetGroup("My Magazines") == null) {

        //        group = new MagazineDataGroup("My Magazines", "My Magazines", "");
        //        newGroup = true;

        //    } else {

        //        group = GetGroup("My Magazines");
        //    }

        //    if (newGroup) {

        //        _sampleDataSource.AllGroups.Add(group);
        //        newGroup = false;
        //    }

        //    for (int i = 0; i < manager.MagazineLocalUrl.Count; i++) {

        //        if (manager.MagazineLocalUrl[i].IsDownloaded) {

        //            var m = new MagazineModel(manager.MagazineLocalUrl[i], i);
        //            if (GetItem(m.Title + m.Subtitle) != null) continue;
        //            BitmapImage image = null;
        //            try
        //            {
        //                var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
        //                image = new BitmapImage();
        //                await image.SetSourceAsync(await file.OpenReadAsync());
        //            }
        //            catch { }
        //            int index = 0;
        //            for (int p = 0; p < group.Items.Count; p++)
        //            {
        //                index = p + 1;
        //                if (group.Items[p].Index > m.Index)
        //                    break;
        //            }

        //            group.Items.Insert(index, new MagazineViewModel(m.Title + m.Subtitle, 1, 1, m.Title, m.Subtitle, image, group, m));
        //        }
        //    }

        //    newGroup = false;

        //    if (GetGroup("All Magazines") == null) {

        //        group = new MagazineDataGroup("All Magazines", "All Magazines", "");
        //        newGroup = true;

        //    } else {

        //        group = GetGroup("All Magazines");
        //    }

        //    if (newGroup)
        //    {
        //        _sampleDataSource.AllGroups.Add(group);
        //        newGroup = false;
        //    }

        //    for (int i = 0; i < manager.MagazineLocalUrl.Count; i++) {

        //            var m = new MagazineModel(manager.MagazineLocalUrl[i], i);
        //            if (GetItem(m.Title + m.Subtitle + "1") != null) continue;
        //            BitmapImage image = null;
        //            try
        //            {
        //                //if (m.isDowloaded) {

        //                //    var file = await StorageFile.GetFileFromPathAsync(m.pngPath);
        //                //    image = new BitmapImage();
        //                //    await image.SetSourceAsync(await file.OpenReadAsync());

        //                //} else {

        //                if (group.Items.Count == 0)
        //                    image = new BitmapImage(new Uri(m.pngUrl.Replace(".png", "_newsstand.png")));
        //                else
        //                    image = new BitmapImage(new Uri(m.pngUrl));
        //                //}

        //            }
        //            catch { }
        //            int index = 0;
        //            for (int p = 0; p < group.Items.Count; p++)
        //            {
        //                index = p + 1;
        //                if (group.Items[p].Index > m.Index)
        //                    break;
        //            }
        //            MagazineViewModel item = null;
        //            if (group.Items.Count == 0)
        //                item = new MagazineViewModel(m.Title + m.Subtitle + "1", 2, 2, m.Title, m.Subtitle, image, group, m);
        //            else
        //                item = new MagazineViewModel(m.Title + m.Subtitle + "1", 1, 1, m.Title, m.Subtitle, image, group, m);

        //            if (m.isDowloaded)
        //                item.SecondButtonVisible = false;
        //            group.Items.Insert(index, item);
        //    }

        //    return manager;
        //}

        //public MagazineDataSource()
        //{
        //    _allMagazines.Add(new MagazineDataGroup("aaa", "bbb", "vccc", "buy", "sample"));
        //    _allMagazines.Add(new MagazineDataGroup("aaa1", "bbb2", "vccc", "buy", "sample"));
            //_allMagazines.Add(new MagazineDataGroup("aaa2", "bbb3", "vccc", "buy", "sample"));
            //_allMagazines.Add(new MagazineDataGroup("aaa3", "bbb4", "vccc", "buy", "sample"));
        //}
    }
}
