using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserContenuTests
{
    private static string TomlMetaEpub(DirectoryInfo dir)
    {
        File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
        return """
            [metadonnees]
            titre = "Mon Roman"
            auteurs = ["Moi"]
            langue = "fr"

            [epub]
            couverture = "cover.jpg"

            """;
    }

    [Fact]
    public void Parse_ContenusMultiples_RetourneListeOrdrée()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "remerciements.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "mentions.md"), "");
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "remerciements.md"

                [[contenu]]
                fichier = "corps.md"
                navigation = true

                [[contenu]]
                fichier = "mentions.md"
                """;

            var items = ProjectParser.Parse(toml, dir).Contenu;

            Assert.Equal(3, items.Count);
            Assert.Equal("remerciements.md", items[0].Fichier.Name);
            Assert.Equal("corps.md", items[1].Fichier.Name);
            Assert.Equal("mentions.md", items[2].Fichier.Name);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_NavigationFalseParDefaut()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "remerciements.md"), "");
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "remerciements.md"
                """;

            var items = ProjectParser.Parse(toml, dir).Contenu;

            Assert.False(items[0].Navigation);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_NavigationTrue_Conservee()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "corps.md"
                navigation = true
                """;

            var items = ProjectParser.Parse(toml, dir).Contenu;

            Assert.True(items[0].Navigation);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_SectionContenuAbsente_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaEpub(dir);

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_FichierIntrouvable_MessageContientChemin()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "inexistant.md"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.Parse(toml, dir));

            Assert.Contains("inexistant.md", ex.Message);
            Assert.Contains(dir.FullName, ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampFichierAbsent_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                navigation = true
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_TitreAbsent_NullParDefaut()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "corps.md"
                """;

            var item = ProjectParser.Parse(toml, dir).Contenu[0];

            Assert.Null(item.Titre);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_TitrePresent_Conserve()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "remerciements.md"), "");
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "remerciements.md"
                titre = "Remerciements"
                """;

            var item = ProjectParser.Parse(toml, dir).Contenu[0];

            Assert.Equal("Remerciements", item.Titre);
        }
        finally { dir.Delete(recursive: true); }
    }

    // ========== Sécurité : path traversal ==========

    [Fact]
    public void Parse_ContenuPathTraversal_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaEpub(dir) + """
                [[contenu]]
                fichier = "../../secret.md"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }
}
