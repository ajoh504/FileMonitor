namespace Services
{
    public class FilesChangedEventArgs : EventArgs
    {
        public int ChangedFileCount { get; private set; }

        public FilesChangedEventArgs(int changedFileCount)
        {
            ChangedFileCount = changedFileCount;
        }
    }
}
