namespace HsTracker.Models.HearthstoneData;

public class HearthstoneDataCard
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public sbyte ManaCost { get; set; }

    public Uri? Image { get; set; }

    public Uri? CropImage { get; set; }
}
