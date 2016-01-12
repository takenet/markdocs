using System.Configuration;

namespace Takenet.MarkDocs
{
    public class MarkDocsSection : ConfigurationSection, INode
    {
        const string DefaultLanguageKey = "defaultLanguage";

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

        [ConfigurationProperty(DefaultLanguageKey, DefaultValue = null, IsRequired = false, IsKey = false)]
        public string DefaultLanguage
        {
            get { return (string)this[DefaultLanguageKey]; }
            set { this[DefaultLanguageKey] = value; }
        }
    }
}
