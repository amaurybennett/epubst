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
                Console.WriteLine($"Couverture : {projet.EpubOptions.Couverture.FullName}");

                // TODO: implémenter la compilation ePub
                Console.WriteLine("Compilation non encore implémentée.");
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
