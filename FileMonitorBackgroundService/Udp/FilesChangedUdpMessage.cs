namespace FileMonitorBackgroundService.Udp
{
    public class FilesChangedUdpMessage : IUdpMessage
    {
        public int ChangedFileCount { get; set; }

        public FilesChangedUdpMessage(int changedFileCount)
        {
            ChangedFileCount = changedFileCount;
        }
    }
}
