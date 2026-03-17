namespace epubst.Models;

public record ContenuItem
{
    public FileInfo Fichier { get; init; } = null!;
    public bool Navigation { get; init; } = false;
}
