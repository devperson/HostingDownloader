using CommonLibrary;
using DataAccess;
using DataAccess.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DownloaderFromHosting.ViewModel
{
    public class DownloadViewModel : ObservableObject
    {
        public DownloadViewModel()
        {
            this.IsPauseVisible = false;
            this.IsRestartVisible = false;
            this.IsStartVisible = true;
            
            context.Configuration.AutoDetectChangesEnabled = false;
            context.Configuration.ValidateOnSaveEnabled = false;
        }

        #region Properties
        ClientDataBaseContext context = new ClientDataBaseContext();
        WPFHubClient hubClient;
        ApiService service = new ApiService();
        DataAccess.Models.FileInfo file;
        int downloadedCount;
        List<long> failedIds = new List<long>();        
        
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

        private int _dwCount;
        public int DownloadingThreadsCount
        {
            get
            {
                return this._dwCount;
            }
            set
            {
                this._dwCount = value;
                this.RaisePropertyChanged(p => p.DownloadingThreadsCount);
            }
        }

        private bool _isCreatingFile;
        public bool IsCreatingFile
        {
            get
            {
                return this._isCreatingFile;                
            }
            set
            {
                this._isCreatingFile = value;
                this.RaisePropertyChanged(p => p.IsCreatingFile);
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

        DispatcherTimer t = new DispatcherTimer();
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
            using (ClientDataBaseContext context = new ClientDataBaseContext())
            {
                var objCtx = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)context).ObjectContext;
                objCtx.ExecuteStoreCommand("TRUNCATE TABLE [ClientFileParts]");                
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

                t.Interval = TimeSpan.FromSeconds(5);
                t.Tick += (s, e) =>
                {
                    if (this.DownloadingThreadsCount == 0 && !failedIds.Any())
                    {
                        hubClient.SendMessage(new MsgData { From = Clients.Downloader, To = Clients.Uploader, Message = Messages.ContinueUploading });
                    }
                };
                t.Start();
            }

            if (data.Message.Contains(Messages.DownloadAvailable))
            {
                this.DownloadingThreadsCount++;
                long partId = long.Parse(data.Message.Replace(Messages.DownloadAvailable, ""));
                Debug.WriteLine(string.Format("Downloading part with id {0}", partId));
                service.GetPart(partId, OnReceivedPart);
            }

            if (data.Message == Messages.UploadingAppLoaded)
            {
                if (IsStartVisible == false) // notify uploader about downloader ready state.
                {
                    hubClient.SendMessage(new MsgData { From = Clients.Downloader, To = Clients.Uploader, Message = Messages.DownloadingAppReady });
                }
            }
        }

        private void OnReceivedPart(FilePart part, long id)
        {
            this.DownloadingThreadsCount--;

            if (part != null)
            {                                
                Debug.WriteLine(string.Format("Part received with id {0}", id));                
                downloadedCount++;

                service.RemovePart(id);

                var path = string.Format(@"{0}\{1}{2}", "Parts", part.Part, file.FileName);
                ClientFilePart clientPart = new ClientFilePart();
                clientPart.Part = part.Part;
                clientPart.FilePath = path;
                context.Parts.Add(clientPart);

                this.WritePartToDisk(path, part);

                if (downloadedCount % 100 == 0)
                {
                    context.SaveChanges();
                    context.Dispose();
                    context = new ClientDataBaseContext();
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;
                }

                this.CalcProgress();
            }
            else
            {
                Debug.WriteLine(string.Format("Null part received with id {0}", id));
                failedIds.Add(id);
            }

            if (this.DownloadingThreadsCount == 0)
            {
                var faileId = failedIds.FirstOrDefault();
                failedIds.Remove(faileId);
                if (faileId != 0)
                {
                    this.DownloadingThreadsCount++;
                    service.GetPart(faileId, OnReceivedPart);
                }
            }
        }

        private async void WritePartToDisk(string path, FilePart part)
        {
            await Task.Run(() =>
            {
                if (!Directory.Exists("Parts"))
                    Directory.CreateDirectory("Parts");

                File.WriteAllBytes(path, part.Bytes);
            });
        }

        private void CalcProgress()
        {            
            this.Progress = (downloadedCount * 100) / file.PartsCount;

            if (downloadedCount == file.PartsCount)
            {
                context.SaveChanges();
                context.Dispose();

                this.CreateFile();

                this.IsFinished = true;
                this.IsPauseVisible = false;
                this.IsRestartVisible = false;
                this.IsStartVisible = false;

                t.Stop();
                t.IsEnabled = false;

                this.OnClearRemoteDbCommand();
            }
        }

        private async void CreateFile()
        {
            this.IsCreatingFile = true;
            await Task.Run(() =>
            {
                if (!Directory.Exists("Downloads"))
                    Directory.CreateDirectory("Downloads");
                var mergFilePath = string.Format(@"Downloads\{0}", file.FileName);
                List<ClientFilePart> portion = new List<ClientFilePart>();
                int iteration = 0;

                using (var context = new ClientDataBaseContext())
                {
                    portion = context.Parts.OrderBy(p => p.Part).Skip(0).Take(5).ToList();
                    portion.Reverse();
                }
                while (portion.Any())
                {
                    var stack = new Stack<ClientFilePart>(portion);
                    while (stack.Any())
                    {
                        var p = stack.Pop();

                        var bytes = File.ReadAllBytes(p.FilePath);
                        using (var fs = new FileStream(mergFilePath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            var seek = (p.Part - 1) * file.PartSize;  
                            fs.Seek(seek, SeekOrigin.Begin);
                            using (BinaryWriter writer = new BinaryWriter(fs))
                            {                                                                                   
                                //writer.Seek(seek, SeekOrigin.Begin);
                                writer.Write(bytes, 0, bytes.Count());
                            }
                        }
                    }
                    iteration++;
                    using (var context = new ClientDataBaseContext())
                    {
                        portion = context.Parts.OrderBy(p => p.Part).Skip(5 * iteration).Take(5).ToList();
                        portion.Reverse();
                    }
                }
                Directory.Delete("Parts", true);
                this.OnClearLocalDbCommand();
            });
            this.IsCreatingFile = false;
        }
        #endregion
    }
}
