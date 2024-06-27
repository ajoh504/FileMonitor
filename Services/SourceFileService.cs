using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Services.Dto;
using Services.Helpers;

namespace Services
{
    /// <summary>
    /// A service class offering database access to the SourceFile Entity. This class stores a repository, and offers data transfer objects for updating the ViewModel.
    /// </summary>
    public class SourceFileService : DisposableService
    {
        private ISourceFileRepository _sourceFileRepository;
        private int _lastId;

        /// <summary>
        /// The <see cref="SourceFileService"/> class constructor. 
        /// </summary>
        /// <param name="sourceFileRepository"> An instance of <see cref="ISourceFileRepository"/> which provides database access. </param>
        public SourceFileService(ISourceFileRepository sourceFileRepository)
        {
            _sourceFileRepository = sourceFileRepository;
            _lastId = -1;
        }

        /// <summary>
        /// Returns a paginated list of file paths from the database.
        /// </summary>
        public List<SourceFileDto> GetFiles()
        {
            List<SourceFileDto> result = _sourceFileRepository.GetRangePaginated(
                s => true,
                // Create a new Dto for each Entity, and assign the Dto property values from the Entity properties
                s => new SourceFileDto
                {
                    Id = s.Id,
                    Path = s.Path
                },
                _lastId
            );

            SourceFileDto last = result.Last();
            _lastId = last.Id;

            return result;
        }

        /// <summary>
        /// Returns all source file paths from the database if the IsModified property is set to true.
        /// </summary>
        public List<SourceFileDto> GetModifiedFiles()
        {
            RefreshModifiedFilePaths();
            List<SourceFileDto> result = _sourceFileRepository.GetRange(
                s => s.IsModified == true,
                s => new SourceFileDto
                {
                    Id = s.Id,
                    Path = s.Path
                },
                s => s.Path
            );
            return result;
        }

        /// <summary>
        /// Adds a source file path to the database.
        /// </summary>
        /// <param name="path"> The source file path to add to the database. </param>
        /// <param name="fromSourceFolder"> Set to true if the file was added from a folder. Otherwise, set to false. </param>
        /// <returns> A source file DTO object for updating the UI. </returns>
        public SourceFileDto Add(string path, bool fromSourceFolder)
        {
            SourceFile entity = new SourceFile
            {
                Path = path,
                Hash = EncryptionHelper.GetHash(path),
                IsModified = true,
                FromSourceFolder = fromSourceFolder
            };

            _sourceFileRepository.Add(entity);
            _sourceFileRepository.SaveChanges();

            return new SourceFileDto
            {
                Id = entity.Id,
                Path = entity.Path
            };
        }

        /// <summary>
        /// Remove a range of source file paths from the database.
        /// </summary>
        /// <param name="ids"> The Ids for each source file path to be removed. </param>
        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                _sourceFileRepository.Remove(
                    new SourceFile
                    {
                        Id = id
                    }
                );
            }
            _sourceFileRepository.SaveChanges();
        }

        /// <summary>
        /// Set the <see cref="SourceFile.IsModified"/> property to false on all the specified files. This class should be called when the files are first copied to a backup location.
        /// </summary>
        /// <param name="ids"> The Ids for each file where the <see cref="SourceFile.IsModified"/> property should be set to false. </param>
        public void ResetIsModifiedFlag(IEnumerable<int> ids)
        {
            _sourceFileRepository.Update(
                s => ids.Contains(s.Id), 
                s => s.IsModified = false); 
            _sourceFileRepository.SaveChanges();
        }

        /// <summary>
        /// Returns true if the path exists in the database, false otherwise.
        /// </summary>
        public bool PathExists(string path)
        {
            return _sourceFileRepository.Exists(s => s.Path == path);
        }

        /// <summary>
        /// Updates the hash for all source files based on their Ids.
        /// </summary>
        /// <param name="ids"> A list of Ids for each file requiring an updated hash. </param>
        public void UpdateHashesToCurrent(List<int> ids)
        {
            _sourceFileRepository.Update(
                s => ids.Contains(s.Id),
                s => s.Hash = EncryptionHelper.GetHash(s.Path));
            _sourceFileRepository.SaveChanges();
        }

        /// <summary>
        /// Get a list of all files that have been moved, deleted, or renamed since being added to the database.
        /// </summary>
        public List<SourceFileDto> GetMovedOrRenamedFiles()
        {
            List<SourceFileDto> files = GetFiles();
            List<SourceFileDto> result = new List<SourceFileDto>();
            foreach (SourceFileDto file in files)
            {
                if (!File.Exists(file.Path)) result.Add(file);
            }
            return result;
        }

        // If FileIsUpdated() returns true, then set IsModified to true, and call SaveChanges on the repository.
        private void RefreshModifiedFilePaths()
        {
            List<SourceFile> files = _sourceFileRepository.GetRange(s => true);
            foreach (SourceFile file in files) 
            {
                if (FileIsUpdated(file.Path))
                {
                    file.IsModified = true;
                }
            }
            _sourceFileRepository.SaveChanges();
        }

        // Return true if the stored hash is different from the current hash. Otherwise return false.
        private bool FileIsUpdated(string path)
        {
            SourceFile? sourceFile = _sourceFileRepository.FirstOrDefault(s => s.Path == path);
            if (sourceFile == null) return false;
            // If file does not exist, it has been moved or renamed. Return false in this instance
            // because VerifyMovedOrRenamed() will find those files and add them to the UI.
            if (!File.Exists(sourceFile.Path)) return false;
            return EncryptionHelper.GetHash(path) != sourceFile.Hash;
        }

        /// <summary>
        /// Ensures that the service object is properly disposed. Also calls <c>Dispose</c> on the repository object.
        /// </summary>
        /// <param name="disposing">
        /// Signifies that the object is not being disposed directly from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _sourceFileRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}
