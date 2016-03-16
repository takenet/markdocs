namespace Takenet.MarkDocs
{
    public class DynamicNode
    {
        public string Key { get; set; }
        public string ParentKey { get; set; }
        public string Title { get; set; }
        public string Folder { get; set; }
        public string Document { get; set; }
        public bool IsLocalized { get; set; }
    }
}
