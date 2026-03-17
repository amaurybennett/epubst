using System.CommandLine;
using epubst.Epub;
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

    public static readonly Option<bool> DebugOutputOption = new("--debug-output")
    {
        Description = "Génère les fichiers XHTML intermédiaires dans un dossier _tmp/ à côté de book.toml"
    };

    public static Command Build()
    {
        var command = new Command("compile", "Compile un projet en ePub");
        command.Add(FichierArgument.AcceptExistingOnly());
        command.Add(OutputOption);
        command.Add(DebugOutputOption);

        command.SetAction((ParseResult parseResult) =>
        {
            var fichier = parseResult.GetValue(FichierArgument)!;
            var output = parseResult.GetValue(OutputOption);
            var debugOutput = parseResult.GetValue(DebugOutputOption);

            try
            {
                var toml = File.ReadAllText(fichier.FullName);
                var projectDir = fichier.Directory!;

                var projet = ProjectParser.Parse(toml, projectDir);

                Console.WriteLine($"Projet : {projet.Metadonnees.Titre}");
                Console.WriteLine($"Auteur(s) : {string.Join(", ", projet.Metadonnees.Auteurs)}");

                DirectoryInfo? tmpDir = null;
                if (debugOutput)
                {
                    tmpDir = new DirectoryInfo(Path.Combine(projectDir.FullName, "_tmp"));
                    tmpDir.Create();
                    if (!tmpDir.Exists)
                        throw new InvalidOperationException($"Impossible de créer le répertoire '{tmpDir.FullName}'.");
                }

                if (tmpDir is not null)
                {
                    foreach (var item in projet.Contenu)
                    {
                        var markdown = File.ReadAllText(item.Fichier.FullName);
                        var nomBase = Path.GetFileNameWithoutExtension(item.Fichier.Name);
                        var nomCss = projet.EpubOptions.Css?.Name;
                        var result = MarkdownConverter.Convert(markdown, item.Navigation, nomBase, projectDir, nomCss);
                        foreach (var doc in result.Documents)
                            File.WriteAllText(Path.Combine(tmpDir.FullName, doc.NomFichier), doc.Contenu);
                    }
                }

                var fichierSortie = output ?? new FileInfo(
                    Path.Combine(projectDir.FullName, $"{projet.Metadonnees.Titre}.epub"));

                using var stream = fichierSortie.Open(FileMode.Create, FileAccess.Write);
                EpubBuilder.Compiler(projet, projectDir, stream);

                Console.WriteLine($"ePub généré : {fichierSortie.FullName}");
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
