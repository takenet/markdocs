using System.Configuration;

namespace Takenet.MarkDocs
{
    public class NodeElement : ConfigurationElement, INode
    {
        [ConfigurationProperty("targetFolder", DefaultValue = null, IsRequired = true, IsKey = true)]
        public string TargetFolder
        {
            get { return (string)this["targetFolder"]; }
            set { this["targetFolder"] = value; }
        }

        [ConfigurationProperty("index", DefaultValue = "index", IsRequired = true, IsKey = true)]
        public string Index
        {
            get { return (string)this["index"]; }
            set { this["index"] = value; }
        }

        [ConfigurationProperty("username", DefaultValue = null, IsRequired = true, IsKey = true)]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }

        [ConfigurationProperty("password", DefaultValue = null, IsRequired = true, IsKey = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("display", DefaultValue = null, IsRequired = true, IsKey = false)]
        public string Display
        {
            get { return (string)this["display"]; }
            set { this["display"] = value; }
        }

        [ConfigurationProperty("owner", DefaultValue = null, IsRequired = true, IsKey = false)]
        public string Owner
        {
            get { return (string)this["owner"]; }
            set { this["owner"] = value; }
        }

        [ConfigurationProperty("repo", DefaultValue = null, IsRequired = true, IsKey = false)]
        public string Repo
        {
            get { return (string)this["repo"]; }
            set { this["repo"] = value; }
        }

        [ConfigurationProperty("branch", DefaultValue = "master", IsRequired = false, IsKey = false)]
        public string Branch
        {
            get { return (string)this["branch"]; }
            set { this["branch"] = value; }
        }

        [ConfigurationProperty("sourceFolder", DefaultValue = "docs", IsRequired = false, IsKey = false)]
        public string SourceFolder
        {
            get { return (string)this["sourceFolder"]; }
            set { this["sourceFolder"] = value; }
        }

        [ConfigurationProperty("localized", DefaultValue = true, IsRequired = false, IsKey = false)]
        public bool Localized
        {
            get { return (bool)this["localized"]; }
            set { this["localized"] = value; }
        }

        [ConfigurationProperty("", IsRequired = false, IsKey = false, IsDefaultCollection = true)]
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
