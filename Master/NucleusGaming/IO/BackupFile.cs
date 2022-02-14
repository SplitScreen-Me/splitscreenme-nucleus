namespace Nucleus.Gaming
{
    public struct BackupFile
    {
        public string Source { get; set; }
        public string BackupPath { get; set; }

        public BackupFile(string source, string backup)
            : this()
        {
            Source = source;
            BackupPath = backup;
        }
    }
}
