using System.IO.Compression;
using epubst.Epub;
using epubst.Models;

namespace epubst.Tests.Epub;

public class EpubBuilderTests : IDisposable
{
    private DirectoryInfo _dir = null!;
    private BookProject   _projet = null!;

    public EpubBuilderTests()
    {
        _dir = Directory.CreateTempSubdirectory("epubst_test_");

        File.WriteAllText(Path.Combine(_dir.FullName, "cover.jpg"), "");
        File.WriteAllText(Path.Combine(_dir.FullName, "remerciements.md"), "Merci à tous.");
        File.WriteAllText(Path.Combine(_dir.FullName, "corps.md"),
            "# Prologue\n\nIl faisait nuit.\n\n# Chapitre 1\n\nLe récit commence.");

        _projet = new BookProject
        {
            Metadonnees = new Metadonnees
            {
                Titre   = "Mon Roman",
                Auteurs = ["Jean Dupont"],
                Langue  = "fr"
            },
            EpubOptions = new EpubOptions
            {
                Couverture = new FileInfo(Path.Combine(_dir.FullName, "cover.jpg"))
            },
            Contenu =
            [
                new ContenuItem { Fichier = new FileInfo(Path.Combine(_dir.FullName, "remerciements.md")), Navigation = false },
                new ContenuItem { Fichier = new FileInfo(Path.Combine(_dir.FullName, "corps.md")),         Navigation = true  },
            ]
        };
    }

    public void Dispose() => _dir.Delete(recursive: true);

    private ZipArchive Compiler()
    {
        var ms = new MemoryStream();
        EpubBuilder.Compiler(_projet, _dir, ms);
        ms.Position = 0;
        return new ZipArchive(ms, ZipArchiveMode.Read);
    }

    // ========== Structure obligatoire ==========

    [Fact]
    public void Compiler_ContientMimetype()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("mimetype"));
    }

    [Fact]
    public void Compiler_MimetypeNonCompresse()
    {
        using var zip = Compiler();
        var entry = zip.GetEntry("mimetype")!;
        Assert.Equal(entry.CompressedLength, entry.Length);
    }

    [Fact]
    public void Compiler_MimetypePremiereFichier()
    {
        using var zip = Compiler();
        Assert.Equal("mimetype", zip.Entries[0].FullName);
    }

    [Fact]
    public void Compiler_MimetypeContientValeurCorrecte()
    {
        using var zip = Compiler();
        using var reader = new StreamReader(zip.GetEntry("mimetype")!.Open());
        Assert.Equal("application/epub+zip", reader.ReadToEnd());
    }

    [Fact]
    public void Compiler_ContientContainerXml()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("META-INF/container.xml"));
    }

    [Fact]
    public void Compiler_ContainerXmlReferenceLOpf()
    {
        using var zip = Compiler();
        using var reader = new StreamReader(zip.GetEntry("META-INF/container.xml")!.Open());
        Assert.Contains("OEBPS/content.opf", reader.ReadToEnd());
    }

    // ========== Fichiers OEBPS ==========

    [Fact]
    public void Compiler_ContientOpf()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/content.opf"));
    }

    [Fact]
    public void Compiler_ContientNavXhtml()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/nav.xhtml"));
    }

    [Fact]
    public void Compiler_ContientCoverXhtml()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/cover.xhtml"));
    }

    [Fact]
    public void Compiler_ContientDefaultCss()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/styles/default.css"));
    }

    [Fact]
    public void Compiler_ContientImageCouverture()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/images/cover.jpg"));
    }

    // ========== Documents texte ==========

    [Fact]
    public void Compiler_ContientDocumentNavigationFalse()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/text/remerciements.xhtml"));
    }

    [Fact]
    public void Compiler_NavigationTrue_UnFichierParH1()
    {
        using var zip = Compiler();
        Assert.NotNull(zip.GetEntry("OEBPS/text/corps_prologue.xhtml"));
        Assert.NotNull(zip.GetEntry("OEBPS/text/corps_chapitre_1.xhtml"));
    }

    [Fact]
    public void Compiler_NavXhtmlContientLesChapitres()
    {
        using var zip = Compiler();
        using var reader = new StreamReader(zip.GetEntry("OEBPS/nav.xhtml")!.Open());
        var nav = reader.ReadToEnd();
        Assert.Contains("Prologue", nav);
        Assert.Contains("Chapitre 1", nav);
    }

    // ========== CSS personnalisé ==========

    [Fact]
    public void Compiler_AvecCssPersonnalise_ContientLesCssDansZip()
    {
        File.WriteAllText(Path.Combine(_dir.FullName, "style.css"), "body { color: red; }");
        var projet = _projet with
        {
            EpubOptions = _projet.EpubOptions with { Css = new FileInfo(Path.Combine(_dir.FullName, "style.css")) }
        };

        var ms = new MemoryStream();
        EpubBuilder.Compiler(projet, _dir, ms);
        ms.Position = 0;
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        Assert.NotNull(zip.GetEntry("OEBPS/styles/style.css"));
    }
}
