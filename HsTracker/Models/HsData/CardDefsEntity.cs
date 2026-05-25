using System.Xml.Serialization;

namespace HsTracker.Models.HsData;

public class CardDefsEntity
{
    [XmlAttribute("CardID")]
    public string? CardId { get; set; }

    [XmlAttribute("ID")]
    public int Id { get; set; }
}
