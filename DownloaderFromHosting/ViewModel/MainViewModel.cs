using CommonLibrary;
using DataAccess;
using DataAccess.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloaderFromHosting.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            this.IsPauseVisible = false;
            this.IsRestartVisible = false;
            this.IsStartVisible = true;
        }
        WPFHubClient hubClient;
        ApiService service = new ApiService();
        DataAccess.Models.FileInfo file;
        int downloadedCount;
        object locking = new object();

        #region Properties
        private int _progress;
        public int Progress
        {
            get
            {
                return this._progress;
            }
            set
            {
                this._progress = value;
                this.RaisePropertyChanged(p => p.Progress);
            }
        }
        private bool _isPauseVisible;
        public bool IsPauseVisible
        {
            get
            {
                return this._isPauseVisible;
            }
            set
            {
                this._isPauseVisible = value;
                this.RaisePropertyChanged(p => p.IsPauseVisible);
            }
        }
        private bool isStart;
        public bool IsStartVisible
        {
            get
            {
                return this.isStart;
            }
            set
            {
                this.isStart = value;
                this.RaisePropertyChanged(p => p.IsStartVisible);
            }
        }
        private bool isRes;
        public bool IsRestartVisible
        {
            get
            {
                return this.isRes;
            }
            set
            {
                this.isRes = value;
                this.RaisePropertyChanged(p => p.IsRestartVisible);
            }
        }
        #endregion

        #region Commands
        #region StartCommand
        private RelayCommand _startCommand;
        public RelayCommand StartCommand
        {
            get { return _startCommand ?? (_startCommand = new RelayCommand(OnStartCommand)); }
        }
        
        private void OnStartCommand()
        {
            hubClient = new WPFHubClient(Constants.Host, Clients.Downloader, OnMessageRecieved);
            this.IsStartVisible = false;
            this.IsPauseVisible = true;
        }
        #endregion
        #endregion

        private void OnMessageRecieved(MsgData data)
        {
            if (data.Message == Messages.FileInfoAvailable)
            {
                service.GetFileInfo((fi) =>
                {
                    service.RemoveFileInfo(fi.Id);
                    file = fi;                                        
                });
            }
            if (data.Message == Messages.DownloadAvailable)
            {              
                service.GetAnyPart((part) =>
                {                    
                    var id = part.Id;
                    service.RemovePart(id);                    
                    using (DataBaseContext context = new DataBaseContext())
                    {
                        part.Id = 0;
                        context.Parts.Add(part);
                        context.SaveChanges();                       
                    }

                    this.CalcProgress();                    
                });
            }
        }

        private void CalcProgress()
        {
            lock (locking)
            {
                downloadedCount++;
                if (file != null)
                {
                    this.Progress = (downloadedCount * 100) / file.PartsCount;

                    if (downloadedCount == file.PartsCount)
                        this.CreateFile();
                }
            }
        }

        private async void CreateFile()
        {
            await Task.Run(() =>
            {
                if (Directory.Exists("Downloads"))
                    Directory.CreateDirectory("Downloads");
                var path = string.Format(@"Downloads\{0}", file.FileName);
                using (var context = new DataBaseContext())
                {
                    int iteration = 0;
                    var portion = context.Parts.OrderBy(p => p.Part).Skip(0).Take(5).ToList();


                    while (portion.Any())
                    {
                        var stack = new Stack<FilePart>(portion);
                        while (stack.Any())
                        {
                            var p = stack.Pop();
                            using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                using (BinaryWriter writer = new BinaryWriter(fs))
                                {
                                    //writer.Seek((int)(position * data.Count()), SeekOrigin.Begin);
                                    writer.Seek((int)((p.Part - 1) * file.PartSize), SeekOrigin.Begin);
                                    writer.Write(p.Bytes, 0, p.Bytes.Count());
                                }
                            }
                        }
                        iteration++;
                        portion = context.Parts.OrderBy(p => p.Part).Skip(5 * iteration).Take(5).Reverse().ToList();
                    }

                }
            });
        }

        
    }
}
