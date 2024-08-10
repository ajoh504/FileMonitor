namespace FileMonitorBackgroundService.Udp
{
    public interface IUdpMessage
    {
        public int ChangedFileCount { get; set; }
    }
}
