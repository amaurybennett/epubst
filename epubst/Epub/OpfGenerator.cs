using System.Text;
using epubst.Models;

namespace epubst.Epub;

public static class OpfGenerator
{
    public static string Generer(BookProject projet, IReadOnlyList<XhtmlDocument> documents, IReadOnlyList<FileInfo> images)
    {
        var meta = projet.Metadonnees;
        var epub = projet.EpubOptions;

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <package xmlns="http://www.idpf.org/2007/opf" version="3.0" unique-identifier="uid">
            {GenererMetadata(meta, epub)}
            {GenererManifest(epub, documents, images, projet.Fontes)}
            {GenererSpine(documents)}
            </package>
            """;
    }

    private static string GenererMetadata(Metadonnees meta, EpubOptions epub)
    {
        var sb = new StringBuilder();
        sb.AppendLine("  <metadata xmlns:dc=\"http://purl.org/dc/elements/1.1/\">");

        var identifiant = meta.Isbn is not null ? $"isbn:{meta.Isbn}" : $"urn:uuid:{Guid.NewGuid()}";
        sb.AppendLine($"    <dc:identifier id=\"uid\">{EscapeXml(identifiant)}</dc:identifier>");
        sb.AppendLine($"    <dc:title>{EscapeXml(meta.Titre)}</dc:title>");

        foreach (var auteur in meta.Auteurs)
            sb.AppendLine($"    <dc:creator>{EscapeXml(auteur)}</dc:creator>");

        sb.AppendLine($"    <dc:language>{EscapeXml(meta.Langue)}</dc:language>");

        if (meta.Editeur is not null)
            sb.AppendLine($"    <dc:publisher>{EscapeXml(meta.Editeur)}</dc:publisher>");

        if (meta.DatePublication is not null)
            sb.AppendLine($"    <dc:date>{EscapeXml(meta.DatePublication)}</dc:date>");

        sb.AppendLine($"    <meta property=\"dcterms:modified\">{DateTimeOffset.UtcNow:yyyy-MM-ddTHH:mm:ssZ}</meta>");
        sb.Append("  </metadata>");
        return sb.ToString();
    }

    private static string GenererManifest(EpubOptions epub, IReadOnlyList<XhtmlDocument> documents, IReadOnlyList<FileInfo> images, IReadOnlyList<FonteItem> fontes)
    {
        var sb = new StringBuilder();
        sb.AppendLine("  <manifest>");

        sb.AppendLine("    <item id=\"nav\" href=\"nav.xhtml\" media-type=\"application/xhtml+xml\" properties=\"nav\"/>");
        sb.AppendLine("    <item id=\"cover-page\" href=\"cover.xhtml\" media-type=\"application/xhtml+xml\"/>");
        sb.AppendLine($"    <item id=\"cover-img\" href=\"images/{EscapeXml(epub.Couverture.Name)}\" media-type=\"{MediaType(epub.Couverture.Name)}\" properties=\"cover-image\"/>");
        sb.AppendLine("    <item id=\"css-default\" href=\"styles/default.css\" media-type=\"text/css\"/>");

        if (fontes.Count > 0)
            sb.AppendLine("    <item id=\"css-fontes\" href=\"styles/fontes.css\" media-type=\"text/css\"/>");

        if (epub.Css is not null)
            sb.AppendLine($"    <item id=\"css-custom\" href=\"styles/{EscapeXml(epub.Css.Name)}\" media-type=\"text/css\"/>");

        for (int i = 0; i < images.Count; i++)
            sb.AppendLine($"    <item id=\"img-{i}\" href=\"images/{EscapeXml(images[i].Name)}\" media-type=\"{MediaType(images[i].Name)}\"/>");

        for (int i = 0; i < documents.Count; i++)
            sb.AppendLine($"    <item id=\"doc-{i}\" href=\"text/{EscapeXml(documents[i].NomFichier)}\" media-type=\"application/xhtml+xml\"/>");

        foreach (var fonte in fontes)
            sb.AppendLine($"    <item id=\"font-{EscapeXml(fonte.Nom)}\" href=\"fonts/{EscapeXml(fonte.Fichier.Name)}\" media-type=\"{MediaTypeFonte(fonte.Fichier.Name)}\"/>");

        sb.Append("  </manifest>");
        return sb.ToString();
    }

    private static string GenererSpine(IReadOnlyList<XhtmlDocument> documents)
    {
        var sb = new StringBuilder();
        sb.AppendLine("  <spine>");
        sb.AppendLine("    <itemref idref=\"cover-page\"/>");

        for (int i = 0; i < documents.Count; i++)
            sb.AppendLine($"    <itemref idref=\"doc-{i}\"/>");

        sb.Append("  </spine>");
        return sb.ToString();
    }

    private static string MediaType(string nomFichier) =>
        Path.GetExtension(nomFichier).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".gif"            => "image/gif",
            ".svg"            => "image/svg+xml",
            ".webp"           => "image/webp",
            _                 => "application/octet-stream"
        };

    private static string MediaTypeFonte(string nomFichier) =>
        Path.GetExtension(nomFichier).ToLowerInvariant() switch
        {
            ".otf"   => "font/otf",
            ".ttf"   => "font/ttf",
            ".woff"  => "font/woff",
            ".woff2" => "font/woff2",
            _        => "application/octet-stream"
        };

    private static string EscapeXml(string texte) =>
        texte.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
