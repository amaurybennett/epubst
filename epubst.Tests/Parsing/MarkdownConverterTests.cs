using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class MarkdownConverterTests
{
    // ========== Rendu des éléments de base ==========

    [Fact]
    public void Convert_ParagrapheSimple_RenduEnP()
    {
        var result = MarkdownConverter.Convert("Hello world.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<p>Hello world.</p>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_Italique_RenduEnEm()
    {
        var result = MarkdownConverter.Convert("Texte *italique* ici.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<em>italique</em>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_Gras_RenduEnStrong()
    {
        var result = MarkdownConverter.Convert("Texte **gras** ici.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<strong>gras</strong>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_H1_RenduEnH1()
    {
        var result = MarkdownConverter.Convert("# Titre chapitre", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<h1>Titre chapitre</h1>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_H2_RenduEnHr()
    {
        var result = MarkdownConverter.Convert("## Séparateur de scène", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<hr/>", result.Documents[0].Contenu);
        Assert.DoesNotContain("Séparateur de scène", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_BlocsHorsScope_AbsentsSansErreur()
    {
        var markdown = """
            - item liste
            - autre item

            Paragraphe normal.
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<p>Paragraphe normal.</p>", result.Documents[0].Contenu);
        Assert.DoesNotContain("<ul>", result.Documents[0].Contenu);
        Assert.DoesNotContain("<li>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_CaracteresSpeciaux_EchappesEnXhtml()
    {
        var result = MarkdownConverter.Convert("Tom & Jerry <test>.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("Tom &amp; Jerry &lt;test&gt;.", result.Documents[0].Contenu);
    }

    // ========== Mode navigation = false ==========

    [Fact]
    public void Convert_NavigationFalse_RetourneUnSeulDocument()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Single(result.Documents);
    }

    [Fact]
    public void Convert_NavigationFalse_ChapitresVide()
    {
        var result = MarkdownConverter.Convert("# H1\n\nContenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Empty(result.Chapitres);
    }

    [Fact]
    public void Convert_NavigationFalse_PlusieursH1_PasDeDecoupage()
    {
        var markdown = """
            # Chapitre 1

            Contenu 1.

            # Chapitre 2

            Contenu 2.
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Single(result.Documents);
        Assert.Contains("<h1>Chapitre 1</h1>", result.Documents[0].Contenu);
        Assert.Contains("<h1>Chapitre 2</h1>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_NavigationFalse_NomFichierCorrect()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Equal("remerciements.xhtml", result.Documents[0].NomFichier);
    }

    // ========== Mode navigation = true ==========

    [Fact]
    public void Convert_NavigationTrue_TroisH1_TroisDocuments()
    {
        var markdown = """
            # Chapitre 1

            Contenu 1.

            # Chapitre 2

            Contenu 2.

            # Chapitre 3

            Contenu 3.
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Equal(3, result.Documents.Count);
    }

    [Fact]
    public void Convert_NavigationTrue_TroisH1_TroisChapitresAvecBonsTitres()
    {
        var markdown = "# Chapitre 1\n\n# Chapitre 2\n\n# Chapitre 3";

        var result = MarkdownConverter.Convert(markdown, navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Equal(3, result.Chapitres.Count);
        Assert.Equal("Chapitre 1", result.Chapitres[0].Titre);
        Assert.Equal("Chapitre 2", result.Chapitres[1].Titre);
        Assert.Equal("Chapitre 3", result.Chapitres[2].Titre);
    }

    [Fact]
    public void Convert_NavigationTrue_NomsFichiersDerivesDesTitres()
    {
        var markdown = "# Prologue\n\n# Chapitre 42\n\n# Épilogue";

        var result = MarkdownConverter.Convert(markdown, navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Equal("corps_prologue.xhtml", result.Documents[0].NomFichier);
        Assert.Equal("corps_chapitre_42.xhtml", result.Documents[1].NomFichier);
        Assert.Equal("corps_epilogue.xhtml", result.Documents[2].NomFichier);
    }

    [Fact]
    public void Convert_NavigationTrue_ChapitresReferencentBonsNomsFichiers()
    {
        var markdown = "# Ch1\n\n# Ch2";

        var result = MarkdownConverter.Convert(markdown, navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Equal(result.Documents[0].NomFichier, result.Chapitres[0].NomFichier);
        Assert.Equal(result.Documents[1].NomFichier, result.Chapitres[1].NomFichier);
    }

    [Fact]
    public void Convert_NavigationTrue_ContenuAvantPremierH1_Ignore()
    {
        var markdown = """
            Ceci est du contenu orphelin.

            # Premier chapitre

            Contenu du chapitre.
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Single(result.Documents);
        Assert.DoesNotContain("contenu orphelin", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_NavigationTrue_SansH1_DocumentsEtChapitresVides()
    {
        var result = MarkdownConverter.Convert("Juste un paragraphe sans H1.", navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Empty(result.Documents);
        Assert.Empty(result.Chapitres);
    }

    // ========== Validité XHTML ==========

    [Fact]
    public void Convert_XhtmlContientDeclarationXml()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_XhtmlContientNamespaceXhtml()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("xmlns=\"http://www.w3.org/1999/xhtml\"", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_H2_RenduBaliseAutoFermante()
    {
        var result = MarkdownConverter.Convert("## Scène", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<hr/>", result.Documents[0].Contenu);
        Assert.DoesNotContain("<hr>", result.Documents[0].Contenu);
    }

    // ========== GenericAttributes ==========

    [Fact]
    public void Convert_GenericAttributes_ParagrapheAvecClasse()
    {
        var result = MarkdownConverter.Convert("{.dedicace}\nÀ mes parents.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<p class=\"dedicace\">À mes parents.</p>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_GenericAttributes_ParagraphePlusieursClasses()
    {
        var result = MarkdownConverter.Convert("{.premiere .importante}\nTexte.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("class=\"premiere importante\"", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_GenericAttributes_H1AvecClasse()
    {
        var markdown = "# Prologue {.ouverture}";

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<h1 class=\"ouverture\">Prologue</h1>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_GenericAttributes_H2AvecClasse()
    {
        var markdown = """
            ## {.fondu}
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<hr class=\"fondu\"/>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_GenericAttributes_SansClasse_PasAttributClass()
    {
        var result = MarkdownConverter.Convert("Texte normal.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<p>Texte normal.</p>", result.Documents[0].Contenu);
    }

    // ========== CustomContainers ==========

    [Fact]
    public void Convert_CustomContainer_RenduEnDivAvecClasse()
    {
        var markdown = """
            ::: exergue
            Il faisait nuit noire.
            :::
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("<div class=\"exergue\">", result.Documents[0].Contenu);
        Assert.Contains("</div>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_CustomContainer_ContenuRenduDansDiv()
    {
        var markdown = """
            ::: exergue
            Il faisait nuit noire.

            Les etoiles disparaissaient.
            :::
            """;

        var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        var contenu = result.Documents[0].Contenu;
        Assert.Contains("<p>Il faisait nuit noire.</p>", contenu);
        Assert.Contains("<p>Les etoiles disparaissaient.</p>", contenu);
    }

    // ========== Classe CSS du body ==========

    [Fact]
    public void Convert_NavigationFalse_BodyContientClasseAvecNomFichier()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Contains("class=\"remerciements\"", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_NavigationTrue_BodyContientClasseAvecNomFichierEtTitre()
    {
        var result = MarkdownConverter.Convert("# Prologue", navigation: true, nomFichierBase: "corps", new DirectoryInfo("."));

        Assert.Contains("class=\"corps_prologue\"", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_NomFichierAvecAccents_ClasseSanitisee()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "à_propos", new DirectoryInfo("."));

        Assert.Contains("class=\"a_propos\"", result.Documents[0].Contenu);
    }

    // ========== Images ==========

    [Fact]
    public void Convert_Image_RendueEnImgXhtml()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "logo.png"), "");
            var result = MarkdownConverter.Convert("![Mon logo](logo.png)", navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName));

            Assert.Contains("<img src=\"../images/logo.png\" alt=\"Mon logo\"/>", result.Documents[0].Contenu);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Convert_Image_AjouteeALaListeImages()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "logo.png"), "");
            var result = MarkdownConverter.Convert("![Logo](logo.png)", navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName));

            Assert.Single(result.Images);
            Assert.Equal("logo.png", result.Images[0].Name);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Convert_ImageDansSousDossier_CheminRelatifResolu()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            Directory.CreateDirectory(Path.Combine(dir.FullName, "assets"));
            File.WriteAllText(Path.Combine(dir.FullName, "assets", "logo.png"), "");
            var result = MarkdownConverter.Convert("![Logo](assets/logo.png)", navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName));

            Assert.Contains("<img src=\"../images/logo.png\"", result.Documents[0].Contenu);
            Assert.Equal("logo.png", result.Images[0].Name);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Convert_MemeImagePlusieursFois_DedupliqueDansListe()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            File.WriteAllText(Path.Combine(dir.FullName, "logo.png"), "");
            var markdown = "![A](logo.png)\n\n![B](logo.png)";
            var result = MarkdownConverter.Convert(markdown, navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName));

            Assert.Single(result.Images);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Convert_ImageIntrouvable_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            Assert.Throws<FileNotFoundException>(() =>
                MarkdownConverter.Convert("![Logo](inexistant.png)", navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName)));
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Convert_ImagePathTraversal_LanceException()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            Assert.Throws<InvalidOperationException>(() =>
                MarkdownConverter.Convert("![Logo](../../secret.png)", navigation: false, nomFichierBase: "test", new DirectoryInfo(dir.FullName)));
        }
        finally { dir.Delete(recursive: true); }
    }

    // ========== CSS ==========

    [Fact]
    public void Convert_SansCssPersonnalise_SeulDefaultCssPresent()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.Contains("default.css", result.Documents[0].Contenu);
        Assert.Single(result.Documents[0].Contenu.Split("stylesheet").Skip(1).ToList());
    }

    [Fact]
    public void Convert_AvecCssPersonnalise_DeuxLiensCss()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."), nomFichierCss: "style.css");

        Assert.Contains("default.css", result.Documents[0].Contenu);
        Assert.Contains("style.css", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_SansTitreOverride_TitreEstNomFichier()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."));

        Assert.Contains("<title>remerciements</title>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_AvecTitreOverride_TitreUtilise()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."), titreOverride: "Remerciements");

        Assert.Contains("<title>Remerciements</title>", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_TitreOverride_NomFichierInchange()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "remerciements", new DirectoryInfo("."), titreOverride: "Remerciements");

        Assert.Equal("remerciements.xhtml", result.Documents[0].NomFichier);
    }

    [Fact]
    public void Convert_SansFontesCss_LienFontesCssAbsent()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."));

        Assert.DoesNotContain("fontes.css", result.Documents[0].Contenu);
    }

    [Fact]
    public void Convert_AvecFontesCss_LienFontesCssPresentApresDefault()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."), avecFontesCss: true);

        var contenu = result.Documents[0].Contenu;
        Assert.Contains("fontes.css", contenu);
        Assert.True(contenu.IndexOf("default.css") < contenu.IndexOf("fontes.css"));
    }

    [Fact]
    public void Convert_AvecFontesCssEtCssPersonnalise_TroisLiensCss()
    {
        var result = MarkdownConverter.Convert("Contenu.", navigation: false, nomFichierBase: "test", new DirectoryInfo("."), nomFichierCss: "style.css", avecFontesCss: true);

        var contenu = result.Documents[0].Contenu;
        Assert.Contains("default.css", contenu);
        Assert.Contains("fontes.css", contenu);
        Assert.Contains("style.css", contenu);
    }
}
