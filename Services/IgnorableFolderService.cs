using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Services.Dto;

namespace Services
{
    /// <summary>
    /// A service class offering database access to the IgnorableFolder Entity. This class stores a repository, and
    /// offers data transfer objects for updating the ViewModel.
    /// </summary>
    public class IgnorableFolderService : DisposableService
    {
        IIgnorableFolderRepository _ignorableFolderRepository;

        /// <summary>
        /// The <see cref="IgnorableFolderService"/> class constructor.
        /// </summary>
        /// <param name="ignorableFolderRepository">
        /// An instance of <see cref="IIgnorableFolderRepository"/> which provides database access.
        /// </param>
        public IgnorableFolderService(IIgnorableFolderRepository ignorableFolderRepository)
        {
            _ignorableFolderRepository = ignorableFolderRepository;
        }

        /// <summary>
        /// Returns all ignorable folders from the database.
        /// </summary>
        public List<IgnorableFolderDto> Get()
        {
            return _ignorableFolderRepository.GetRange(
                f => true,
                f => new IgnorableFolderDto
                {
                    Id = f.Id,
                    Name = f.Name
                },
                f => f.Id
                ); ;
        }

        /// <summary>
        /// Adds an ignorable folder to the database.
        /// </summary>
        /// <param name="name"> The name of the ignorable folder to add. </param>
        /// <returns> An ignorable folder DTO object for updating the UI. </returns>
        public IgnorableFolderDto Add(string name)
        {
            IgnorableFolder entity = new IgnorableFolder
            {
                Name = name
            };

            _ignorableFolderRepository.Add(entity);
            _ignorableFolderRepository.SaveChanges();

            return new IgnorableFolderDto
            {
                Name = name,
                Id = entity.Id
            };
        }

        /// <summary>
        /// Remove a range of ignorable folders from the database.
        /// </summary>
        /// <param name="id"> The Ids for each ignorable folder to remove. </param>
        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                _ignorableFolderRepository.Remove(new IgnorableFolder
                {
                    Id = id
                });
            }
            _ignorableFolderRepository.SaveChanges();
        }

        /// <summary>
        /// Ensures that the service objects are properly disposed. Also calls <c>Dispose</c> on the repository objects.
        /// </summary>
        /// <param name="disposing">
        /// Signifies that the object is not being disposed directly from the finalizer.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            _ignorableFolderRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}
