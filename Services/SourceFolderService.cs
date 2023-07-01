﻿using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;

namespace Services
{
    /// <summary>
    ///  A service class offering database access to the FolderFileMapping Entity. This class stores a repository, and creates the appropriate mapping between the folders and the files contained within them.
    /// </summary>
    public class SourceFolderService : DisposableService
    {
        ISourceFolderRepository _sourceFolderRepository;
        IFolderFileMappingRepository _folderFileMappingRepository;

        /// <summary>
        /// The <see cref="SourceFolderService"/> class constructor.
        /// </summary>
        /// <param name="sourceFolderRepository"> An instance of the <see cref="ISourceFolderRepository"/> which provides database access. </param>
        /// <param name="folderFileMappingRepository"> An instance of the <see cref="IFolderFileMappingRepository"/> which provides database access. </param>
        public SourceFolderService(ISourceFolderRepository sourceFolderRepository, IFolderFileMappingRepository folderFileMappingRepository)
        {
            _sourceFolderRepository = sourceFolderRepository;
            _folderFileMappingRepository = folderFileMappingRepository;
        }

        /// <summary>
        /// Adds a monitored folder to the database. This method ensures appropriate mapping from the source folder to all child files contained within it. <see cref="SourceFileService.Add(string)"/> must be called first on all new file paths before this method is called. This ensures that the file IDs are created, allowing for the folders and files to be mapped appropriately.
        /// </summary>
        /// <param name="directoryPath"> The folder to add to the database. </param>
        /// <param name="filePaths"> An array of all newly added file paths. </param>
        public void Add(string directoryPath, string[] filePaths)
        {
            _sourceFolderRepository.Add(new SourceFolder
            {
                Path = directoryPath,
            });
            _sourceFolderRepository.SaveChanges();
        }

        private void AddFolderFileMapping(string[] filePaths)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                _sourceFolderRepository.Remove(new SourceFolder
                { 
                    Id = id 
                });
            }
            _sourceFolderRepository.SaveChanges();
        }
    }
}
