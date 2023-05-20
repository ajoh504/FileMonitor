﻿using DataAccessLayer;
using DataAccessLayer.Repositories;
using System.Configuration;

namespace Services.Helpers
{
    public class RepositoryHelper
    {
        public static ISourceFileRepository CreateSourceFileRepositoryInstance()
            => new SourceFileRepository(CreateInstance());

        public static IFullBackupPathRepository CreateFullBackupPathRepositoryInstance()
            => new FullBackupPathRepository(CreateInstance());

        private static FileMonitorDbContext CreateInstance()
            => new FileMonitorDbContext(ConfigurationManager.ConnectionStrings[nameof(FileMonitorDbContext)].ConnectionString);
    }
}