namespace epubst.Models;

public record BookProject
{
    public Metadonnees Metadonnees { get; init; } = null!;
    public EpubOptions EpubOptions { get; init; } = null!;
    public List<ContenuItem> Contenu { get; init; } = [];
}
