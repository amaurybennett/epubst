using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserMetadonneesTests
{
    private static (DirectoryInfo dir, string tomlBase) CreeSetup()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
        File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
        var tomlBase = """

            [epub]
            couverture = "cover.jpg"

            [[contenu]]
            fichier = "corps.md"
            navigation = true
            """;
        return (dir, tomlBase);
    }

    [Fact]
    public void Parse_ChampsObligatoires_Succes()
    {
        var (dir, tomlBase) = CreeSetup();
        try
        {
            var toml = """
                [metadonnees]
                titre = "À l'école des sorciers"
                auteurs = ["J.K. Rowling"]
                langue = "fr"
                """ + tomlBase;

            var projet = ProjectParser.Parse(toml, dir);

            Assert.Equal("À l'école des sorciers", projet.Metadonnees.Titre);
            Assert.Equal(["J.K. Rowling"], projet.Metadonnees.Auteurs);
            Assert.Equal("fr", projet.Metadonnees.Langue);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampsOptionnels_Succes()
    {
        var (dir, tomlBase) = CreeSetup();
        try
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
                """ + tomlBase;

            var projet = ProjectParser.Parse(toml, dir);
            var meta = projet.Metadonnees;

            Assert.Equal("Harry Potter", meta.SousTitre);
            Assert.Equal("Harry Potter", meta.Serie);
            Assert.Equal(1, meta.NumeroTome);
            Assert.Equal("Gallimard", meta.Editeur);
            Assert.Equal("978-2-07-054127-2", meta.Isbn);
            Assert.Equal("1998-10-09", meta.DatePublication);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_PlusieurAuteurs_Succes()
    {
        var (dir, tomlBase) = CreeSetup();
        try
        {
            var toml = """
                [metadonnees]
                titre = "Un livre à deux"
                auteurs = ["Auteur Un", "Auteur Deux"]
                langue = "fr"
                """ + tomlBase;

            var projet = ProjectParser.Parse(toml, dir);

            Assert.Equal(2, projet.Metadonnees.Auteurs.Count);
            Assert.Contains("Auteur Un", projet.Metadonnees.Auteurs);
            Assert.Contains("Auteur Deux", projet.Metadonnees.Auteurs);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_SectionMetadonneesAbsente_LanceException()
    {
        var (dir, tomlBase) = CreeSetup();
        try
        {
            var toml = """
                [epub]
                couverture = "cover.jpg"

                [[contenu]]
                fichier = "corps.md"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampsOptionnelsAbsents_SontNull()
    {
        var (dir, tomlBase) = CreeSetup();
        try
        {
            var toml = """
                [metadonnees]
                titre = "Mon Roman"
                auteurs = ["Moi"]
                langue = "fr"
                """ + tomlBase;

            var meta = ProjectParser.Parse(toml, dir).Metadonnees;

            Assert.Null(meta.SousTitre);
            Assert.Null(meta.Serie);
            Assert.Null(meta.NumeroTome);
            Assert.Null(meta.Editeur);
            Assert.Null(meta.Isbn);
            Assert.Null(meta.DatePublication);
        }
        finally { dir.Delete(recursive: true); }
    }
}
