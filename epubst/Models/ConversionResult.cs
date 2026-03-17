namespace epubst.Models;

public record XhtmlDocument
{
    public string NomFichier { get; init; } = null!;
    public string Contenu { get; init; } = null!;
}

public record ChapitreNav
{
    public string Titre { get; init; } = null!;
    public string NomFichier { get; init; } = null!;
}

public record ConversionResult
{
    public List<XhtmlDocument> Documents { get; init; } = [];
    public List<ChapitreNav> Chapitres { get; init; } = [];
}
