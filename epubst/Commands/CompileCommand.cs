using System.CommandLine;
using epubst.Parsing;

namespace epubst.Commands;

public static class CompileCommand
{
    public static readonly Argument<FileInfo> FichierArgument = new("fichier")
    {
        Description = "Le fichier book.toml du projet à compiler"
    };

    public static readonly Option<FileInfo?> OutputOption = new("-o", ["--output"])
    {
        Description = "Le fichier epub de sortie (optionnel)"
    };

    public static Command Build()
    {
        var command = new Command("compile", "Compile un projet en ePub");
        command.Add(FichierArgument.AcceptExistingOnly());
        command.Add(OutputOption);

        command.SetAction((ParseResult parseResult) =>
        {
            var fichier = parseResult.GetValue(FichierArgument)!;
            var output = parseResult.GetValue(OutputOption);

            try
            {
                var toml = File.ReadAllText(fichier.FullName);
                var projectDir = fichier.Directory!;

                var projet = ProjectParser.Parse(toml, projectDir);

                Console.WriteLine($"Projet : {projet.Metadonnees.Titre}");
                Console.WriteLine($"Auteur(s) : {string.Join(", ", projet.Metadonnees.Auteurs)}");

                foreach (var item in projet.Contenu)
                {
                    var markdown = File.ReadAllText(item.Fichier.FullName);
                    var nomBase = Path.GetFileNameWithoutExtension(item.Fichier.Name);
                    var result = MarkdownConverter.Convert(markdown, item.Navigation, nomBase);
                    Console.WriteLine($"  {item.Fichier.Name} → {result.Documents.Count} document(s), {result.Chapitres.Count} chapitre(s)");
                }

                // TODO: implémenter EpubBuilder
                Console.WriteLine("Assemblage ePub non encore implémenté.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur : {ex.Message}");
                return 1;
            }
        });

        return command;
    }
}
