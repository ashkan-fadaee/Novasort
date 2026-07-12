using System;

namespace NovaSortPro.Models
{
    public class Bookmark
    {
        public int Id { get; set; }
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Favorite"; // Favorite, Recent, Pinned
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
