using System;
using System.IO;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Gaming.Coop.Generic
{
    /// <summary>
    /// One handler script of the handlers folder as GameManager last read it (Nucleus 2.5): the file, when it was
    /// written, and the parsed handler or why it failed. Used to reload handlers when their file changes.
    /// </summary>
    public class HandlerInfo
    {
        private readonly GenericGameInfo genericGameInfo;
        private readonly string fileName;
        private readonly string fileFullPath;
        private readonly DateTime lastWriteTimeUtc;
        private readonly string error;
        private readonly string errorDetails;

        /// <summary>
        /// The parsed handler in use for this file, or null when the file never parsed. After an edit that broke the script
        /// it is still the previous version (<see cref="Error"/> says what is wrong with the file now).
        /// </summary>
        public GenericGameInfo GenericGameInfo => genericGameInfo;

        public string FileName => fileName;

        public string FileFullPath => fileFullPath;

        public string GUID => genericGameInfo?.GUID;

        public string GameName => genericGameInfo?.GameName;

        /// <summary>Last write time of the file when it was read.</summary>
        public DateTime LastWriteTimeUtc => lastWriteTimeUtc;

        /// <summary>Why the file as it is now can't be used (script error, no GUID or game name), or null.</summary>
        public string Error => error;

        /// <summary>
        /// The short form of <see cref="Error"/> for a script error ("Error at line 12 ..."), to show when the previous version
        /// stays in use; otherwise the same text as <see cref="Error"/>.
        /// </summary>
        public string ErrorDetails => errorDetails ?? error;

        /// <summary>The file parsed without error and declares a game name and a GUID.</summary>
        public bool IsValid => error == null && genericGameInfo != null && !string.IsNullOrEmpty(genericGameInfo.GameName) && !string.IsNullOrEmpty(genericGameInfo.GUID);

        /// <summary>
        /// To use when the GenericGameInfo can be null, so we get the file related info from the FileInfo instead
        /// </summary>
        public HandlerInfo(FileInfo fileInfo, GenericGameInfo genericGameInfo)
            : this(fileInfo.FullName, fileInfo.Name, fileInfo.Exists ? fileInfo.LastWriteTimeUtc : DateTime.MinValue, genericGameInfo, Describe(genericGameInfo))
        {
        }

        public HandlerInfo(string fileFullPath, string fileName, DateTime lastWriteTimeUtc, GenericGameInfo genericGameInfo, string error, string errorDetails = null)
        {
            this.fileFullPath = fileFullPath;
            this.fileName = fileName;
            this.lastWriteTimeUtc = lastWriteTimeUtc;
            this.genericGameInfo = genericGameInfo;
            this.error = error;
            this.errorDetails = errorDetails;
        }

        /// <summary>Why a parsed handler can't be used (script error, missing GUID or game name), or null.</summary>
        public static string Describe(GenericGameInfo info)
        {
            if (info == null)
            {
                return T("engine.handlers.invalid.unreadable");
            }

            if (info.ScriptError != null)
            {
                return info.ScriptError;
            }

            if (string.IsNullOrEmpty(info.GameName))
            {
                return T("engine.handlers.invalid.noGameName");
            }

            if (string.IsNullOrEmpty(info.GUID))
            {
                return T("engine.handlers.invalid.noGuid");
            }

            return null;
        }
    }
}
