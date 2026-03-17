using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserContenuTests
{
    private static DirectoryInfo CreeRepertoireTemporaire()
    {
        return Directory.CreateTempSubdirectory("epubst_test_");
    }

    [Fact]
    public void Parse_ContenusMultiples_RetourneListeOrdrée()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "remerciements.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
            File.WriteAllText(Path.Combine(dir.FullName, "mentions.md"), "");

            var toml = """
                [[contenu]]
                fichier = "remerciements.md"

                [[contenu]]
                fichier = "corps.md"
                navigation = true

                [[contenu]]
                fichier = "mentions.md"
                """;

            var items = ProjectParser.ParseContenu(toml, dir);

            Assert.Equal(3, items.Count);
            Assert.EndsWith("remerciements.md", items[0].Fichier.Name);
            Assert.EndsWith("corps.md", items[1].Fichier.Name);
            Assert.EndsWith("mentions.md", items[2].Fichier.Name);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_NavigationFalseParDefaut()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "remerciements.md"), "");

            var toml = """
                [[contenu]]
                fichier = "remerciements.md"
                """;

            var items = ProjectParser.ParseContenu(toml, dir);

            Assert.False(items[0].Navigation);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_NavigationTrue_Conservee()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");

            var toml = """
                [[contenu]]
                fichier = "corps.md"
                navigation = true
                """;

            var items = ProjectParser.ParseContenu(toml, dir);

            Assert.True(items[0].Navigation);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_SectionContenuAbsente_LanceException()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [metadonnees]
                titre = "Mon Roman"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.ParseContenu(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_FichierIntrouvable_MessageContientChemin()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [[contenu]]
                fichier = "inexistant.md"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.ParseContenu(toml, dir));

            Assert.Contains("inexistant.md", ex.Message);
            Assert.Contains(dir.FullName, ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_ChampFichierAbsent_LanceException()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [[contenu]]
                navigation = true
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.ParseContenu(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }
}
