using System;
using System.IO;

namespace Nucleus.Gaming.Coop.Generic
{
    public class HandlerInfo
    {
        private GenericGameInfo genericGameInfo;
        public GenericGameInfo GenericGameInfo => genericGameInfo;

        private string fileName;
        public string FileName => fileName;

        private string fileFullPath;
        public string FileFullPath => fileFullPath;

        private string guid;
        public string GUID => guid;
        private string gameName;
        public string GameName => gameName;

        private DateTime lastWriteTimeUtc;
        public DateTime LastWriteTimeUtc => lastWriteTimeUtc;

        public bool IsValid => isvalid();

        private bool isvalid()
        {
            if (GenericGameInfo == null)
            {
                return false;
            }
            else if (string.IsNullOrEmpty(GenericGameInfo.GameName) || string.IsNullOrEmpty(GenericGameInfo.GUID))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// To use when the GenericGameInfo can be null, so we get the file related info from the FileInfo instead
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="genericGameInfo"></param>
        public HandlerInfo(FileInfo fileInfo,GenericGameInfo genericGameInfo)
        {
            this.genericGameInfo = genericGameInfo;
            guid = genericGameInfo?.GUID;
            gameName = genericGameInfo?.GameName;

            fileFullPath = fileInfo.FullName;
            fileName = fileInfo.Name;      
            lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;  
        }
    }
}
