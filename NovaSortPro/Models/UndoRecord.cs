using System;

namespace NovaSortPro.Models
{
    public class UndoRecord
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public DateTime OperationTime { get; set; }
        public string OriginalPath { get; set; } = string.Empty; // Where the file was originally
        public string NewPath { get; set; } = string.Empty;      // Where the file is now
        public long FileSize { get; set; }
        public bool IsRestored { get; set; } = false;
    }
}
