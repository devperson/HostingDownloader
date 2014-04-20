using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UploaderToHosting.ViewModel
{
    public class UploadViewModel : BaseViewModel
    { 
        public int UploadingCount = 4;
        private int CurrentPartIndex = 0;
        private int AllPartCount = -1;
        /// <summary>
        /// Start multiple uploading simultaneously
        /// </summary>
        public void StartUpload()
        {
        }
        /// <summary>
        /// Uploads part
        /// </summary>
        private void UploadPart()
        {
        }

        private void OnCompletedUpload()
        {
        }

        /// <summary>
        /// Gets next part index
        /// </summary>
        /// <returns>next part index</returns>
        private int NextPartIndex()
        {
            //lock should be used here
            return -1;
        }
    }
}
