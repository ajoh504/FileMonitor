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

        protected virtual void OnFilesChanged(FilesChangedEventArgs e)
        {
            FilesChanged?.Invoke(this, e);
        }

        public event EventHandler<FilesChangedEventArgs> FilesChanged;

        /// <summary>
        /// The <see cref="SourceFileService"/> class constructor. 
        /// </summary>
        /// <param name="sourceFileRepository"> An instance of <see cref="ISourceFileRepository"/> which provides database access. </param>
        public SourceFileService(ISourceFileRepository sourceFileRepository)
        {
            _sourceFileRepository = sourceFileRepository;
        }

        /// <summary>
        /// Returns all source file paths from the database.
        /// </summary>
        public List<SourceFileDto> GetFiles()
        {
            List<SourceFileDto> result = _sourceFileRepository.GetRange(
                s => true,
                // Create a new Dto for each Entity, and assign the Dto property values from the Entity properties
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
        /// Returns true if the path exists in the database, false otherwise.
        /// </summary>
        public bool PathExists(string path)
        {
            return _sourceFileRepository.Exists(s => s.Path == path);
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
