namespace Services.Dto
{
    /// <summary>
    /// A data transfer object related to the IgnorableFolder entity. Provides a folder name to be ignored by the
    /// program.
    /// </summary>
    public class IgnorableFolderDto : IPathDto
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
        public string? Path { get; set; }
    }
}
