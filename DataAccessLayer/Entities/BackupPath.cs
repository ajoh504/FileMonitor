using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    /// <summary>
    /// An entity class for the backup_path table. These records store full directory paths for the backup location as
    /// selected by the user.
    /// </summary>
    [Table("backup_path")]
    public class BackupPath
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The full file path to the backup directory location.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// A boolean value to determine if the path has been selected by the user.
        /// </summary>
        /// <remarks>
        /// Data binding is mapped to a checkbox in the UI.
        /// </remarks>
        public bool IsSelected { get; set; }
    }
}
