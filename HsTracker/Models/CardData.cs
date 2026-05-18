using System.Drawing;

namespace HsTracker.Models;

public class CardData
{
    public int Id { get; set; }

    public string? CardId { get; set; }

    public string? Name { get; set; }

    public sbyte ManaCost { get; set; }

    public string? FullImage { get; set; }

    public string? CropImage { get; set; }
}
