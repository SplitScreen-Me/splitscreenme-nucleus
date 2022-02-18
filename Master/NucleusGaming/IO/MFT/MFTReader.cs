#if UNSAFE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming
{
    public class MFTReader
    {
        private Dictionary<ulong, FileNameAndParentFrn> _directories = new Dictionary<ulong, FileNameAndParentFrn>();

        public Dictionary<ulong, FileNameAndParentFrn> Directories
        {
            get => _directories;
            set => _directories = value;
        }

        private IntPtr _changeJournalRootHandle;
        private string _drive;

        public string Drive
        {
            get => _drive;
            set => _drive = value.Replace(@"\", "");
        }

        #region DllImports and Constants

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
        public const int INVALID_HANDLE_VALUE = -1;
        public const uint FSCTL_QUERY_USN_JOURNAL = 0x000900f4;
        public const uint FSCTL_ENUM_USN_DATA = 0x000900b3;
        public const uint FSCTL_CREATE_USN_JOURNAL = 0x000900e7;

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess,
                                                  uint dwShareMode, IntPtr lpSecurityAttributes,
                                                  uint dwCreationDisposition, uint dwFlagsAndAttributes,
                                                  IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileInformationByHandle(IntPtr hFile,
                                                                     out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(IntPtr hDevice,
                                                      uint dwIoControlCode,
                                                      IntPtr lpInBuffer, int nInBufferSize,
                                                      out USN_JOURNAL_DATA lpOutBuffer, int nOutBufferSize,
                                                      out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(IntPtr hDevice,
                                                      uint dwIoControlCode,
                                                      IntPtr lpInBuffer, int nInBufferSize,
                                                      IntPtr lpOutBuffer, int nOutBufferSize,
                                                      out uint lpBytesReturned, IntPtr lpOverlapped);

        [DllImport("kernel32.dll")]
        public static extern void ZeroMemory(IntPtr ptr, int size);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct USN_JOURNAL_DATA
        {
            public ulong UsnJournalID;
            public long FirstUsn;
            public long NextUsn;
            public long LowestValidUsn;
            public long MaxUsn;
            public ulong MaximumSize;
            public ulong AllocationDelta;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MFT_ENUM_DATA
        {
            public ulong StartFileReferenceNumber;
            public long LowUsn;
            public long HighUsn;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CREATE_USN_JOURNAL_DATA
        {
            public ulong MaximumSize;
            public ulong AllocationDelta;
        }

        public class USN_RECORD
        {
            public uint RecordLength;
            public ulong FileReferenceNumber;
            public ulong ParentFileReferenceNumber;
            public uint FileAttributes;
            public int FileNameLength;
            public int FileNameOffset;
            public string FileName = string.Empty;

            private const int FR_OFFSET = 8;
            private const int PFR_OFFSET = 16;
            private const int FA_OFFSET = 52;
            private const int FNL_OFFSET = 56;
            private const int FN_OFFSET = 58;

            public USN_RECORD(IntPtr p)
            {
                RecordLength = (uint)Marshal.ReadInt32(p);
                FileReferenceNumber = (ulong)Marshal.ReadInt64(p, FR_OFFSET);
                ParentFileReferenceNumber = (ulong)Marshal.ReadInt64(p, PFR_OFFSET);
                FileAttributes = (uint)Marshal.ReadInt32(p, FA_OFFSET);
                FileNameLength = Marshal.ReadInt16(p, FNL_OFFSET);
                FileNameOffset = Marshal.ReadInt16(p, FN_OFFSET);
                FileName = Marshal.PtrToStringUni(new IntPtr(p.ToInt32() + FileNameOffset), FileNameLength / sizeof(char));
            }
        }

        #endregion

        public void EnumerateVolume(out Dictionary<ulong, FileNameAndParentFrn> files, string[] fileExtensions)
        {
            files = new Dictionary<ulong, FileNameAndParentFrn>();
            IntPtr medBuffer = IntPtr.Zero;
            try
            {
                GetRootFrnEntry();
                GetRootHandle();

                CreateChangeJournal();

                SetupMFT_Enum_DataBuffer(ref medBuffer);
                EnumerateFiles(medBuffer, ref files, fileExtensions);
            }
            catch (Exception e)
            {
                //	Log.Info(e.Message, e);
                Exception innerException = e.InnerException;
                while (innerException != null)
                {
                    //		Log.Info(innerException.Message, innerException);
                    innerException = innerException.InnerException;
                }
                throw new ApplicationException("Error in EnumerateVolume()", e);
            }
            finally
            {
                if (_changeJournalRootHandle.ToInt32() != MFTReader.INVALID_HANDLE_VALUE)
                {
                    MFTReader.CloseHandle(_changeJournalRootHandle);
                }
                if (medBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(medBuffer);
                }
            }
        }

        private void GetRootFrnEntry()
        {
            string driveRoot = string.Concat("\\\\.\\", _drive);
            driveRoot = string.Concat(driveRoot, Path.DirectorySeparatorChar);
            IntPtr hRoot = MFTReader.CreateFile(driveRoot,
                0,
                MFTReader.FILE_SHARE_READ | MFTReader.FILE_SHARE_WRITE,
                IntPtr.Zero,
                MFTReader.OPEN_EXISTING,
                MFTReader.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

            if (hRoot.ToInt32() != MFTReader.INVALID_HANDLE_VALUE)
            {
                MFTReader.BY_HANDLE_FILE_INFORMATION fi = new MFTReader.BY_HANDLE_FILE_INFORMATION();
                bool bRtn = MFTReader.GetFileInformationByHandle(hRoot, out fi);
                if (bRtn)
                {
                    ulong fileIndexHigh = fi.FileIndexHigh;
                    ulong indexRoot = (fileIndexHigh << 32) | fi.FileIndexLow;

                    FileNameAndParentFrn f = new FileNameAndParentFrn(driveRoot, 0);
                    _directories.Add(indexRoot, f);
                }
                else
                {
                    throw new IOException("GetFileInformationbyHandle() returned invalid handle",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                }
                MFTReader.CloseHandle(hRoot);
            }
            else
            {
                throw new IOException("Unable to get root frn entry", new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        private void GetRootHandle()
        {
            string vol = string.Concat("\\\\.\\", _drive);
            _changeJournalRootHandle = MFTReader.CreateFile(vol,
                 MFTReader.GENERIC_READ | MFTReader.GENERIC_WRITE,
                 MFTReader.FILE_SHARE_READ | MFTReader.FILE_SHARE_WRITE,
                 IntPtr.Zero,
                 MFTReader.OPEN_EXISTING,
                 0,
                 IntPtr.Zero);
            if (_changeJournalRootHandle.ToInt32() == MFTReader.INVALID_HANDLE_VALUE)
            {
                throw new IOException("CreateFile() returned invalid handle",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        public string GetFullPath(FileNameAndParentFrn frn)
        {
            string address = "";
            while (frn.ParentFrn != 0)
            {
                address = Path.Combine(frn.Name, address);
                frn = _directories[frn.ParentFrn];
            }

            return Drive + @"\" + address;
        }

        public unsafe void EnumerateFiles(IntPtr medBuffer, ref Dictionary<ulong, FileNameAndParentFrn> files, string[] fileExtensions)
        {
            IntPtr pData = Marshal.AllocHGlobal(sizeof(ulong) + 0x10000);
            MFTReader.ZeroMemory(pData, sizeof(ulong) + 0x10000);
            uint outBytesReturned = 0;

            while (false != MFTReader.DeviceIoControl(_changeJournalRootHandle, MFTReader.FSCTL_ENUM_USN_DATA, medBuffer,
                                    sizeof(MFTReader.MFT_ENUM_DATA), pData, sizeof(ulong) + 0x10000, out outBytesReturned,
                                    IntPtr.Zero))
            {
                IntPtr pUsnRecord = new IntPtr(pData.ToInt32() + sizeof(long));
                while (outBytesReturned > 60)
                {
                    MFTReader.USN_RECORD usn = new MFTReader.USN_RECORD(pUsnRecord);
                    if (0 != (usn.FileAttributes & MFTReader.FILE_ATTRIBUTE_DIRECTORY))
                    {
                        //  
                        // handle directories  
                        //  
                        if (!_directories.ContainsKey(usn.FileReferenceNumber))
                        {
                            _directories.Add(usn.FileReferenceNumber,
                                new FileNameAndParentFrn(usn.FileName, usn.ParentFileReferenceNumber));
                        }
                        else
                        {   // this is debug code and should be removed when we are certain that  
                            // duplicate frn's don't exist on a given drive.  To date, this exception has  
                            // never been thrown.  Removing this code improves performance....  
                            throw new Exception(string.Format("Duplicate FRN: {0} for {1}",
                                usn.FileReferenceNumber, usn.FileName));
                        }
                    }
                    else
                    {
                        //   
                        // handle files  
                        //  

                        // at this point we could get the * for the extension
                        bool add = true;
                        bool fullpath = false;
                        if (fileExtensions != null && fileExtensions.Length != 0)
                        {
                            if (fileExtensions[0].ToString() == "*")
                            {
                                add = true;
                                fullpath = true;
                            }
                            else
                            {
                                add = false;
                                string s = Path.GetExtension(usn.FileName);
                                foreach (string extension in fileExtensions)
                                {
                                    if (0 == string.Compare(s, extension, true))
                                    {
                                        add = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (add)
                        {
                            if (fullpath)
                            {
                                if (!files.ContainsKey(usn.FileReferenceNumber))
                                {
                                    files.Add(usn.FileReferenceNumber,
                                        new FileNameAndParentFrn(usn.FileName, usn.ParentFileReferenceNumber));
                                }
                                else
                                {
                                    FileNameAndParentFrn frn = files[usn.FileReferenceNumber];
                                    if (0 != string.Compare(usn.FileName, frn.Name, true))
                                    {
                                        //	Log.InfoFormat(
                                        //	"Attempt to add duplicate file reference number: {0} for file {1}, file from index {2}",
                                        //	usn.FileReferenceNumber, usn.FileName, frn.Name);
                                        throw new Exception(string.Format("Duplicate FRN: {0} for {1}",
                                            usn.FileReferenceNumber, usn.FileName));
                                    }
                                }
                            }
                            else
                            {
                                if (!files.ContainsKey(usn.FileReferenceNumber))
                                {
                                    files.Add(usn.FileReferenceNumber,
                                        new FileNameAndParentFrn(usn.FileName, usn.ParentFileReferenceNumber));
                                }
                                else
                                {
                                    FileNameAndParentFrn frn = files[usn.FileReferenceNumber];
                                    if (0 != string.Compare(usn.FileName, frn.Name, true))
                                    {
                                        //	Log.InfoFormat(
                                        //	"Attempt to add duplicate file reference number: {0} for file {1}, file from index {2}",
                                        //	usn.FileReferenceNumber, usn.FileName, frn.Name);
                                        throw new Exception(string.Format("Duplicate FRN: {0} for {1}",
                                            usn.FileReferenceNumber, usn.FileName));
                                    }
                                }
                            }
                        }
                    }
                    pUsnRecord = new IntPtr(pUsnRecord.ToInt32() + usn.RecordLength);
                    outBytesReturned -= usn.RecordLength;
                }
                Marshal.WriteInt64(medBuffer, Marshal.ReadInt64(pData, 0));
            }
            Marshal.FreeHGlobal(pData);
        }

        private unsafe void CreateChangeJournal()
        {
            // This function creates a journal on the volume. If a journal already  
            // exists this function will adjust the MaximumSize and AllocationDelta  
            // parameters of the journal  
            ulong MaximumSize = 0x800000;
            ulong AllocationDelta = 0x100000;
            MFTReader.CREATE_USN_JOURNAL_DATA cujd;
            cujd.MaximumSize = MaximumSize;
            cujd.AllocationDelta = AllocationDelta;

            int sizeCujd = Marshal.SizeOf(cujd);
            IntPtr cujdBuffer = Marshal.AllocHGlobal(sizeCujd);
            MFTReader.ZeroMemory(cujdBuffer, sizeCujd);
            Marshal.StructureToPtr(cujd, cujdBuffer, true);

            bool fOk = MFTReader.DeviceIoControl(_changeJournalRootHandle, MFTReader.FSCTL_CREATE_USN_JOURNAL,
                cujdBuffer, sizeCujd, IntPtr.Zero, 0, out uint cb, IntPtr.Zero);
            if (!fOk)
            {
                throw new IOException("DeviceIoControl() returned false", new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        private unsafe void SetupMFT_Enum_DataBuffer(ref IntPtr medBuffer)
        {
            MFTReader.USN_JOURNAL_DATA ujd = new MFTReader.USN_JOURNAL_DATA();

            bool bOk = MFTReader.DeviceIoControl(_changeJournalRootHandle,                           // Handle to drive  
                MFTReader.FSCTL_QUERY_USN_JOURNAL,   // IO Control Code  
                IntPtr.Zero,                // In Buffer  
                0,                          // In Buffer Size  
                out ujd,                    // Out Buffer  
                sizeof(MFTReader.USN_JOURNAL_DATA),  // Size Of Out Buffer  
                out uint bytesReturned,          // Bytes Returned  
                IntPtr.Zero);               // lpOverlapped  
            if (bOk)
            {
                MFTReader.MFT_ENUM_DATA med;
                med.StartFileReferenceNumber = 0;
                med.LowUsn = 0;
                med.HighUsn = ujd.NextUsn;
                int sizeMftEnumData = Marshal.SizeOf(med);
                medBuffer = Marshal.AllocHGlobal(sizeMftEnumData);
                MFTReader.ZeroMemory(medBuffer, sizeMftEnumData);
                Marshal.StructureToPtr(med, medBuffer, true);
            }
            else
            {
                throw new IOException("DeviceIoControl() returned false", new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }
    }

}
#endif