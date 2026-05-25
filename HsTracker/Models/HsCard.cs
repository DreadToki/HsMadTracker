namespace HsTracker.Models;

public class HsCard
{
    public sbyte Player { get; set; }

    public string? CardId { get; set; }

    public HsCardData? CardData { get; set; }
}
