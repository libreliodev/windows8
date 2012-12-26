using System;
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

namespace LibrelioApplication.Data
{
    public class MagazineModel : LibrelioApplication.Common.BindableBase
    {
        private const String TAG = "MagazineModel";
        private const String COMPLETE_FILE = ".complete";
        private const String COMPLETE_SAMPLE_FILE = ".sample_complete";
        private const String PAYED_FILE = ".payed";

        public String title { get; set; }
        public String subtitle { get; set; }
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

        public MagazineModel(String fileName, String title, String subtitle,
                String downloadDate)
        {
            this.fileName = fileName;
            this.title = title;
            this.subtitle = subtitle;
            this.downloadDate = downloadDate;

            //valuesInit(fileName);
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

            public String getMagazineDir(){
                int finishNameIndex = fileName.indexOf("/");
                return ((IBaseContext)context).getStoragePath() + fileName.substring(0,finishNameIndex)+"/";
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

            private void valuesInit(String fileName) {
                isPaid = fileName.contains("_.");
                int startNameIndex = fileName.indexOf("/")+1;
                String png = ((IBaseContext)context).getStoragePath()+fileName.substring(startNameIndex, fileName.length()); 
                pdfUrl = LibrelioApplication.BASE_URL + fileName;
                pdfPath = getMagazineDir()+fileName.substring(startNameIndex, fileName.length());
                if(isPaid){
                    pngUrl = pdfUrl.replace("_.pdf", ".png");
                    pngPath = png.replace("_.pdf", ".png");
                    sampleUrl = pdfUrl.replace("_.", ".");
                    samplePath = pdfPath.replace("_.", ".");
                    File sample = new File(getMagazineDir()+COMPLETE_SAMPLE_FILE);
                    isSampleDowloaded = sample.exists();
                } else {
                    pngUrl = pdfUrl.replace(".pdf", ".png");
                    pngPath = png.replace(".pdf", ".png");
                }
                File complete = new File(getMagazineDir()+COMPLETE_FILE);
                isDowloaded = complete.exists();
		
                assetsDir = getMagazineDir();
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

        public sealed class MagazineDataSource
        {
            private static MagazineDataSource _magazineDataSource = new MagazineDataSource();

            private ObservableCollection<MagazineModel> _allMagazines = new ObservableCollection<MagazineModel>();
            public ObservableCollection<MagazineModel> AllMagazines
            {
                get { return this._allMagazines; }
            }



            public MagazineDataSource()
            {

            }
        }


    }
}