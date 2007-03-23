
using System;
using System.Xml;
using System.Collections.Specialized;
using Mono.Addins.Serialization;

namespace Mono.Addins.Description
{
	public class ExtensionNodeDescription: ObjectDescription, NodeElement
	{
		ExtensionNodeDescriptionCollection childNodes;
		string[] attributes;
		string nodeName;
		
		public ExtensionNodeDescription (string nodeName)
		{
			this.nodeName = nodeName;
		}
		
		internal ExtensionNodeDescription (XmlElement elem)
		{
			Element = elem;
			nodeName = elem.LocalName;
		}
		
		internal ExtensionNodeDescription ()
		{
		}
		
		internal override void Verify (string location, StringCollection errors)
		{
			if (nodeName == null || nodeName.Length == 0)
				errors.Add (location + "Node: NodeName can't be empty.");
			ChildNodes.Verify (location + NodeName + "/", errors);
		}
		
		public string NodeName {
			get { return nodeName; }
		}
		
		internal override void SaveXml (XmlElement parent)
		{
			if (Element == null) {
				Element = parent.OwnerDocument.CreateElement (nodeName);
				parent.AppendChild (Element);
				if (attributes != null) {
					for (int n=0; n<attributes.Length; n+=2)
						Element.SetAttribute (attributes[n], attributes[n+1]);
				}
				ChildNodes.SaveXml (Element);
			}
		}
		
		public string GetAttribute (string key)
		{
			if (Element != null)
				return Element.GetAttribute (key);

			if (attributes == null)
				return string.Empty;
			for (int n=0; n<attributes.Length; n+=2) {
				if (attributes [n] == key)
					return attributes [n+1];
			}
			return string.Empty;
		}
		
		public void SetAttribute (string key, string value)
		{
			if (Element != null) {
				Element.SetAttribute (key, value);
				return;
			}
			
			if (value == null)
				value = string.Empty;
			
			if (attributes == null) {
				attributes = new string [2];
				attributes [0] = key;
				attributes [1] = value;
				return;
			}
			
			for (int n=0; n<attributes.Length; n+=2) {
				if (attributes [n] == key) {
					attributes [n+1] = value;
					return;
				}
			}
			string[] newList = new string [attributes.Length + 2];
			attributes.CopyTo (newList, 0);
			attributes = newList;
			attributes [attributes.Length - 2] = key;
			attributes [attributes.Length - 1] = value;
		}
		
		public NodeAttribute[] Attributes {
			get {
				if (Element != null)
					SaveXmlAttributes ();
				if (attributes == null)
					return new NodeAttribute [0];
				NodeAttribute[] ats = new NodeAttribute [attributes.Length / 2];
				for (int n=0; n<ats.Length; n++) {
					NodeAttribute at = new NodeAttribute ();
					at.name = attributes [n*2];
					at.value = attributes [n*2 + 1];
					ats [n] = at;
				}
				return ats;
			}
		}
		
		public ExtensionNodeDescriptionCollection ChildNodes {
			get {
				if (childNodes == null) {
					childNodes = new ExtensionNodeDescriptionCollection ();
					if (Element != null) {
						foreach (XmlNode nod in Element.ChildNodes) {
							if (nod is XmlElement)
								childNodes.Add (new ExtensionNodeDescription ((XmlElement)nod));
						}
					}
				}
				return childNodes;
			}
		}
		
		void SaveXmlAttributes ()
		{
			attributes = new string [Element.Attributes.Count * 2];
			for (int n=0; n<attributes.Length; n+=2) {
				XmlAttribute at = Element.Attributes [n/2];
				attributes [n] = at.LocalName;
				attributes [n+1] = at.Value;
			}
		}
		
		internal override void Write (BinaryXmlWriter writer)
		{
			if (Element != null)
				SaveXmlAttributes ();
			
			writer.WriteValue ("nodeName", nodeName);
			writer.WriteValue ("attributes", attributes);
			writer.WriteValue ("ChildNodes", ChildNodes);
		}
		
		internal override void Read (BinaryXmlReader reader)
		{
			nodeName = reader.ReadStringValue ("nodeName");
			attributes = (string[]) reader.ReadValue ("attributes");
			childNodes = (ExtensionNodeDescriptionCollection) reader.ReadValue ("ChildNodes", new ExtensionNodeDescriptionCollection ());
		}
	}
}