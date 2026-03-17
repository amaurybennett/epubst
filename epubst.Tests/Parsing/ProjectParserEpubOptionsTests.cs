using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserEpubOptionsTests
{
    private static string TomlMetaContenu(DirectoryInfo dir)
    {
        File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
        return """
            [metadonnees]
            titre = "Mon Roman"
            auteurs = ["Moi"]
            langue = "fr"

            [[contenu]]
            fichier = "corps.md"
            """;
    }

    [Fact]
    public void Parse_CouvertureExistante_Succes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");
            var toml = TomlMetaContenu(dir) + """

                [epub]
                couverture = "cover.jpg"
                """;

            var options = ProjectParser.Parse(toml, dir).EpubOptions;

            Assert.Equal(couverture, options.Couverture.FullName);
            Assert.Null(options.Css);
            Assert.False(options.TableDesMatieres);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureEtCssExistants_Succes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            var css = Path.Combine(dir.FullName, "style.css");
            File.WriteAllText(couverture, "");
            File.WriteAllText(css, "");
            var toml = TomlMetaContenu(dir) + """

                [epub]
                couverture = "cover.jpg"
                css = "style.css"
                """;

            var options = ProjectParser.Parse(toml, dir).EpubOptions;

            Assert.Equal(css, options.Css!.FullName);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_TableDesMatieres_Succes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
            var toml = TomlMetaContenu(dir) + """

                [epub]
                couverture = "cover.jpg"
                table_des_matieres = true
                """;

            var options = ProjectParser.Parse(toml, dir).EpubOptions;

            Assert.True(options.TableDesMatieres);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CheminAbsoluCouverture_Succes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");
            var toml = TomlMetaContenu(dir) + $"""

                [epub]
                couverture = "{couverture.Replace("\\", "\\\\")}"
                """;

            var options = ProjectParser.Parse(toml, dir).EpubOptions;

            Assert.Equal(couverture, options.Couverture.FullName);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_SectionEpubAbsente_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaContenu(dir);

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureAbsente_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaContenu(dir) + """

                [epub]
                table_des_matieres = false
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.Parse(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureIntrouvable_MessageContientCheminComplet()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var toml = TomlMetaContenu(dir) + """

                [epub]
                couverture = "inexistant.jpg"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.Parse(toml, dir));

            Assert.Contains(dir.FullName, ex.Message);
            Assert.Contains("inexistant.jpg", ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CssIntrouvable_MessageContientCheminComplet()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
            var toml = TomlMetaContenu(dir) + """

                [epub]
                couverture = "cover.jpg"
                css = "inexistant.css"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.Parse(toml, dir));

            Assert.Contains(dir.FullName, ex.Message);
            Assert.Contains("inexistant.css", ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }
}
