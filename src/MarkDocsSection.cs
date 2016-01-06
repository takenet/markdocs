using System.Configuration;

namespace Takenet.MarkDocs
{
    public class MarkDocsSection : ConfigurationSection, INode
    {
        [ConfigurationProperty("", IsRequired = true, IsKey = false, IsDefaultCollection = true)]
        public NodeCollection Items
        {
            get
            {
                return ((NodeCollection)(base[""]));
            }
            set
            {
                base[""] = value;
            }
        }
    }
}
