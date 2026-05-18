namespace HsTracker.Models;

public class HsCardsPage
{
    public List<HsCard>? Cards { get; set; }

    public int CardCount { get; set; }

    public int PageCount { get; set; }

    public int Page { get; set; }
}
