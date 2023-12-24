namespace Services.Dto
{
    /// <summary>
    /// A public interface for interacting with any DTO that contains an ID and a file path.
    /// </summary>
    public interface IPathDto
    {
        /// <summary>
        /// Database ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full path to the folder or file. 
        /// </summary>
        public string? Path { get; set; }
    }
}
