using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class ProjectParserEpubOptionsTests
{
    private static DirectoryInfo CreeRepertoireTemporaire()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        return dir;
    }

    [Fact]
    public void Parse_CouvertureExistante_Succes()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");

            var toml = """
                [epub]
                couverture = "cover.jpg"
                """;

            var options = ProjectParser.ParseEpubOptions(toml, dir);

            Assert.Equal(couverture, options.Couverture.FullName);
            Assert.Null(options.Css);
            Assert.False(options.TableDesMatieres);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureEtCssExistants_Succes()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            var css = Path.Combine(dir.FullName, "style.css");
            File.WriteAllText(couverture, "");
            File.WriteAllText(css, "");

            var toml = """
                [epub]
                couverture = "cover.jpg"
                css = "style.css"
                """;

            var options = ProjectParser.ParseEpubOptions(toml, dir);

            Assert.Equal(css, options.Css!.FullName);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_TableDesMatieres_Succes()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");

            var toml = """
                [epub]
                couverture = "cover.jpg"
                table_des_matieres = true
                """;

            var options = ProjectParser.ParseEpubOptions(toml, dir);

            Assert.True(options.TableDesMatieres);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CheminAbsoluCouverture_Succes()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");

            var toml = $"""
                [epub]
                couverture = "{couverture.Replace("\\", "\\\\")}"
                """;

            var options = ProjectParser.ParseEpubOptions(toml, dir);

            Assert.Equal(couverture, options.Couverture.FullName);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_SectionEpubAbsente_LanceException()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [metadonnees]
                titre = "Mon Roman"
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.ParseEpubOptions(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureAbsente_LanceException()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [epub]
                table_des_matieres = false
                """;

            Assert.Throws<InvalidOperationException>(() =>
                ProjectParser.ParseEpubOptions(toml, dir));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CouvertureIntrouvable_MessageContientCheminComplet()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var toml = """
                [epub]
                couverture = "inexistant.jpg"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.ParseEpubOptions(toml, dir));

            Assert.Contains(dir.FullName, ex.Message);
            Assert.Contains("inexistant.jpg", ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Parse_CssIntrouvable_MessageContientCheminComplet()
    {
        var dir = CreeRepertoireTemporaire();
        try
        {
            var couverture = Path.Combine(dir.FullName, "cover.jpg");
            File.WriteAllText(couverture, "");

            var toml = """
                [epub]
                couverture = "cover.jpg"
                css = "inexistant.css"
                """;

            var ex = Assert.Throws<FileNotFoundException>(() =>
                ProjectParser.ParseEpubOptions(toml, dir));

            Assert.Contains(dir.FullName, ex.Message);
            Assert.Contains("inexistant.css", ex.Message);
        }
        finally { dir.Delete(recursive: true); }
    }
}
