﻿namespace Services.Dto
{
    public class BackupPathDto : IHasPath
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }
    }
}