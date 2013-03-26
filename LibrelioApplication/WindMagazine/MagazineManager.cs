using System;
using System.Collections.Generic;
using Windows.Storage;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace LibrelioApplication
{
    public class MagazineManager
    {
        private string _path;
        private string _name;
        private LibrelioUrl _pList = null;
        private IList<LibrelioUrl> _pdf = new List<LibrelioUrl>();

        public LibrelioUrl PLIST { get { return _pList; } }
        public IList<LibrelioUrl> PDF { get { return _pdf; } }

        /// <summary>
        /// You can download .plist, .pdf, and different assets
        /// </summary>
        /// <param name="path">The path to the magazine directory</param>
        /// <param name="name">Name of collection</param>
        public MagazineManager(string path, string name)
        {
            this._path = path;

            _pList = new LibrelioUrl(path, name + ".plist");
        }

        public async Task LoadPLISTAsync()
        {
            using (var stream = await DownloadManager.DownloadFileAsync(_pList))
            {
                await DownloadManager.StoreToFolderAsync(_pList.FullName, KnownFolders.DocumentsLibrary, stream);

                var pListString = await DownloadManager.GetStringFromStream(stream);
                ReadPList(pListString);
            }
        }

        public IList<LibrelioUrl> GetAssets(string magazineName)
        {
            return null;
        }

        private void ReadPList(string plist)
        {
            var pos = plist.IndexOf("FileName", 0);
            while (pos >= 0)
            {
                var relUrl = GetValue(plist, pos);
                if (relUrl != "")
                {
                    var pdf = new LibrelioUrl(this._path, relUrl);
                    var p = plist.IndexOf("Title", pos);
                    if (p >= 0)
                    {
                        var title = GetValue(plist, p);
                        if (title != "")
                            pdf.Name = title;
                    }
                    _pdf.Add(pdf);
                }

                pos = plist.IndexOf("FileName", pos + 1); 
            }
        }

        private string GetValue(string plist, int pos)
        {
            var start = plist.IndexOf("<string>", pos);
            var end = plist.IndexOf("</string>", pos);
            if (start >= 0 && end >= 0 && start < end)
            {
                start += 8;
                return plist.Substring(start, end - start);
            }
            else
            {
                return "";
            }
        }
    }
}