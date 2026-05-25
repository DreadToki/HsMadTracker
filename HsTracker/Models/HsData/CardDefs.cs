using System.Xml.Serialization;

namespace HsTracker.Models.HsData;

[XmlRoot("CardDefs")]
public class CardDefs
{
    [XmlElement("Entity")]
    public List<CardDefsEntity>? Entities { get; set; }
}
