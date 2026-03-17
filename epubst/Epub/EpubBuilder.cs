using System.IO.Compression;
using System.Text;
using epubst.Assets;
using epubst.Models;
using epubst.Parsing;

namespace epubst.Epub;

public static class EpubBuilder
{
    public static void Compiler(BookProject projet, DirectoryInfo projectDir, Stream sortie)
    {
        var (documents, chapitres, images) = ConvertirContenu(projet, projectDir);

        var coverXhtml = CoverGenerator.Generer(projet.EpubOptions.Couverture.Name);
        var navXhtml   = NavGenerator.Generer(chapitres);
        var opf        = OpfGenerator.Generer(projet, documents, images);

        AssemblerZip(sortie, projet, coverXhtml, navXhtml, opf, documents, images);
    }

    private static (List<XhtmlDocument> Documents, List<ChapitreNav> Chapitres, List<FileInfo> Images)
        ConvertirContenu(BookProject projet, DirectoryInfo projectDir)
    {
        var documents = new List<XhtmlDocument>();
        var chapitres = new List<ChapitreNav>();
        var images    = new List<FileInfo>();

        foreach (var item in projet.Contenu)
        {
            var markdown = File.ReadAllText(item.Fichier.FullName);
            var nomBase  = Path.GetFileNameWithoutExtension(item.Fichier.Name);
            var nomCss   = projet.EpubOptions.Css?.Name;
            var result   = MarkdownConverter.Convert(markdown, item.Navigation, nomBase, projectDir, nomCss, projet.Metadonnees);

            documents.AddRange(result.Documents);
            chapitres.AddRange(result.Chapitres);

            foreach (var img in result.Images)
                if (!images.Any(f => f.FullName == img.FullName))
                    images.Add(img);
        }

        return (documents, chapitres, images);
    }

    private static void AssemblerZip(Stream sortie, BookProject projet, string coverXhtml, string navXhtml, string opf,
        List<XhtmlDocument> documents, List<FileInfo> images)
    {
        using var archive = new ZipArchive(sortie, ZipArchiveMode.Create, leaveOpen: true);

        // mimetype — non compressé, en premier (obligation ePub)
        EcrireTexte(archive, "mimetype", "application/epub+zip", CompressionLevel.NoCompression);

        EcrireTexte(archive, "META-INF/container.xml", ContainerXml());
        EcrireTexte(archive, "OEBPS/content.opf",      opf);
        EcrireTexte(archive, "OEBPS/nav.xhtml",         navXhtml);
        EcrireTexte(archive, "OEBPS/cover.xhtml",       coverXhtml);
        EcrireTexte(archive, "OEBPS/styles/default.css", EmbeddedAssets.DefaultCss);

        if (projet.EpubOptions.Css is not null)
            EcrireFichier(archive, $"OEBPS/styles/{projet.EpubOptions.Css.Name}", projet.EpubOptions.Css);

        EcrireFichier(archive, $"OEBPS/images/{projet.EpubOptions.Couverture.Name}", projet.EpubOptions.Couverture);

        foreach (var img in images)
            EcrireFichier(archive, $"OEBPS/images/{img.Name}", img);

        foreach (var doc in documents)
            EcrireTexte(archive, $"OEBPS/text/{doc.NomFichier}", doc.Contenu);
    }

    private static void EcrireTexte(ZipArchive archive, string chemin, string contenu,
        CompressionLevel niveau = CompressionLevel.Optimal)
    {
        var entree = archive.CreateEntry(chemin, niveau);
        using var writer = new StreamWriter(entree.Open(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        writer.Write(contenu);
    }

    private static void EcrireFichier(ZipArchive archive, string cheminDansZip, FileInfo fichier)
    {
        archive.CreateEntryFromFile(fichier.FullName, cheminDansZip);
    }

    private static string ContainerXml() => """
        <?xml version="1.0" encoding="utf-8"?>
        <container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
          <rootfiles>
            <rootfile full-path="OEBPS/content.opf" media-type="application/oebps-package+xml"/>
          </rootfiles>
        </container>
        """;
}
