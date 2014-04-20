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
        public UploadViewModel()
        {            
            this.IsBrawseVisible = true;                        
        }

        #region Properties
        
        int uploadingCount = 8;
        long partSize = 1024 * 1024;

        int currentPartIndex = 0;
        DataAccess.Models.FileInfo file;
        WPFHubClient hubClient;
        ApiService service = new ApiService();
        public bool isPaused = false;
        object locking = new object();
        int sendCount = 0;

        private string fname;
        public string FilePath
        {
            get
            {
                return this.fname;
            }
            set
            {
                this.fname = value;
                this.RaisePropertyChanged(p => p.FilePath);
            }
        }
        private bool brawseVisible;
        public bool IsBrawseVisible
        {
            get
            {
                return this.brawseVisible;
            }
            set
            {
                this.brawseVisible = value;
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
        private int prog;
        public int Progress
        {
            get
            {
                return this.prog;
            }
            set
            {
                this.prog = value;
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
                file.PartSize = partSize;
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
            hubClient = new WPFHubClient(Constants.Host, Clients.Uploader, OnMessageRecieved);
            this.StartUpload(true);
            this.IsStartVisible = false;
            this.IsProgressVisible = true;
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
            for (int i = 0; i < uploadingCount; i++)
            {
                this.UploadPart();
            }
        }
        /// <summary>
        /// Uploads part
        /// </summary>
        private void UploadPart()
        {
            lock (locking)
            {
                if (!this.isPaused)
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
                            this.CalcProgress();
                            this.UploadPart();
                        });
                    }
                    else
                        this.IsFinished = true;
                }
            }
        }

        private void CalcProgress()
        {
            sendCount++;
            this.Progress = (sendCount * 100) / file.PartsCount;
        }

        private void OnMessageRecieved(MsgData data)
        {
            if (data.Message == Messages.PauseUploading)
            {
                Debug.WriteLine("Paused");
                this.isPaused = true;
            }
            if (data.Message == Messages.ContinueUploading)
            {
                Debug.WriteLine("Unpaused");
                this.isPaused = false;
                this.StartUpload();
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
