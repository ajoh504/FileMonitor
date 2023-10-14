namespace Services.Dto
{
    /// <summary>
    /// A public interface to expose any data transfer object containing a folder or file path.
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
