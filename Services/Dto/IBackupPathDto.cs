namespace Services.Dto
{
    /// <summary>
    /// A public interface for interacting with a backup path DTO. 
    /// </summary>
    public interface IBackupPathDto : IPathDto
    {
        /// <summary>
        /// A boolean value to determine if the path has been selected by the user.
        /// </summary>
        /// <remarks>
        /// Data binding is mapped to a checkbox in the UI.
        /// </remarks>
        public bool IsSelected { get; set; }
    }
}
