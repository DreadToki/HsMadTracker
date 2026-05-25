namespace HsTracker.Models.HearthstoneData;

public class HearthstoneDataCardsPage
{
    public List<HearthstoneDataCard>? Cards { get; set; }

    public int CardCount { get; set; }

    public int PageCount { get; set; }

    public int Page { get; set; }
}
