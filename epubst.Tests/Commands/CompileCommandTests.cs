using epubst.Commands;

namespace epubst.Tests.Commands;

public class CompileCommandTests
{
    private static (DirectoryInfo dir, string bookToml) CreeProjetValide()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        File.WriteAllText(Path.Combine(dir.FullName, "cover.jpg"), "");
        var bookToml = Path.Combine(dir.FullName, "book.toml");
        File.WriteAllText(Path.Combine(dir.FullName, "corps.md"), "");
        File.WriteAllText(bookToml, """
            [metadonnees]
            titre = "Mon Roman"
            auteurs = ["Moi"]
            langue = "fr"

            [epub]
            couverture = "cover.jpg"

            [[contenu]]
            fichier = "corps.md"
            navigation = true
            """);
        return (dir, bookToml);
    }

    [Fact]
    public async Task CompileCommand_SansArgument_RetourneErreur()
    {
        var root = RootCommandBuilder.Build();
        var code = await root.Parse(["compile"]).InvokeAsync();
        Assert.NotEqual(0, code);
    }

    [Fact]
    public async Task CompileCommand_AvecFichier_RetourneSucces()
    {
        var (dir, bookToml) = CreeProjetValide();
        try
        {
            var root = RootCommandBuilder.Build();
            var code = await root.Parse(["compile", bookToml]).InvokeAsync();
            Assert.Equal(0, code);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public async Task CompileCommand_AvecFichierEtOutput_RetourneSucces()
    {
        var (dir, bookToml) = CreeProjetValide();
        try
        {
            var root = RootCommandBuilder.Build();
            var code = await root.Parse(["compile", bookToml, "-o", "sortie.epub"]).InvokeAsync();
            Assert.Equal(0, code);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public async Task CompileCommand_FichierInexistant_RetourneErreur()
    {
        var root = RootCommandBuilder.Build();
        var code = await root.Parse(["compile", "inexistant.toml"]).InvokeAsync();
        Assert.NotEqual(0, code);
    }

    [Fact]
    public async Task CommandeInconnue_RetourneErreur()
    {
        var root = RootCommandBuilder.Build();
        var code = await root.Parse(["inconnue"]).InvokeAsync();
        Assert.NotEqual(0, code);
    }
}
