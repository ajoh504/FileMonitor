using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    /// <summary>
    /// An Entity class for the source_folder table. These records store paths to all folders being monitored by the program.
    /// </summary>
    [Table("source_folder")]
    public class SourceFolder
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The full path to where the folder is stored.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Determines whether or not the program should monitor every sub directory within this folder.
        /// </summary>
        public bool MonitorAllSubDirectories { get; set; }
    }
}
