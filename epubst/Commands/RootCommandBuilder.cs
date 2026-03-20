using System.CommandLine;

namespace epubst.Commands;

public static class RootCommandBuilder
{
    public static RootCommand Build()
    {
        var root = new RootCommand("epubst — compilateur de projets Markdown en ePub")
        {
            CompileCommand.Build()
        };
        return root;
    }
}
