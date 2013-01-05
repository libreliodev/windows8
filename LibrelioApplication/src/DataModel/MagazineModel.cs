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

        public MagazineModel(String fileName, String title, String subtitle)
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


    public class MagazineViewModel : LibrelioApplication.Common.BindableBase
    {
        public const string TAG_READ  = "READ";
        public const string TAG_DEL  = "DEL";
        public const string TAG_SAMPLE  = "SAMPLE";
        public const string TAG_DOWNLOAD  = "DOWNLOAD";

        public String Title { get; set; }
        public String Subtitle { get; set; }
        public String Thumbnail { get; set; }
        public String DownloadOrReadButton { get; set; }
        public String SampleOrDeleteButton { get; set; }
        public String Button1Tag { get; set; }
        public String Button2Tag { get; set; }
        public String MagazineTag { get; set; }
        public MagazineViewModel(string title, string subtitle, string tumb, string b1, string b2) {
            Title = title;
            Subtitle = subtitle;
            Thumbnail=tumb;
            DownloadOrReadButton=b1;
            SampleOrDeleteButton=b2;
            Button1Tag = "t1";
            Button2Tag = "t2";
            MagazineTag = "m1";
        }

        public MagazineViewModel(MagazineModel m) {
            Title = m.Title;
            Subtitle = m.Subtitle;
            Thumbnail = String.Format("ms-appdata:///local/{0}", m.pngPath);
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
                Button1Tag = TAG_DOWNLOAD;
                Button2Tag = TAG_SAMPLE;
            }
            
        }

    }


    /// <summary>
    /// Creates a collection of Magazines. Hardcoded (for design time) and from plist (runtime).
    /// </summary>
    public sealed class MagazineDataSource
    {
        private static MagazineDataSource _sampleDataSource = new MagazineDataSource();

        private ObservableCollection<MagazineViewModel> _allMagazines = new ObservableCollection<MagazineViewModel>();
        public ObservableCollection<MagazineViewModel> AllMagazines
        {
            get { return this._allMagazines; }
        }


        public MagazineDataSource(int none) {
            PList list = new PList("Assets/data/magazines.plist");
            //TODODEBUG try
            {
                List<dynamic> arr = list[""];
                for (int i = 0; i < arr.Count; i++)
                {
                    MagazineModel m = new MagazineModel((Dictionary<string, dynamic>)arr[i]);
                    _allMagazines.Add(new MagazineViewModel(m));
                }

            }
            //catch (Exception e)
            //{
            //    throw new Exception("Failed to load list");
            //}
        }
        
        public MagazineDataSource()
        {
            _allMagazines.Add(new MagazineViewModel("aaa", "bbb", "vccc", "buy", "sample"));
            _allMagazines.Add(new MagazineViewModel("aaa1", "bbb2", "vccc", "buy", "sample"));
            _allMagazines.Add(new MagazineViewModel("aaa2", "bbb3", "vccc", "buy", "sample"));
            _allMagazines.Add(new MagazineViewModel("aaa3", "bbb4", "vccc", "buy", "sample"));
        }
    }
}
