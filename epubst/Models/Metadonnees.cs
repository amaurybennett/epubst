namespace epubst.Models;

public record Metadonnees
{
    public string Titre { get; init; } = "";
    public string? SousTitre { get; init; }
    public string? Serie { get; init; }
    public int? NumeroTome { get; init; }
    public List<string> Auteurs { get; init; } = [];
    public string Langue { get; init; } = "";
    public string? Editeur { get; init; }
    public string? Isbn { get; init; }
    public string? DatePublication { get; init; }
}
