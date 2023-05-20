﻿using DataAccessLayer.Entities;
using DataAccessLayer.Repositories;
using Services.Files.Dto;
using Services.Helpers;

namespace Services.Files
{
    public class FileService : IDisposable
    {
        private ISourceFileRepository _sourceFileRepository;
        private bool disposedValue;

        public FileService(ISourceFileRepository sourceFileRepository)
        {
            _sourceFileRepository = sourceFileRepository;
        }

        public List<SourceFileDto> GetFiles()
        {
            var result = _sourceFileRepository.GetMany(s => true, s => new SourceFileDto
            {
                Id = s.Id,
                Path = s.Path
            }, 
            s => s.Id);
            return result;
        }

        public SourceFileDto Add(string path)
        {
            var entity = new SourceFile
            {
                Path = path,
                Hash = EncryptionHelper.GetHash(path)
            };

            _sourceFileRepository.Add(entity);
            _sourceFileRepository.SaveChanges();

            return new SourceFileDto
            {
                Id = entity.Id,
                Path = entity.Path
            };
        }

        public void Remove(IEnumerable<int> ids)
        {
            foreach (int id in ids)
            {
                _sourceFileRepository.Remove(new SourceFile
                {
                    Id = id
                });
            }
            _sourceFileRepository.SaveChanges();
        }

        public bool PathExists(string path)
        {
            return _sourceFileRepository.Exists(s => s.Path == path);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _sourceFileRepository.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~FileService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
