using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Services.Dto;
using Services.Helpers;
using System.IO;

namespace Services
{
    /// <summary>
    ///  A service class offering database access to the FolderFileMapping Entity. This class stores a repository, and creates the appropriate mapping between the folders and the files contained within them.
    /// </summary>
    public class SourceFolderService : DisposableService
    {
        ISourceFolderRepository _sourceFolderRepository;
        IFolderFileMappingRepository _folderFileMappingRepository;
        ISourceFileRepository _sourceFileRepository;

        /// <summary>
        /// The <see cref="SourceFolderService"/> class constructor.
        /// </summary>
        /// <param name="sourceFolderRepository"> An instance of the <see cref="ISourceFolderRepository"/> which provides database access. </param>
        /// <param name="folderFileMappingRepository"> An instance of the <see cref="IFolderFileMappingRepository"/> which provides database access. </param>
        /// <param name="sourceFileRepository"> An instance of the <see cref="ISourceFileRepository"/> which provides database access. </param>
        public SourceFolderService(
            ISourceFolderRepository sourceFolderRepository, 
            IFolderFileMappingRepository folderFileMappingRepository,
            ISourceFileRepository sourceFileRepository)
        {
            _sourceFolderRepository = sourceFolderRepository;
            _folderFileMappingRepository = folderFileMappingRepository;
            _sourceFileRepository = sourceFileRepository;
        }

        /// <summary>
        /// Returns all monitored folders from the database.
        /// </summary>
        public List<SourceFolderDto> GetFolders()
        {
            return _sourceFolderRepository.GetRange(
                sf => true,
                // Create a new Dto for each Entity, and assign the Dto property values from the Entity properties
                sf => new SourceFolderDto
                {
                    Id = sf.Id,
                    Path = sf.Path,
                },
                sf => sf.Path);
        }

        /// <summary>
        /// Get all files stored in the database if they are mapped to a SourceFolder object. 
        /// </summary>
        /// <param name="folderId"> The ID of the folder to search in. </param>
        /// <returns> A list of source files as data transfer objects. </returns>
        public List<SourceFileDto> GetStoredFilesFromFolder(int folderId)
        {
            List<SourceFileDto> result = new List<SourceFileDto>();
            List<FolderFileMapping> mapping = _folderFileMappingRepository.GetRange(
                ffm => ffm.SourceFolderId == folderId);
            foreach (FolderFileMapping map in mapping)
            {
                SourceFileDto? file = _sourceFileRepository.FirstOrDefault(
                    f => f.Id == map.SourceFileId,
                    f => new SourceFileDto
                    {
                        Id = f.Id,
                        Path = f.Path
                    });
                if (file != null) result.Add(file);
            }
            return result;
        }

        /// <summary>
        /// Adds a monitored folder to the database. This method ensures appropriate mapping from the source folder to all children files contained within it. <see cref="SourceFileService.Add(string)"/> must be called first on all new file paths before this method is called. This ensures that the file IDs are created, allowing for the folders and files to be mapped appropriately.
        /// </summary>
        /// <param name="directoryPath"> The folder to add to the database. </param>
        /// <param name="filePaths"> A string list of all children files. </param>
        /// <param name="MonitorAllSubDirectories"> Determines if the program should monitor all sub directories within the parent directory. </param>
        /// <returns> A source folder DTO object for updating the UI. </returns>
        public SourceFolderDto Add(
            string directoryPath, 
            List<string> filePaths,
            bool MonitorAllSubFolders)
        {
            SourceFolder entity = new SourceFolder
            {
                Path = directoryPath,
                MonitorAllSubDirectories = MonitorAllSubFolders
            };
            _sourceFolderRepository.Add(entity);
            _sourceFolderRepository.SaveChanges(); 
            AddFolderFileMapping(filePaths, GetDirectoryId(directoryPath));

            return new SourceFolderDto
            {
                Path = entity.Path,
                Id = entity.Id
            };
        }

        /// <summary>
        /// Remove a range of source folders from the database. This method also removes any source files contained within that folder, as long as the files were not added individually.
        /// </summary>
        /// <param name="ids"> The Ids for each source folder path to be removed. </param>
        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                List<SourceFile> filesToRemove = GetFileEntitiesFromFolder(id);
                _sourceFileRepository.RemoveRange(filesToRemove);
                _sourceFileRepository.SaveChanges();

                _sourceFolderRepository.Remove(new SourceFolder
                {
                    Id = id
                });
            }
            _sourceFolderRepository.SaveChanges();
        }

        /// <summary>
        /// Returns true if any monitored folder contains newly added files, false otherwise. If the method returns true, it will add the files to the database and provide an out parameter containing any files that were added to the database.
        /// </summary>
        /// <param name="newFilesFromFolder"> A list of data transfer objects for values. The values represent all files that have been added to the monitored folder. </param>
        public bool FilesAddedToFolders(
            out List<SourceFileDto>? newFilesFromFolder)
        {
            List<SourceFolderDto> folders = GetFolders();
            bool filesAddedToFolders = false;
            newFilesFromFolder = new List<SourceFileDto>();
            foreach (SourceFolderDto folder in folders)
            {
                List<string> currentFiles = GetCurrentFilesFromFolder(folder);
                List<string> storedFiles = GetPathsFromEntities(GetFileEntitiesFromFolder(folder.Id));
                List<string> filesToMap = new List<string>();
                foreach (string file in currentFiles)
                {
                    if (storedFiles.Contains(file)) continue;
                    else
                    {
                        SourceFileDto? sourceFile = AddFile(file, fromSourceFolder: true);
                        filesToMap.Add(file);
                        if (filesAddedToFolders == false) filesAddedToFolders = true;
                        newFilesFromFolder.Add(sourceFile);
                    }
                }
                AddFolderFileMapping(filesToMap, folder.Id);
            }
            return filesAddedToFolders;
        }

        private SourceFileDto? AddFile(string path, bool fromSourceFolder)
        {
            // Add the file Entity to the database.
            _sourceFileRepository.Add(
                new SourceFile
                {
                    Path = path,
                    Hash = EncryptionHelper.GetHash(path),
                    IsModified = true,
                    FromSourceFolder = fromSourceFolder
                });
            _sourceFileRepository.SaveChanges();
            // Return the file as a data transfer object.
            return _sourceFileRepository.FirstOrDefault(
                f => f.Path == path,
                f => new SourceFileDto
                {
                    Path = f.Path,
                    Id = f.Id
                });
        }

        // This method adds the appropriate mapping. It stores the id of the monitored folder (directoryId).
        // Then it stores the id for each file contained within that folder. The mapping can be used to know
        // the exact files contained within a folder during its first addition to the database. So if files
        // are added to that folder in the future then the program has a way to inform the user. 
        private void AddFolderFileMapping(List<string> filePaths, int directoryId)
        {
            List<SourceFile> filesInMonitoredFolder = GetMonitoredFolderChildrenFiles(filePaths);
            foreach(SourceFile file in filesInMonitoredFolder)
            {
                _folderFileMappingRepository.Add(
                    new FolderFileMapping
                    {
                        SourceFileId = file.Id,
                        SourceFolderId = directoryId
                    }
                );
            }
            _folderFileMappingRepository.SaveChanges();
        }

        // Get all file Entities stored in the database if they are mapped to a SourceFolder object. 
        private List<SourceFile> GetFileEntitiesFromFolder(int folderId)
        {
            List<SourceFile> result = new List<SourceFile>();
            List<FolderFileMapping> mapping = _folderFileMappingRepository.GetRange(
                ffm => ffm.SourceFolderId == folderId);
            foreach (FolderFileMapping map in mapping)
            {
                SourceFile? file = _sourceFileRepository.FirstOrDefault(
                    f => f.Id == map.SourceFileId,
                    asNoTracking: false);
                if (file != null) result.Add(file);
            }
            return result;
        }

        // Get all file paths from the database where the path matches the contents of "filePaths."
        private List<SourceFile> GetMonitoredFolderChildrenFiles(List<string> filePaths)
            => _sourceFileRepository.GetRange(s => filePaths.Contains<string>(s.Path));

        // Get the directory id (folder id) from the given directory path.
        private int GetDirectoryId(string directoryPath) 
            => _sourceFolderRepository.FirstOrDefault(f => f.Path == directoryPath).Id;
        

        // Get all current files from the provided folder. Check the "MonitorAllSubDirectories" 
        // property to determine if the program monitors the top directory only, or all sub directories.
        private List<string> GetCurrentFilesFromFolder(SourceFolderDto folder)
        {
            List<string> fileSystemEntries = new List<string>();
            if (folder.MonitorAllSubDirectories)
            {
                fileSystemEntries = Directory.GetFileSystemEntries(folder.Path, "*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                fileSystemEntries = Directory.GetFileSystemEntries(folder.Path, "*", SearchOption.TopDirectoryOnly).ToList();
            }

            List<string> result = new List<string>();   
            foreach (string path in fileSystemEntries)
            {
                FileAttributes attributes = File.GetAttributes(path);
                // If the path is a directory, do not add it to the result!
                if (attributes.HasFlag(FileAttributes.Directory)) continue;
                result.Add(path);
            }
            return result;
        }


        // Get a list of the specified file paths from a SourceFile list.
        private static List<string> GetPathsFromEntities(List<SourceFile> files)
        {
            List<string> result = new List<string>();
            foreach (SourceFile file in files) result.Add(file.Path);
            return result;
        }

        /// <summary>
        /// Ensures that the service objects are properly disposed. Also calls <c>Dispose</c> on the repository objects.
        /// </summary>
        /// <param name="disposing"> Signifies that the object is not being disposed directly from the finalizer. </param>
        protected override void Dispose(bool disposing)
        {
            _sourceFileRepository.Dispose();
            _sourceFolderRepository.Dispose();
            _folderFileMappingRepository.Dispose();
            base.Dispose(disposing);
        }
    }
}
