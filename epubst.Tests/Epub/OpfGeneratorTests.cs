using epubst.Epub;
using epubst.Models;

namespace epubst.Tests.Epub;

public class OpfGeneratorTests
{
    private static BookProject ProjetMinimal() => new()
    {
        Metadonnees = new Metadonnees
        {
            Titre = "Mon Roman",
            Auteurs = ["Jean Dupont"],
            Langue = "fr"
        },
        EpubOptions = new EpubOptions
        {
            Couverture = new FileInfo("cover.jpg")
        },
        Contenu = []
    };

    // ========== Structure générale ==========

    [Fact]
    public void Generer_ContientDeclarationXml()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", opf);
    }

    [Fact]
    public void Generer_ContientBalisePackage()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("<package", opf);
        Assert.Contains("version=\"3.0\"", opf);
    }

    // ========== Métadonnées ==========

    [Fact]
    public void Generer_ContientTitre()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("<dc:title>Mon Roman</dc:title>", opf);
    }

    [Fact]
    public void Generer_ContientAuteur()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("<dc:creator>Jean Dupont</dc:creator>", opf);
    }

    [Fact]
    public void Generer_PlusieursAuteurs_TousPresents()
    {
        var projet = ProjetMinimal() with
        {
            Metadonnees = ProjetMinimal().Metadonnees with
            {
                Auteurs = ["Alice", "Bob"]
            }
        };

        var opf = OpfGenerator.Generer(projet, [], []);

        Assert.Contains("<dc:creator>Alice</dc:creator>", opf);
        Assert.Contains("<dc:creator>Bob</dc:creator>", opf);
    }

    [Fact]
    public void Generer_ContientLangue()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("<dc:language>fr</dc:language>", opf);
    }

    [Fact]
    public void Generer_AvecIsbn_ContientIdentifiantIsbn()
    {
        var projet = ProjetMinimal() with
        {
            Metadonnees = ProjetMinimal().Metadonnees with { Isbn = "978-2-07-054127-2" }
        };

        var opf = OpfGenerator.Generer(projet, [], []);

        Assert.Contains("isbn:978-2-07-054127-2", opf);
    }

    [Fact]
    public void Generer_SansIsbn_ContientIdentifiantUuid()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("urn:uuid:", opf);
    }

    [Fact]
    public void Generer_ContientDctermsModified()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("dcterms:modified", opf);
    }

    // ========== Manifest ==========

    [Fact]
    public void Generer_ManifestContientNav()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("href=\"nav.xhtml\"", opf);
        Assert.Contains("properties=\"nav\"", opf);
    }

    [Fact]
    public void Generer_ManifestContientCoverPage()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("href=\"cover.xhtml\"", opf);
    }

    [Fact]
    public void Generer_ManifestContientCoverImage()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("href=\"images/cover.jpg\"", opf);
        Assert.Contains("properties=\"cover-image\"", opf);
    }

    [Fact]
    public void Generer_ManifestContientDefaultCss()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.Contains("href=\"styles/default.css\"", opf);
    }

    [Fact]
    public void Generer_AvecCssPersonnalise_ManifestContientLesCss()
    {
        var projet = ProjetMinimal() with
        {
            EpubOptions = ProjetMinimal().EpubOptions with { Css = new FileInfo("style.css") }
        };

        var opf = OpfGenerator.Generer(projet, [], []);

        Assert.Contains("href=\"styles/style.css\"", opf);
    }

    [Fact]
    public void Generer_AvecDocuments_ManifestContientTousLesDocuments()
    {
        var docs = new List<XhtmlDocument>
        {
            new() { NomFichier = "corps_prologue.xhtml",   Contenu = "" },
            new() { NomFichier = "corps_chapitre_1.xhtml", Contenu = "" },
        };

        var opf = OpfGenerator.Generer(ProjetMinimal(), docs, []);

        Assert.Contains("href=\"text/corps_prologue.xhtml\"", opf);
        Assert.Contains("href=\"text/corps_chapitre_1.xhtml\"", opf);
    }

    [Fact]
    public void Generer_AvecImages_ManifestContientLesImages()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var img = new FileInfo(Path.Combine(dir.FullName, "logo.png"));
            File.WriteAllText(img.FullName, "");

            var opf = OpfGenerator.Generer(ProjetMinimal(), [], [img]);

            Assert.Contains("href=\"images/logo.png\"", opf);
            Assert.Contains("media-type=\"image/png\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    // ========== Spine ==========

    [Fact]
    public void Generer_SpineCommenceParCoverPage()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        var idxSpine = opf.IndexOf("<spine>");
        var idxCover = opf.IndexOf("idref=\"cover-page\"");
        Assert.True(idxCover > idxSpine);
    }

    [Fact]
    public void Generer_SpineContientTousLesDocuments()
    {
        var docs = new List<XhtmlDocument>
        {
            new() { NomFichier = "ch1.xhtml", Contenu = "" },
            new() { NomFichier = "ch2.xhtml", Contenu = "" },
        };

        var opf = OpfGenerator.Generer(ProjetMinimal(), docs, []);

        Assert.Contains("idref=\"doc-0\"", opf);
        Assert.Contains("idref=\"doc-1\"", opf);
    }

    // ========== MediaType ==========

    [Fact]
    public void Generer_ImageJpeg_MediaTypeCorrect()
    {
        var projet = ProjetMinimal() with
        {
            EpubOptions = ProjetMinimal().EpubOptions with { Couverture = new FileInfo("cover.jpeg") }
        };

        var opf = OpfGenerator.Generer(projet, [], []);

        Assert.Contains("media-type=\"image/jpeg\"", opf);
    }

    [Fact]
    public void Generer_ImagePng_MediaTypeCorrect()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var img = new FileInfo(Path.Combine(dir.FullName, "logo.png"));
            File.WriteAllText(img.FullName, "");

            var opf = OpfGenerator.Generer(ProjetMinimal(), [], [img]);

            Assert.Contains("media-type=\"image/png\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    // ========== CSS Fontes ==========

    [Fact]
    public void Generer_SansFontes_PasDentreCssFontes()
    {
        var opf = OpfGenerator.Generer(ProjetMinimal(), [], []);

        Assert.DoesNotContain("fontes.css", opf);
    }

    [Fact]
    public void Generer_AvecFontes_ManifestContientCssFontes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var fichier = new FileInfo(Path.Combine(dir.FullName, "mafonte.otf"));
            File.WriteAllText(fichier.FullName, "");
            var projet = ProjetMinimal() with
            {
                Fontes = [new FonteItem { Nom = "Ma Fonte", Fichier = fichier }]
            };

            var opf = OpfGenerator.Generer(projet, [], []);

            Assert.Contains("href=\"styles/fontes.css\"", opf);
            Assert.Contains("id=\"css-fontes\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    // ========== Fontes ==========

    [Fact]
    public void Generer_AvecFonteOtf_ManifestContientEntreeFont()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var fichier = new FileInfo(Path.Combine(dir.FullName, "mafonte.otf"));
            File.WriteAllText(fichier.FullName, "");
            var projet = ProjetMinimal() with
            {
                Fontes = [new FonteItem { Nom = "Ma Fonte", Fichier = fichier }]
            };

            var opf = OpfGenerator.Generer(projet, [], []);

            Assert.Contains("href=\"fonts/mafonte.otf\"", opf);
            Assert.Contains("media-type=\"font/otf\"", opf);
            Assert.Contains("id=\"font-Ma Fonte\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Generer_AvecFonteTtf_MediaTypeCorrect()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var fichier = new FileInfo(Path.Combine(dir.FullName, "mafonte.ttf"));
            File.WriteAllText(fichier.FullName, "");
            var projet = ProjetMinimal() with
            {
                Fontes = [new FonteItem { Nom = "Ma Fonte", Fichier = fichier }]
            };

            var opf = OpfGenerator.Generer(projet, [], []);

            Assert.Contains("media-type=\"font/ttf\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Generer_AvecFonteWoff2_MediaTypeCorrect()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var fichier = new FileInfo(Path.Combine(dir.FullName, "mafonte.woff2"));
            File.WriteAllText(fichier.FullName, "");
            var projet = ProjetMinimal() with
            {
                Fontes = [new FonteItem { Nom = "Ma Fonte", Fichier = fichier }]
            };

            var opf = OpfGenerator.Generer(projet, [], []);

            Assert.Contains("media-type=\"font/woff2\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }

    [Fact]
    public void Generer_PlusieursPolices_ToutesPresentes()
    {
        var dir = Directory.CreateTempSubdirectory("epubst_test_");
        try
        {
            var f1 = new FileInfo(Path.Combine(dir.FullName, "regular.otf"));
            var f2 = new FileInfo(Path.Combine(dir.FullName, "bold.otf"));
            File.WriteAllText(f1.FullName, "");
            File.WriteAllText(f2.FullName, "");
            var projet = ProjetMinimal() with
            {
                Fontes =
                [
                    new FonteItem { Nom = "Regular", Fichier = f1 },
                    new FonteItem { Nom = "Bold",    Fichier = f2 }
                ]
            };

            var opf = OpfGenerator.Generer(projet, [], []);

            Assert.Contains("href=\"fonts/regular.otf\"", opf);
            Assert.Contains("href=\"fonts/bold.otf\"", opf);
        }
        finally { dir.Delete(recursive: true); }
    }
}
