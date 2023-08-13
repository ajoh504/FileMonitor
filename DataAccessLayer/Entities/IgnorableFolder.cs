using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    /// <summary>
    /// An Entity class for the ignorable_folder table. These records store folder names that should be ignored by the program.
    /// </summary>
    [Table("ignorable_folder")]
    public class IgnorableFolder
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The name of the folder/directory to be ignored.
        /// </summary>
        public string Name { get; set; }
    }
}
