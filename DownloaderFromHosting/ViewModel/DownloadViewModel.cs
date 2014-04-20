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
    public class DownloadViewModel : ObservableObject
    {
        public DownloadViewModel()
        {
            this.IsPauseVisible = false;
            this.IsRestartVisible = false;
            this.IsStartVisible = true;

            
        }

        #region Properties
        WPFHubClient hubClient;
        ApiService service = new ApiService();
        DataAccess.Models.FileInfo file;
        int downloadedCount;
        object locking = new object();

        
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
        private bool isFin;
        public bool IsFinished
        {
            get
            {
                return this.isFin;
            }
            set
            {
                this.isFin = value;
                this.RaisePropertyChanged(p => p.IsFinished);
                this.IsProgressVisible = false;
            }
        }
        private bool ispr;
        public bool IsProgressVisible
        {
            get
            {
                return this.ispr;
            }
            set
            {
                this.ispr = value;
                this.RaisePropertyChanged(p => p.IsProgressVisible);
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
            hubClient.SendMessage(new MsgData { From = Clients.Downloader, To = Clients.Uploader, Message = Messages.DownloadingAppReady });
            this.IsStartVisible = false;
            this.IsPauseVisible = true;
            this.IsProgressVisible = true;
        }
        #endregion

        #region ClearLocalDbCommand
        private RelayCommand _clearLocalDbCommand;
        public RelayCommand ClearLocalDbCommand
        {
            get { return _clearLocalDbCommand ?? (_clearLocalDbCommand = new RelayCommand(OnClearLocalDbCommand)); }
        }

        private void OnClearLocalDbCommand()
        {
            using (DataBaseContext context = new DataBaseContext())
            {
                var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext;
                objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Parts]");
                objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Files]");
            }
        }
        #endregion

        #region ClearRemoteDbCommand
        private RelayCommand _clearRemoteDbCommand;
        public RelayCommand ClearRemoteDbCommand
        {
            get { return _clearRemoteDbCommand ?? (_clearRemoteDbCommand = new RelayCommand(OnClearRemoteDbCommand)); }
        }

        private void OnClearRemoteDbCommand()
        {
            service.ClearDb();
        }
        #endregion
        #endregion

        #region Methods
        private void OnMessageRecieved(MsgData data)
        {
            if (data.Message == Messages.FileInfoAvailable)
            {                
                service.GetFileInfo((fi) =>
                {
                    if (fi != null)
                    {
                        file = fi;
                        service.RemoveFileInfo(file.Id);
                    }
                });
            }

            if (data.Message.Contains(Messages.DownloadAvailable))
            {
                long partId = long.Parse(data.Message.Replace(Messages.DownloadAvailable, ""));
                service.GetPart(partId, (part) =>
                {
                    if (part != null)
                    {
                        //service.RemovePart(part.Id);
                        using (DataBaseContext context = new DataBaseContext())
                        {
                            part.Id = 0;
                            context.Parts.Add(part);
                            context.SaveChanges();
                        }

                        this.CalcProgress();
                    }
                });
            }

            if (data.Message == Messages.UploadingAppLoaded)
            {
                if (IsStartVisible == false) // notify uploader about downloader ready state.
                {
                    hubClient.SendMessage(new MsgData { From = Clients.Downloader, To = Clients.Uploader, Message = Messages.DownloadingAppReady });
                }
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
                    {
                        this.CreateFile();

                        this.IsFinished = true;
                        this.IsPauseVisible = false;
                        this.IsRestartVisible = false;
                        this.IsStartVisible = false;
                    }
                }
            }
        }

        private async void CreateFile()
        {            
            await Task.Run(() =>
            {
                if (!Directory.Exists("Downloads"))
                    Directory.CreateDirectory("Downloads");
                var path = string.Format(@"Downloads\{0}", file.FileName);
                using (var context = new DataBaseContext())
                {
                    int iteration = 0;
                    var portion = context.Parts.OrderBy(p => p.Part).Skip(0).Take(5).ToList();
                    portion.Reverse();

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
                                    writer.Seek((int)((p.Part - 1) * file.PartSize), SeekOrigin.Begin);
                                    writer.Write(p.Bytes, 0, p.Bytes.Count());
                                }
                            }
                        }
                        iteration++;
                        portion = context.Parts.OrderBy(p => p.Part).Skip(5 * iteration).Take(5).ToList();
                        portion.Reverse();
                    }

                    //var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext;
                    //objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Parts]");
                    //objCtx.ExecuteStoreCommand("TRUNCATE TABLE [Files]");
                }
            });
        }
        #endregion
    }
}
