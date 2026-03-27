using epubst.Epub;
using epubst.Models;

namespace epubst.Tests.Epub;

public class FontesCssGeneratorTests
{
    [Fact]
    public void Generer_ListeVide_RetourneChainVide()
    {
        var css = FontesCssGenerator.Generer([]);

        Assert.Equal(string.Empty, css);
    }

    [Fact]
    public void Generer_UneFonte_ContientAtFontFace()
    {
        var fonte = new FonteItem { Nom = "Ma Fonte", Fichier = new FileInfo("mafonte.otf") };

        var css = FontesCssGenerator.Generer([fonte]);

        Assert.Contains("@font-face", css);
    }

    [Fact]
    public void Generer_UneFonte_ContientFontFamily()
    {
        var fonte = new FonteItem { Nom = "Ma Fonte", Fichier = new FileInfo("mafonte.otf") };

        var css = FontesCssGenerator.Generer([fonte]);

        Assert.Contains("font-family: \"Ma Fonte\"", css);
    }

    [Fact]
    public void Generer_UneFonte_ContientSrcAvecCheminRelatif()
    {
        var fonte = new FonteItem { Nom = "Ma Fonte", Fichier = new FileInfo("mafonte.otf") };

        var css = FontesCssGenerator.Generer([fonte]);

        Assert.Contains("src: url(\"../fonts/mafonte.otf\")", css);
    }

    [Fact]
    public void Generer_NomAvecGuillemet_EchappeDansCss()
    {
        var fonte = new FonteItem { Nom = "Fonte\"Malicieuse", Fichier = new FileInfo("fonte.otf") };

        var css = FontesCssGenerator.Generer([fonte]);

        Assert.Contains("font-family: \"Fonte\\\"Malicieuse\"", css);
    }

    [Fact]
    public void Generer_NomAvecBackslash_EchappeDansCss()
    {
        var fonte = new FonteItem { Nom = "Fonte\\Evil", Fichier = new FileInfo("fonte.otf") };

        var css = FontesCssGenerator.Generer([fonte]);

        Assert.Contains("font-family: \"Fonte\\\\Evil\"", css);
    }

    [Fact]
    public void Generer_PlusieursPolices_ToutesPresentes()
    {
        var fontes = new List<FonteItem>
        {
            new() { Nom = "Regular", Fichier = new FileInfo("regular.otf") },
            new() { Nom = "Bold",    Fichier = new FileInfo("bold.otf") }
        };

        var css = FontesCssGenerator.Generer(fontes);

        Assert.Contains("font-family: \"Regular\"", css);
        Assert.Contains("font-family: \"Bold\"", css);
        Assert.Contains("../fonts/regular.otf", css);
        Assert.Contains("../fonts/bold.otf", css);
    }
}
