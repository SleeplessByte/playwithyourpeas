using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// 
/// </summary>
/// <remarks>http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx</remarks>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[XmlRoot("dictionary")]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
    #region IXmlSerializable Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    public void ReadXml(System.Xml.XmlReader reader)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), typeof(TKey).Namespace);
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), typeof(TValue).Namespace);

        bool wasEmpty = reader.IsEmptyElement;

        reader.Read();
        if (wasEmpty)
            return;

        while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
        {
            reader.ReadStartElement("entry");

            reader.ReadStartElement("key", typeof(TKey).FullName);
            TKey key = (TKey)keySerializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("value", typeof(TValue).FullName);
            TValue value = (TValue)valueSerializer.Deserialize(reader);
            reader.ReadEndElement();

            this.Add(key, value);

            reader.ReadEndElement();
            reader.MoveToContent();
        }
        reader.ReadEndElement();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="writer"></param>
    public void WriteXml(System.Xml.XmlWriter writer)
    {
        XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), typeof(TKey).Namespace);
        XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), typeof(TValue).Namespace);

        foreach (TKey key in this.Keys)
        {
            writer.WriteStartElement("entry");

            writer.WriteStartElement("key", typeof(TKey).FullName);
            keySerializer.Serialize(writer, key);
            writer.WriteEndElement();

            writer.WriteStartElement("value", typeof(TValue).FullName);
            TValue value = this[key];
            valueSerializer.Serialize(writer, value);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }
    #endregion
}