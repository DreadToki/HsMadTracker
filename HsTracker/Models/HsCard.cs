namespace HsTracker.Models;

public class HsCard
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public sbyte ManaCost { get; set; }

    public Uri? Image { get; set; }

    public Uri? CropImage { get; set; }
}
