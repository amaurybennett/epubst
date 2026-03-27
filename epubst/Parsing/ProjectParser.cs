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

    public static BookProject Parse(string toml, DirectoryInfo projectDir)
    {
        var projet = DeserialiserToml(toml);
        return new BookProject
        {
            Metadonnees = ExtraireMetadonnees(projet),
            EpubOptions = ExtraireEpubOptions(projet, projectDir),
            Contenu = ExtraireContenu(projet, projectDir),
            Fontes = ExtrairesFontes(projet, projectDir)
        };
    }

    private static ProjetToml DeserialiserToml(string toml) =>
        TomlSerializer.Deserialize<ProjetToml>(toml, Options)
            ?? throw new InvalidOperationException("Le fichier book.toml est invalide.");

    private static Metadonnees ExtraireMetadonnees(ProjetToml projet) =>
        projet.Metadonnees ?? throw new InvalidOperationException("La section [metadonnees] est absente du fichier book.toml.");

    private static EpubOptions ExtraireEpubOptions(ProjetToml projet, DirectoryInfo projectDir)
    {
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

    private static List<ContenuItem> ExtraireContenu(ProjetToml projet, DirectoryInfo projectDir)
    {
        if (projet.Contenu is null || projet.Contenu.Count == 0)
            throw new InvalidOperationException("La section [[contenu]] est absente ou vide dans book.toml.");

        return projet.Contenu.Select((item, index) =>
        {
            if (string.IsNullOrWhiteSpace(item.Fichier))
                throw new InvalidOperationException($"L'entrée [[contenu]] #{index + 1} n'a pas de champ 'fichier'.");

            var fichier = ResoudreCheminFichier(item.Fichier, projectDir, $"contenu[{index}].fichier");
            return new ContenuItem { Fichier = fichier, Navigation = item.Navigation, Titre = item.Titre };
        }).ToList();
    }

    private static List<FonteItem> ExtrairesFontes(ProjetToml projet, DirectoryInfo projectDir)
    {
        if (projet.Fonte is null || projet.Fonte.Count == 0)
            return [];

        return projet.Fonte.Select((item, index) =>
        {
            if (string.IsNullOrWhiteSpace(item.Nom))
                throw new InvalidOperationException($"L'entrée [[fonte]] #{index + 1} n'a pas de champ 'nom'.");
            if (string.IsNullOrWhiteSpace(item.Fichier))
                throw new InvalidOperationException($"L'entrée [[fonte]] #{index + 1} n'a pas de champ 'fichier'.");

            var fichier = ResoudreCheminFichier(item.Fichier, projectDir, $"fonte[{index}].fichier");
            return new FonteItem { Nom = item.Nom, Fichier = fichier };
        }).ToList();
    }

    private static FileInfo ResoudreCheminFichier(string chemin, DirectoryInfo projectDir, string nomChamp)
    {
        var cheminResolu = Path.IsPathRooted(chemin)
            ? chemin
            : Path.Combine(projectDir.FullName, chemin);

        var cheminAbsolu = Path.GetFullPath(cheminResolu);
        var baseAbsolue  = Path.GetFullPath(projectDir.FullName) + Path.DirectorySeparatorChar;
        if (!cheminAbsolu.StartsWith(baseAbsolue, StringComparison.Ordinal))
            throw new InvalidOperationException($"Le chemin '{chemin}' spécifié dans '{nomChamp}' pointe en dehors du répertoire du projet.");

        var fichier = new FileInfo(cheminAbsolu);
        if (!fichier.Exists)
            throw new FileNotFoundException($"Le fichier '{cheminAbsolu}' spécifié dans '{nomChamp}' est introuvable.", cheminAbsolu);

        return fichier;
    }

    private class ProjetToml
    {
        public Metadonnees? Metadonnees { get; set; }
        public EpubToml? Epub { get; set; }
        public List<ContenuToml>? Contenu { get; set; }
        public List<FonteToml>? Fonte { get; set; }
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
        public string? Titre { get; set; }
    }

    private class FonteToml
    {
        public string? Nom { get; set; }
        public string? Fichier { get; set; }
    }
}
