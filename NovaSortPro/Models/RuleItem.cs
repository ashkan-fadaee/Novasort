namespace NovaSortPro.Models
{
    public class RuleItem
    {
        public int Id { get; set; }
        public string Pattern { get; set; } = string.Empty; // e.g. *.torrent or torrent
        public string TargetFolder { get; set; } = string.Empty; // e.g. Torrents
        public bool IsActive { get; set; } = true;
        public bool IsCustom { get; set; } = true;
    }
}
