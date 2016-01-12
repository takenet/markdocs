using System.Collections;
using System.Collections.Generic;
using System.Configuration;

namespace Takenet.MarkDocs
{
    [ConfigurationCollection(typeof(NodeElement), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
    public class NodeCollection : ConfigurationElementCollection, IEnumerable<NodeElement>
    {
        internal const string ItemPropertyName = "node";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMapAlternate;

        protected override string ElementName => ItemPropertyName;

        protected override bool IsElementName(string elementName)
        {
            return (elementName == ItemPropertyName);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NodeElement)element).TargetFolder;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new NodeElement();
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public new IEnumerator<NodeElement> GetEnumerator()
        {
            return new NodeCollectionEnumerator(this);
        }

        public sealed class NodeCollectionEnumerator : IEnumerator<NodeElement>
        {
            public NodeCollectionEnumerator(NodeCollection collection)
            {
                Collection = collection;
            }

            private readonly NodeCollection Collection;

            public NodeElement Current => (this as IEnumerator).Current as NodeElement;

            public void Dispose()
            {
            }

            private int EnumeratorIndex = -1;

            object IEnumerator.Current => Collection.BaseGet(EnumeratorIndex);

            public bool MoveNext()
            {
                if (EnumeratorIndex < Collection.Count-1)
                {
                    EnumeratorIndex++;
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                EnumeratorIndex = -1;
            }
        }
    }  
}
