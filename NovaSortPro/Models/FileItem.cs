using System;

namespace NovaSortPro.Models
{
    public class FileItem
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public string DirectoryName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Category { get; set; } = "Others";
        public string NewFullPath { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, Processed, Skipped, Error
        public string SHA256 { get; set; } = string.Empty;

        public string SizeDisplay => FormatSize(Size);

        public static string FormatSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            double doubleBytes = bytes;
            int i = 0;
            while (doubleBytes >= 1024 && i < suffixes.Length - 1)
            {
                doubleBytes /= 1024;
                i++;
            }
            return $"{doubleBytes:0.##} {suffixes[i]}";
        }
    }
}
