using System.Collections.Generic;

namespace NovaSortPro.Models
{
    public class ScanResult
    {
        public List<FileItem> DuplicateFiles { get; set; } = new();
        public List<FileItem> EmptyFiles { get; set; } = new();
        public List<string> EmptyFolders { get; set; } = new();
        public List<FileItem> LargeFiles { get; set; } = new(); // > 100MB
        public List<FileItem> OldFiles { get; set; } = new();   // > 1 Year
        public List<FileItem> UnknownExtensions { get; set; } = new();
        public List<string> BrokenShortcuts { get; set; } = new();

        public long TotalFilesScanned { get; set; }
        public long TotalFoldersScanned { get; set; }
        public long TotalSizeScanned { get; set; }
    }
}
