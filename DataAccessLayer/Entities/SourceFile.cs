using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    /// <summary>
    /// An Entity class for the source_file table. These records store paths to all files being monitored by the 
    /// program.
    /// </summary>
    [Table("source_file")]
    public class SourceFile
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The full path to where the file is stored.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// A SHA1 hash for this instance of the file.
        /// </summary>
        /// <remarks>
        /// The hash is generated when the file is copied to a backup location. If the current hash differs from the 
        /// stored hash, then the file has changed and the new version needs to be backed up.
        /// </remarks>
        public string Hash { get; set; }

        /// <summary>
        /// A boolean value to determine if the file has been changed or modified since the last backup.
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// If set to true, the file was added from a source folder. If set to false, the file was added to the program
        /// individually.
        /// </summary>
        public bool FromSourceFolder { get; set; }
    }
}
