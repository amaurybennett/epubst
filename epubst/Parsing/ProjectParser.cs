using System.Text.Json;
using epubst.Models;
using Tomlyn;

namespace epubst.Parsing;

public static class ProjectParser
{
    private static readonly TomlSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public static Metadonnees ParseMetadonnees(string toml)
    {
        var projet = TomlSerializer.Deserialize<ProjetToml>(toml, Options)
            ?? throw new InvalidOperationException("Le fichier book.toml est invalide.");
        return projet.Metadonnees ?? throw new InvalidOperationException("La section [metadonnees] est absente du fichier book.toml.");
    }

    public static EpubOptions ParseEpubOptions(string toml, DirectoryInfo projectDir)
    {
        var projet = TomlSerializer.Deserialize<ProjetToml>(toml, Options)
            ?? throw new InvalidOperationException("Le fichier book.toml est invalide.");
        var epub = projet.Epub ?? throw new InvalidOperationException("La section [epub] est absente du fichier book.toml.");

        if (string.IsNullOrWhiteSpace(epub.Couverture))
            throw new InvalidOperationException("Le champ 'couverture' est absent de la section [epub].");

        var couverture = ResoudreCheminFichier(epub.Couverture, projectDir, "couverture");
        var css = epub.Css is not null ? ResoudreCheminFichier(epub.Css, projectDir, "css") : null;

        return new EpubOptions
        {
            Couverture = couverture,
            Css = css,
            TableDesMatieres = epub.TableDesMatieres
        };
    }

    private static FileInfo ResoudreCheminFichier(string chemin, DirectoryInfo projectDir, string nomChamp)
    {
        var cheminResolu = Path.IsPathRooted(chemin)
            ? chemin
            : Path.Combine(projectDir.FullName, chemin);

        var fichier = new FileInfo(cheminResolu);
        if (!fichier.Exists)
        {
            var cheminAffiche = Path.IsPathRooted(chemin) ? chemin : fichier.FullName;
            throw new FileNotFoundException($"Le fichier '{cheminAffiche}' spécifié dans '{nomChamp}' est introuvable.", fichier.FullName);
        }

        return fichier;
    }

    public static List<ContenuItem> ParseContenu(string toml, DirectoryInfo projectDir)
    {
        var projet = TomlSerializer.Deserialize<ProjetToml>(toml, Options)
            ?? throw new InvalidOperationException("Le fichier book.toml est invalide.");

        if (projet.Contenu is null || projet.Contenu.Count == 0)
            throw new InvalidOperationException("La section [[contenu]] est absente ou vide dans book.toml.");

        return projet.Contenu.Select((item, index) =>
        {
            if (string.IsNullOrWhiteSpace(item.Fichier))
                throw new InvalidOperationException($"L'entrée [[contenu]] #{index + 1} n'a pas de champ 'fichier'.");

            var fichier = ResoudreCheminFichier(item.Fichier, projectDir, $"contenu[{index}].fichier");
            return new ContenuItem { Fichier = fichier, Navigation = item.Navigation };
        }).ToList();
    }

    // Classes internes pour la désérialisation TOML
    private class ProjetToml
    {
        public Metadonnees? Metadonnees { get; set; }
        public EpubToml? Epub { get; set; }
        public List<ContenuToml>? Contenu { get; set; }
    }

    private class EpubToml
    {
        public string? Couverture { get; set; }
        public string? Css { get; set; }
        public bool TableDesMatieres { get; set; } = false;
    }

    private class ContenuToml
    {
        public string? Fichier { get; set; }
        public bool Navigation { get; set; } = false;
    }
}
