namespace HsTracker.Models;

public class PlayedCard
{
    public sbyte Player { get; set; }

    public string? CardId { get; set; }

    public CardData? CardData { get; set; }
}
