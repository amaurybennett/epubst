using System.Net;
using System.Text;
using epubst.Models;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace epubst.Parsing;

public static class MarkdownConverter
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .Build();

    public static ConversionResult Convert(string markdown, bool navigation, string nomFichierBase)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        var blocs = document.ToList();

        return navigation
            ? ConvertirAvecNavigation(blocs, nomFichierBase)
            : ConvertirSansNavigation(blocs, nomFichierBase);
    }

    private static ConversionResult ConvertirSansNavigation(List<Block> blocs, string nomFichierBase)
    {
        var corps = RendreBlocs(blocs);
        var nomFichier = $"{nomFichierBase}.xhtml";
        return new ConversionResult
        {
            Documents = [new XhtmlDocument { NomFichier = nomFichier, Contenu = CreerEnveloppXhtml(nomFichierBase, corps) }],
            Chapitres = []
        };
    }

    private static ConversionResult ConvertirAvecNavigation(List<Block> blocs, string nomFichierBase)
    {
        // Segmenter par H1 — contenu avant le premier H1 ignoré (option C)
        var segments = new List<(string Titre, List<Block> Blocs)>();
        string? titreEnCours = null;
        var blocsEnCours = new List<Block>();

        foreach (var bloc in blocs)
        {
            if (bloc is HeadingBlock h && h.Level == 1)
            {
                if (titreEnCours != null)
                    segments.Add((titreEnCours, blocsEnCours));

                titreEnCours = RendreInlines(h.Inline);
                blocsEnCours = [bloc];
            }
            else if (titreEnCours != null)
            {
                blocsEnCours.Add(bloc);
            }
            // contenu avant le premier H1 → ignoré
        }

        if (titreEnCours != null)
            segments.Add((titreEnCours, blocsEnCours));

        var documents = new List<XhtmlDocument>();
        var chapitres = new List<ChapitreNav>();

        for (int i = 0; i < segments.Count; i++)
        {
            var (titre, segmentBlocs) = segments[i];
            var nomFichier = $"{nomFichierBase}_ch{i + 1:D3}.xhtml";
            var corps = RendreBlocs(segmentBlocs);
            documents.Add(new XhtmlDocument { NomFichier = nomFichier, Contenu = CreerEnveloppXhtml(titre, corps) });
            chapitres.Add(new ChapitreNav { Titre = titre, NomFichier = nomFichier });
        }

        return new ConversionResult { Documents = documents, Chapitres = chapitres };
    }

    private static string RendreBlocs(IEnumerable<Block> blocs)
    {
        var sb = new StringBuilder();
        foreach (var bloc in blocs)
        {
            switch (bloc)
            {
                case HeadingBlock h when h.Level == 1:
                    sb.AppendLine($"  <h1>{RendreInlines(h.Inline)}</h1>");
                    break;
                case HeadingBlock h when h.Level == 2:
                    sb.AppendLine("  <hr/>");
                    break;
                case ParagraphBlock p:
                    sb.AppendLine($"  <p>{RendreInlines(p.Inline)}</p>");
                    break;
                // H3+ et blocs hors scope ignorés silencieusement
            }
        }
        return sb.ToString();
    }

    private static string RendreInlines(ContainerInline? inlines)
    {
        if (inlines == null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var inline in inlines)
            sb.Append(RendreInline(inline));
        return sb.ToString();
    }

    private static string RendreInline(Inline inline) => inline switch
    {
        LiteralInline lit => WebUtility.HtmlEncode(lit.Content.ToString()),
        EmphasisInline em when em.DelimiterCount == 1 => $"<em>{RendreInlines(em)}</em>",
        EmphasisInline em => $"<strong>{RendreInlines(em)}</strong>",
        _ => string.Empty
    };

    private static string CreerEnveloppXhtml(string titre, string corps)
    {
        var titreEncode = WebUtility.HtmlEncode(titre);
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE html>
            <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
            <head>
              <meta charset="utf-8"/>
              <title>{titreEncode}</title>
              <link rel="stylesheet" type="text/css" href="../styles/default.css"/>
            </head>
            <body>
            {corps}</body>
            </html>
            """;
    }
}
