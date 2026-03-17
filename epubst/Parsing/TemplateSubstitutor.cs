using System.Text.RegularExpressions;
using epubst.Models;

namespace epubst.Parsing;

public static partial class TemplateSubstitutor
{
    [GeneratedRegex(@"%%meta\.([a-z_]+)%%")]
    private static partial Regex TagRegex();

    public static string Substituer(string markdown, Metadonnees meta)
    {
        return TagRegex().Replace(markdown, match =>
        {
            var cle = match.Groups[1].Value;
            var valeur = ResoudreValeur(cle, meta);

            if (valeur is null)
            {
                Console.WriteLine($"Avertissement : la variable '{match.Value}' n'a pas de valeur dans les métadonnées.");
                return match.Value;
            }

            return valeur;
        });
    }

    private static string? ResoudreValeur(string cle, Metadonnees meta) => cle switch
    {
        "titre"            => meta.Titre,
        "auteurs"          => string.Join(", ", meta.Auteurs),
        "langue"           => meta.Langue,
        "isbn"             => meta.Isbn,
        "editeur"          => meta.Editeur,
        "date_publication" => meta.DatePublication,
        "sous_titre"       => meta.SousTitre,
        "serie"            => meta.Serie,
        "numero_tome"      => meta.NumeroTome?.ToString(),
        "beta_lecture"     => meta.BetaLecture,
        "correction"       => meta.Correction,
        "couverture"       => meta.Couverture,
        "copyright"        => meta.Copyright?.ToString(),
        _                  => null
    };
}
