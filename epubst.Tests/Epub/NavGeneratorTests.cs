using epubst.Epub;
using epubst.Models;

namespace epubst.Tests.Epub;

public class NavGeneratorTests
{
    [Fact]
    public void Generer_ContientDeclarationXml()
    {
        var nav = NavGenerator.Generer([]);

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", nav);
    }

    [Fact]
    public void Generer_ContientNamespaceXhtml()
    {
        var nav = NavGenerator.Generer([]);

        Assert.Contains("xmlns=\"http://www.w3.org/1999/xhtml\"", nav);
    }

    [Fact]
    public void Generer_ContientEpubTypeToc()
    {
        var nav = NavGenerator.Generer([]);

        Assert.Contains("epub:type=\"toc\"", nav);
    }

    [Fact]
    public void Generer_ListeVide_ContientOlVide()
    {
        var nav = NavGenerator.Generer([]);

        Assert.Contains("<ol>", nav);
        Assert.DoesNotContain("<li>", nav);
    }

    [Fact]
    public void Generer_UnChapitre_ContientLienCorrect()
    {
        var chapitres = new List<ChapitreNav>
        {
            new() { Titre = "Prologue", NomFichier = "corps_prologue.xhtml" }
        };

        var nav = NavGenerator.Generer(chapitres);

        Assert.Contains("<a href=\"text/corps_prologue.xhtml\">Prologue</a>", nav);
    }

    [Fact]
    public void Generer_PlusieursChapitres_TousPresents()
    {
        var chapitres = new List<ChapitreNav>
        {
            new() { Titre = "Prologue",   NomFichier = "corps_prologue.xhtml" },
            new() { Titre = "Chapitre 1", NomFichier = "corps_chapitre_1.xhtml" },
            new() { Titre = "Épilogue",   NomFichier = "corps_epilogue.xhtml" },
        };

        var nav = NavGenerator.Generer(chapitres);

        Assert.Contains("corps_prologue.xhtml", nav);
        Assert.Contains("corps_chapitre_1.xhtml", nav);
        Assert.Contains("corps_epilogue.xhtml", nav);
    }

    [Fact]
    public void Generer_OrdreConserve()
    {
        var chapitres = new List<ChapitreNav>
        {
            new() { Titre = "A", NomFichier = "a.xhtml" },
            new() { Titre = "B", NomFichier = "b.xhtml" },
        };

        var nav = NavGenerator.Generer(chapitres);

        Assert.True(nav.IndexOf("a.xhtml") < nav.IndexOf("b.xhtml"));
    }

    [Fact]
    public void Generer_TitreAvecCaracteresSpeciaux_EchappesEnXml()
    {
        var chapitres = new List<ChapitreNav>
        {
            new() { Titre = "Tom & Jerry <suite>", NomFichier = "ch.xhtml" }
        };

        var nav = NavGenerator.Generer(chapitres);

        Assert.Contains("Tom &amp; Jerry &lt;suite&gt;", nav);
    }
}
