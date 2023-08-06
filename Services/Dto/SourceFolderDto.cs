namespace Services.Dto
{
    /// <summary>
    /// A data transfer object related to the SourceFolder Entity. Provides a full file path for any folder monitored by the program.
    /// </summary>
    public class SourceFolderDto
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The full file path to where the folder is stored.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Determines whether or not the program should monitor every sub directory within this folder.
        /// </summary>
        public bool MonitorAllSubDirectories { get; set; }
    }
}
