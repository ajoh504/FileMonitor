using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccessLayer.Entities
{
    /// <summary>
    /// An Entity class serving as a mapping table. This table establishes the relationship between the 
    /// <c>SourceFolder</c> and <c>SourceFile</c> Entities. It stores a <c>SourceFolderId</c> and a <c>SourceFileId</c>
    /// to show that the file is contained within the folder.
    /// </summary>
    /// <remarks>
    /// The file may be stored directly in the folder, or it may be contained within a subfolder of the parent folder.
    /// </remarks>
    [Table("folder_file_mapping")]
    public class FolderFileMapping
    {
        /// <summary>
        /// The database primary key. 
        /// </summary>
        /// <remarks>
        /// Use to remove Entities from the database.
        /// </remarks>
        public int Id { get; set; }

        /// <summary>
        /// The database primary key for the given <c>SourceFolder</c>.
        /// </summary>
        public int SourceFolderId { get; set; }

        /// <summary>
        /// A navigation property to establish the relationship to the <c>SourceFolder</c> Entity.
        /// </summary>
        public SourceFolder? SourceFolder { get; set; }

        /// <summary>
        /// The database primary key for the given <c>SourceFile</c>.
        /// </summary>
        public int SourceFileId { get; set; }

        /// <summary>
        /// A navigation property to establish the relationship to the <c>SourceFile</c> Entity.
        /// </summary>
        public SourceFile? SourceFile { get; set; }
    }
}
