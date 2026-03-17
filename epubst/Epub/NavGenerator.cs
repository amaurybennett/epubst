using System.Text;
using epubst.Models;

namespace epubst.Epub;

public static class NavGenerator
{
    public static string Generer(IReadOnlyList<ChapitreNav> chapitres)
    {
        var entrees = new StringBuilder();
        foreach (var chapitre in chapitres)
            entrees.AppendLine($"      <li><a href=\"text/{chapitre.NomFichier}\">{EscapeXml(chapitre.Titre)}</a></li>");

        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE html>
            <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
            <head>
              <meta charset="utf-8"/>
              <title>Table des matières</title>
            </head>
            <body>
              <nav epub:type="toc" id="toc">
                <ol>
            {entrees}    </ol>
              </nav>
            </body>
            </html>
            """;
    }

    private static string EscapeXml(string texte) =>
        texte.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
