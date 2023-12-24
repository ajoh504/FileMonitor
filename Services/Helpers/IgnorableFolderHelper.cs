using Services.Dto;

namespace Services.Helpers
{
    /// <summary>
    /// A static helper class for the <see cref="IIgnorableFolderDto"/> and <see cref="IgnorableFolderService"/> classes.
    /// </summary>
    public static class IgnorableFolderHelper
    {
        /// <summary>
        /// Returns false if the path is contained within an ignorable folder, true otherwise.
        /// </summary>
        public static bool NotIgnorable(string path, IEnumerable<IIgnorableFolderDto> ignorableFolders)
        {
            foreach (var folder in ignorableFolders)
            {
                if (path.Split(Path.DirectorySeparatorChar).Contains(folder.Name)) return false;
            }
            return true;
        }
    }
}
