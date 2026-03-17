namespace epubst.Models;

public record EpubOptions
{
    public FileInfo Couverture { get; init; } = null!;
    public FileInfo? Css { get; init; }
    public bool TableDesMatieres { get; init; } = false;
}
