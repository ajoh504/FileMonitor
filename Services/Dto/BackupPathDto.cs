namespace Services.Dto
{
    /// <summary>
    /// A data transfer object related to the BackupPath Entity. Provides a full directory path for the backup location
    /// as selected by the user.
    /// </summary>
    internal class BackupPathDto : IBackupPathDto
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
        public string? Path { get; set; }

        /// <summary>
        /// A boolean value to determine if the path has been selected by the user.
        /// </summary>
        /// <remarks>
        /// Data binding is mapped to a checkbox in the UI.
        /// </remarks>
        public bool IsSelected { get; set; }

        /// <summary>
        /// An overridden version of <see cref="Equals(object?)"/>. This method does a value comparison for the 
        /// <see cref="BackupPathDto"/> properties to determine if the two objects are equal.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="object"/> to compare to this instance of <see cref="BackupPathDto"/>.
        /// </param>
        /// <returns> True if the objects are equal, false otherwise. </returns>
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(obj, this)) return true;
            if (obj is BackupPathDto dto) return dto.Id == Id && dto.Path == Path;
            return false;
        }

        /// <summary>
        /// An overridden version of <see cref="object.GetHashCode"/>. This method returns a hash based on the 
        /// <see cref="BackupPathDto.Path"/> property.
        /// </summary>
        /// <remarks>
        /// The Microsoft docs recommend overriding <see cref="object.GetHashCode"/> whenever overriding <see cref=
        /// "Equals(object?)"/>. <see href= 
        /// "https://learn.microsoft.com/en-us/dotnet/api/system.object.equals?view=net-7.0">See this link for more 
        /// information.</see>
        /// </remarks>
        /// <returns> A 32-bit signed integer hash code. </returns>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
