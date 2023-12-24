namespace Services.Dto
{
    /// <summary>
    /// A public interface for interacting with a source folder DTO. 
    /// </summary>
    public interface ISourceFolderDto : IPathDto
    {
        /// <summary>
        /// Determines whether or not the program should monitor every sub directory within this folder.
        /// </summary>
        public bool MonitorAllSubDirectories { get; set; }
    }
}
