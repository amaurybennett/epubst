namespace epubst.Models;

public record FonteItem
{
    public string Nom { get; init; } = null!;
    public FileInfo Fichier { get; init; } = null!;
}
