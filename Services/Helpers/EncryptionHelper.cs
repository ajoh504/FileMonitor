using System.Security.Cryptography;
using DataAccessLayer.Entities;

namespace Services.Helpers
{
    /// <summary>
    /// A static helper class for generating a <see cref="SHA1"/> hash. The hash is stored as a database record with
    /// the <see cref="SourceFile"/> Entity. 
    /// </summary>
    /// <remarks>
    /// The program compares the stored hash to the current hash to determine if the file has changed since the last
    /// time it was copied to a backup location.
    /// </remarks>
    public static class EncryptionHelper
    {
        /// <summary>
        /// Get a <see cref="SHA1"/> hash for the given file.
        /// </summary>
        /// <param name="file"> A fully qualified file path to retrieve a hash for. </param>
        /// <returns> A <see cref="SHA1"/> hash for the provided file. </returns>
        public static string GetHash(string file)
        {
            using SHA1 sha1 = SHA1.Create();
            using FileStream stream = File.OpenRead(file);
            byte[]? hash = sha1.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
