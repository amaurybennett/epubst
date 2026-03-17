using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserMetadonneesTests
{
    [Fact]
    public void Parse_ChampsObligatoires_Succes()
    {
        var toml = """
            [metadonnees]
            titre = "À l'école des sorciers"
            auteurs = ["J.K. Rowling"]
            langue = "fr"
            """;

        var meta = ProjectParser.ParseMetadonnees(toml);

        Assert.Equal("À l'école des sorciers", meta.Titre);
        Assert.Equal(["J.K. Rowling"], meta.Auteurs);
        Assert.Equal("fr", meta.Langue);
    }

    [Fact]
    public void Parse_ChampsOptionnels_Succes()
    {
        var toml = """
            [metadonnees]
            titre = "À l'école des sorciers"
            sous_titre = "Harry Potter"
            serie = "Harry Potter"
            numero_tome = 1
            auteurs = ["J.K. Rowling"]
            langue = "fr"
            editeur = "Gallimard"
            isbn = "978-2-07-054127-2"
            date_publication = "1998-10-09"
            """;

        var meta = ProjectParser.ParseMetadonnees(toml);

        Assert.Equal("Harry Potter", meta.SousTitre);
        Assert.Equal("Harry Potter", meta.Serie);
        Assert.Equal(1, meta.NumeroTome);
        Assert.Equal("Gallimard", meta.Editeur);
        Assert.Equal("978-2-07-054127-2", meta.Isbn);
        Assert.Equal("1998-10-09", meta.DatePublication);
    }

    [Fact]
    public void Parse_PlusieurAuteurs_Succes()
    {
        var toml = """
            [metadonnees]
            titre = "Un livre à deux"
            auteurs = ["Auteur Un", "Auteur Deux"]
            langue = "fr"
            """;

        var meta = ProjectParser.ParseMetadonnees(toml);

        Assert.Equal(2, meta.Auteurs.Count);
        Assert.Contains("Auteur Un", meta.Auteurs);
        Assert.Contains("Auteur Deux", meta.Auteurs);
    }

    [Fact]
    public void Parse_SectionMetadonneesAbsente_LanceException()
    {
        var toml = """
            [epub]
            couverture = "cover.jpg"
            """;

        Assert.Throws<InvalidOperationException>(() =>
            ProjectParser.ParseMetadonnees(toml));
    }

    [Fact]
    public void Parse_ChampsOptionnelsAbsents_SontNull()
    {
        var toml = """
            [metadonnees]
            titre = "Mon Roman"
            auteurs = ["Moi"]
            langue = "fr"
            """;

        var meta = ProjectParser.ParseMetadonnees(toml);

        Assert.Null(meta.SousTitre);
        Assert.Null(meta.Serie);
        Assert.Null(meta.NumeroTome);
        Assert.Null(meta.Editeur);
        Assert.Null(meta.Isbn);
        Assert.Null(meta.DatePublication);
    }
}
