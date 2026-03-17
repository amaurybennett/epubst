using epubst.Epub;

namespace epubst.Tests.Epub;

public class CoverGeneratorTests
{
    [Fact]
    public void Generer_ContientDeclarationXml()
    {
        var cover = CoverGenerator.Generer("cover.jpg");

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", cover);
    }

    [Fact]
    public void Generer_ContientNamespaceXhtml()
    {
        var cover = CoverGenerator.Generer("cover.jpg");

        Assert.Contains("xmlns=\"http://www.w3.org/1999/xhtml\"", cover);
    }

    [Fact]
    public void Generer_ContientEpubTypeCover()
    {
        var cover = CoverGenerator.Generer("cover.jpg");

        Assert.Contains("epub:type=\"cover\"", cover);
    }

    [Fact]
    public void Generer_SrcPointeVersImagesSansRemontee()
    {
        // cover.xhtml est dans OEBPS/, les images dans OEBPS/images/
        // Le chemin doit etre "images/xxx", pas "../images/xxx"
        var cover = CoverGenerator.Generer("cover.jpg");

        Assert.Contains("src=\"images/cover.jpg\"", cover);
        Assert.DoesNotContain("../images/", cover);
    }

    [Fact]
    public void Generer_NomFichierInclus()
    {
        var cover = CoverGenerator.Generer("couverture-epub.png");

        Assert.Contains("couverture-epub.png", cover);
    }
}
