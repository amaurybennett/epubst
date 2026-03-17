using System.Text;
using epubst.Models;
using Markdig;
using Markdig.Extensions.CustomContainers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace epubst.Parsing;

public static class MarkdownConverter
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseGenericAttributes()
        .UseCustomContainers()
        .Build();

    public static ConversionResult Convert(string markdown, bool navigation, string nomFichierBase, string? nomFichierCss = null)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        var blocs = document.ToList();

        return navigation
            ? ConvertirAvecNavigation(blocs, nomFichierBase, nomFichierCss)
            : ConvertirSansNavigation(blocs, nomFichierBase, nomFichierCss);
    }

    private static ConversionResult ConvertirSansNavigation(List<Block> blocs, string nomFichierBase, string? nomFichierCss)
    {
        var corps = RendreBlocs(blocs);
        var nomFichier = $"{nomFichierBase}.xhtml";
        return new ConversionResult
        {
            Documents = [new XhtmlDocument { NomFichier = nomFichier, Contenu = CreerEnveloppXhtml(nomFichierBase, nomFichierBase, corps, nomFichierCss) }],
            Chapitres = []
        };
    }

    private static ConversionResult ConvertirAvecNavigation(List<Block> blocs, string nomFichierBase, string? nomFichierCss)
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

                titreEnCours = ExtraireTexte(h.Inline);
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
            var nomFichier = $"{nomFichierBase}_{SanitiserNomFichier(titre)}.xhtml";
            var classeBody = Path.GetFileNameWithoutExtension(nomFichier);
            var corps = RendreBlocs(segmentBlocs);
            documents.Add(new XhtmlDocument { NomFichier = nomFichier, Contenu = CreerEnveloppXhtml(titre, classeBody, corps, nomFichierCss) });
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
                    sb.AppendLine($"  <h1{AttrClasse(h)}>{RendreInlines(h.Inline)}</h1>");
                    break;
                case HeadingBlock h when h.Level == 2:
                    sb.AppendLine($"  <hr{AttrClasse(h)}/>");
                    break;
                case ParagraphBlock p:
                    sb.AppendLine($"  <p{AttrClasse(p)}>{RendreInlines(p.Inline)}</p>");
                    break;
                case CustomContainer cc:
                    var classeDiv = cc.Info is not null ? $" class=\"{EscapeXml(cc.Info)}\"" : string.Empty;
                    sb.AppendLine($"  <div{classeDiv}>");
                    sb.Append(RendreBlocs(cc));
                    sb.AppendLine("  </div>");
                    break;
                // H3+ et blocs hors scope ignorés silencieusement
            }
        }
        return sb.ToString();
    }

    private static string AttrClasse(Block bloc)
    {
        var attrs = bloc.TryGetAttributes();
        if (attrs?.Classes == null || attrs.Classes.Count == 0) return string.Empty;
        return $" class=\"{string.Join(" ", attrs.Classes)}\"";
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
        LiteralInline lit => EscapeXml(lit.Content.ToString()),
        EmphasisInline em when em.DelimiterCount == 1 => $"<em>{RendreInlines(em)}</em>",
        EmphasisInline em => $"<strong>{RendreInlines(em)}</strong>",
        _ => string.Empty
    };

    private static string EscapeXml(string texte) =>
        texte.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static string ExtraireTexte(ContainerInline? inlines)
    {
        if (inlines == null) return string.Empty;
        var sb = new StringBuilder();
        foreach (var inline in inlines)
        {
            if (inline is LiteralInline lit)
                sb.Append(lit.Content.ToString());
            else if (inline is ContainerInline container)
                sb.Append(ExtraireTexte(container));
        }
        return sb.ToString();
    }

    private static string CreerEnveloppXhtml(string titre, string classeBody, string corps, string? nomFichierCss)
    {
        var titreEncode = EscapeXml(titre);
        var lienCssPersonnalise = nomFichierCss is not null
            ? $"\n  <link rel=\"stylesheet\" type=\"text/css\" href=\"../styles/{nomFichierCss}\"/>"
            : string.Empty;
        var classeSanitisee = SanitiserClasseCss(classeBody);
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <!DOCTYPE html>
            <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
            <head>
              <meta charset="utf-8"/>
              <title>{titreEncode}</title>
              <link rel="stylesheet" type="text/css" href="../styles/default.css"/>{lienCssPersonnalise}
            </head>
            <body class="{classeSanitisee}">
            {corps}</body>
            </html>
            """;
    }

    private static string SanitiserNomFichier(string valeur)
    {
        var sansAccents = valeur.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in sansAccents)
        {
            if (c >= 'a' && c <= 'z' || c >= '0' && c <= '9' || c == '-' || c == '_')
                sb.Append(c);
            else if (c >= 'A' && c <= 'Z')
                sb.Append(char.ToLowerInvariant(c));
            else if (c == ' ')
                sb.Append('_');
        }
        return sb.ToString();
    }

    private static string SanitiserClasseCss(string valeur)
    {
        var sansAccents = valeur.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in sansAccents)
        {
            if (c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '-' || c == '_')
                sb.Append(c);
        }
        return sb.ToString();
    }
}
