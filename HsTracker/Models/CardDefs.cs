using System.Xml.Serialization;

namespace HsTracker.Models;

[XmlRoot("CardDefs")]
public class CardDefs
{
    [XmlElement("Entity")]
    public List<CardDefsEntity>? Entities { get; set; }
}
