using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Services.Dto;

namespace Services
{
    /// <summary>
    /// A service class offering database access to the BackupPath Entity. This class stores a repository, and offers
    /// data transfer objects for updating the ViewModel.
    /// </summary>
    public class BackupPathService : DisposableService
    {
        private IBackupPathRepository _repository;
        
        /// <summary>
        /// The <see cref="BackupPathService"/> class constructor.
        /// </summary>
        /// <param name="repository">
        /// An instance of <see cref="IBackupPathRepository"/> which provides database access.
        /// </param>
        public BackupPathService(IBackupPathRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns all backup directory paths from the database.
        /// </summary>
        public List<BackupPathDto> GetDirectories()
        {
            List<BackupPathDto> result = _repository.GetRange(
                f => true,
                // Create a new Dto for each Entity, and assign the Dto property values from the Entity properties
                f => new BackupPathDto
                {
                    Id = f.Id,
                    Path = f.Path,
                    IsSelected = f.IsSelected
                },
                f => f.Id);
            return result;
        }

        /// <summary>
        /// Adds a backup path directory to the database.
        /// </summary>
        /// <param name="path"> The backup path to add to the database. </param>
        /// <returns> A backup path DTO object for updating the UI. </returns>
        public BackupPathDto Add(string path)
        {
            BackupPath entity = new BackupPath
            {
                Path = path
            };

            _repository.Add(entity);
            _repository.SaveChanges();

            return new BackupPathDto
            {
                Id = entity.Id,
                Path = entity.Path
            };
        }

        /// <summary>
        /// Remove a range of backup path directories from the database.
        /// </summary>
        /// <param name="ids"> The Ids for each backup path to be removed. </param>
        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                _repository.Remove(new BackupPath
                {
                    Id = id
                });
            }
            _repository.SaveChanges();
        }

        /// <summary>
        /// Returns true if the path exists in the database, false otherwise.
        /// </summary>
        public bool PathExists(string path)
        {
            return _repository.Exists(obj => obj.Path == path);
        }

        /// <summary>
        /// Updates the Entity properties in the database using the provided DTO object.
        /// </summary>
        /// <param name="dto"> The DTO used to update the Entity. </param>
        /// <param name="updatePath"> This parameter must be set to true in order to update the Path property. </param>
        /// <param name="updateIsSelected">
        /// This parameter must be set to true in order to update the IsSelected property.
        /// </param>
        public void Update(BackupPathDto dto, bool updatePath, bool updateIsSelected)
        {
            BackupPath entity = _repository.FirstOrDefault(f => f.Id == dto.Id, asNoTracking: false);
            if(entity == null) return;
            if(updatePath) entity.Path = dto.Path;
            if(updateIsSelected)entity.IsSelected = dto.IsSelected;
            _repository.SaveChanges();
        }

        /// <summary>
        /// Get a list of all backup paths that have been moved, deleted, or renamed since being added to the database.
        /// </summary>
        public List<BackupPathDto> GetMovedOrRenamedPaths()
        {
            List<BackupPathDto> paths = GetDirectories();
            List<BackupPathDto> result = new List<BackupPathDto>();
            foreach (BackupPathDto path in paths)
            {
                if (!Directory.Exists(path.Path)) result.Add(path);
            }
            return result;
        }

        /// <summary>
        /// Ensures that the service object is properly disposed. Also calls <c>Dispose</c> on the repository object.
        /// </summary>
        /// <param name="disposing">
        /// Signifies that the object is not being disposed directly from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _repository.Dispose();
            base.Dispose(disposing);
        }
    }
}
