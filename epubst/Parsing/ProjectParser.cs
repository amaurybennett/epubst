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

    // Classes internes pour la désérialisation TOML
    private class ProjetToml
    {
        public Metadonnees? Metadonnees { get; set; }
        public EpubToml? Epub { get; set; }
    }

    private class EpubToml
    {
        public string? Couverture { get; set; }
        public string? Css { get; set; }
        public bool TableDesMatieres { get; set; } = false;
    }
}
