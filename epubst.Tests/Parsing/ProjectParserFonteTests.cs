using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserFonteTests
{
    private static string TomlBase(DirectoryInfo dir)
    {
        File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
        File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
        return """
            [metadonnees]
            titre = "Mon Roman"
            auteurs = ["Moi"]
            langue = "fr"

            [epub]
            couverture = "cover.jpg"

            [[contenu]]
            fichier = "corps.md"
            navigation = true

            """;
    }

    [Fact]
    public void Parse_SansFonte_ListeVide()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlBase(dir);
            var projet = ProjectParser.Parse(toml, dir);

            Assert.Empty(projet.Fontes);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_UneFonte_NomEtFichierResolus()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "mafonte.otf"), "");
            var toml = TomlBase(dir) + """
                [[fonte]]
                nom = "Ma Jolie Fonte"
                fichier = "mafonte.otf"
                """;

            var fontes = ProjectParser.Parse(toml, dir).Fontes;

            Assert.Single(fontes);
            Assert.Equal("Ma Jolie Fonte", fontes[0].Nom);
            Assert.Equal("mafonte.otf", fontes[0].Fichier.Name);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_PlusieursPolices_ListeOrdonnee()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "regular.ttf"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "bold.ttf"), "");
            var toml = TomlBase(dir) + """
                [[fonte]]
                nom = "Regular"
                fichier = "regular.ttf"

                [[fonte]]
                nom = "Bold"
                fichier = "bold.ttf"
                """;

            var fontes = ProjectParser.Parse(toml, dir).Fontes;

            Assert.Equal(2, fontes.Count);
            Assert.Equal("Regular", fontes[0].Nom);
            Assert.Equal("Bold", fontes[1].Nom);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_FichierFonteIntrouvable_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlBase(dir) + """
                [[fonte]]
                nom = "Inexistante"
                fichier = "inexistante.otf"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.Parse(toml, dir));

            Assert.Contains("inexistante.otf", ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampNomAbsent_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "mafonte.otf"), "");
            var toml = TomlBase(dir) + """
                [[fonte]]
                fichier = "mafonte.otf"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampFichierAbsent_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlBase(dir) + """
                [[fonte]]
                nom = "Ma Fonte"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }
}
