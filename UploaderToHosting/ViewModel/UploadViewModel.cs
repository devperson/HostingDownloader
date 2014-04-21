using CommonLibrary;
using DataAccess.Models;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace UploaderToHosting.ViewModel
{
    public class UploadViewModel : ObservableObject
    {
        private const int UploadingCount = 5;
        private const long PartSize = 1024 * 1024; //1 mb

        public UploadViewModel()
        {
            Extenssions.InvokeAfterSec(0.1, () =>
            {
                hubClient = new WPFHubClient(Constants.Host, Clients.Uploader, OnMessageRecieved);
                hubClient.SendMessage(new MsgData { From = Clients.Uploader, To = Clients.Downloader, Message = Messages.UploadingAppLoaded });
            });
            this.IsBrawseVisible = true;                        
        }

        #region Properties        
        int currentPartIndex = 0;
        DataAccess.Models.FileInfo file;
        WPFHubClient hubClient;
        ApiService service = new ApiService();
        
        object locking = new object();        

        private string _fileInfo;
        public string FilePath
        {
            get
            {
                return this._fileInfo;
            }
            set
            {
                this._fileInfo = value;
                this.RaisePropertyChanged(p => p.FilePath);
            }
        }

        private bool _isBrawseVisible;
        public bool IsBrawseVisible
        {
            get
            {
                return this._isBrawseVisible;
            }
            set
            {
                this._isBrawseVisible = value;
                this.RaisePropertyChanged(p => p.IsBrawseVisible);
            }
        }

        private bool isStartVisible;
        public bool IsStartVisible
        {
            get
            {
                return this.isStartVisible;
            }
            set
            {
                this.isStartVisible = value;
                this.RaisePropertyChanged(p => p.IsStartVisible);
            }
        }

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
                this.IsUploading = false;
            }
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get
            {
                return this._isProgressVisible;
            }
            set
            {
                this._isProgressVisible = value;
                this.RaisePropertyChanged(p => p.IsProgressVisible);
            }
        }

        private bool _isReady;
        public bool IsReady
        {
            get
            {
                return this._isReady;
            }
            set
            {
                this._isReady = value;
                this.RaisePropertyChanged(p => p.IsReady);
            }
        }

        private bool _isReadyVisible;
        public bool IsReadyMessageVisible
        {
            get
            {
                return this._isReadyVisible;
            }
            set
            {
                this._isReadyVisible = value;
                this.RaisePropertyChanged(p => p.IsReadyMessageVisible);                
            }            
        }

        private bool _isErrorVisible = true;
        public bool IsErrorMessageVisible
        {
            get
            {
                return this._isErrorVisible;
            }
            set
            {
                this._isErrorVisible = value;
                this.RaisePropertyChanged(p => p.IsErrorMessageVisible);
            }
        }

        private bool _isPaused;
        public bool IsPaused
        {
            get
            {
                return this._isPaused;
            }
            set
            {
                this._isPaused = value;
                this.RaisePropertyChanged(p => p.IsPaused);
            }
        }

        private bool _isUpload;
        public bool IsUploading
        {
            get
            {
                return this._isUpload;
            }
            set
            {
                this._isUpload = value;
                this.RaisePropertyChanged(p => p.IsUploading);
            }
        }
        #endregion

        #region Commands
        #region SelectFileCommand
        private RelayCommand _selectFileCommand;
        public RelayCommand SelectFileCommand
        {
            get { return _selectFileCommand ?? (_selectFileCommand = new RelayCommand(OnSelectFileCommand)); }
        }
        private void OnSelectFileCommand()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            //dlg.Multiselect = true;
            if (dlg.ShowDialog().GetValueOrDefault())
            {
                this.FilePath = dlg.FileName;
                file = new DataAccess.Models.FileInfo();
                file.PartSize = PartSize;
                file.FileName = Path.GetFileName(this.FilePath);
                using (var fs = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
                {
                    file.PartsCount = (int)(fs.Length / file.PartSize);

                    if ((fs.Length - (file.PartsCount * file.PartSize)) > 0)
                    {
                        file.LastPartSize = fs.Length - (file.PartsCount * file.PartSize);
                        file.PartsCount += 1;
                    }
                }
                this.IsBrawseVisible = false;
                this.IsStartVisible = true;
            }
        }
        #endregion

        #region StartCommand
        private RelayCommand _startCommand;
        public RelayCommand StartCommand
        {
            get { return _startCommand ?? (_startCommand = new RelayCommand(OnStartCommand)); }
        }
        private void OnStartCommand()
        {            
            this.StartUpload(true);
            this.IsStartVisible = false;
            this.IsReadyMessageVisible = false;
            this.IsProgressVisible = true;
            this.IsUploading = true;
        }       
        #endregion        
        #endregion

        #region Methods      
        /// <summary>
        /// Start multiple uploading simultaneously
        /// </summary>
        public void StartUpload(bool postFileInfo = false)
        {
            if (postFileInfo)
                service.UploadFileInfo(file, () => { });
            for (int i = 0; i < UploadingCount; i++)
            {
                this.UploadPart();
            }
        }

        /// <summary>
        /// Uploads part
        /// </summary>
        private void UploadPart()
        {
            //lock (locking)
            //{
                if (!this.IsPaused)
                {
                    ++currentPartIndex;
                    if (file.PartsCount >= currentPartIndex)
                    {
                        FilePart part = new FilePart();
                        part.Part = currentPartIndex;                        
                        part.FileName = file.FileName;
                        part.Bytes = this.ReadFile();
                        service.UploadPart(part, () =>
                        {
                            this.Progress = (currentPartIndex * 100) / file.PartsCount;

                            this.UploadPart();
                        });
                    }
                    else
                        this.IsFinished = true;
                }
            //}
        }       

        private void OnMessageRecieved(MsgData data)
        {
            if (data.Message == Messages.PauseUploading)
            {
                Debug.WriteLine("Paused");
                this.IsPaused = true;
                this.IsUploading = false;
            }
            if (data.Message == Messages.ContinueUploading)
            {
                Debug.WriteLine("Unpaused");
                this.IsPaused = false;
                this.IsUploading = true;
                //this.StartUpload();
            }
            if (data.Message == Messages.DownloadingAppReady)
            {
                this.IsReady = true;
                this.IsReadyMessageVisible = true;
                this.IsErrorMessageVisible = false;
            }
        }

        private byte[] ReadFile()
        {
            byte[] buffer = new byte[file.PartSize];
            using (var fs = new FileStream(this.FilePath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {
                    br.BaseStream.Position = (currentPartIndex - 1) * file.PartSize;
                    if (currentPartIndex == file.PartsCount)//if last chunk
                    {
                        buffer = new byte[file.LastPartSize];
                        buffer = br.ReadBytes((int)file.LastPartSize);
                    }
                    else
                        buffer = br.ReadBytes((int)file.PartSize);
                }
            }
            return buffer;
        }
        #endregion
    }
}
