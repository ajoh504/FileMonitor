namespace Services.Dto
{
    /// <summary>
    /// A public interface for interacting with an ignorable folder DTO.
    /// </summary>
    public interface IIgnorableFolderDto
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
        public string? Name { get; set; }
    }
}
