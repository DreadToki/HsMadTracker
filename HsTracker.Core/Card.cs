class Card
{
    // Bare minimum properties for a card, can be expanded as needed
    public required string CardId { get; set; }
    public string? DbfId { get; set; }
    public string? Name { get; set; }

    // public string? Type { get; set; }
    // public string? Class { get; set; }
    public int ManaCost { get; set; }
    // public int Attack { get; set; }
    // public int Health { get; set; }
    // Introduce an Image property
}
