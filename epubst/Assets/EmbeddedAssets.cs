using System.Reflection;

namespace epubst.Assets;

public static class EmbeddedAssets
{
    public static string DefaultCss => LireRessource("epubst.Assets.default.css");

    private static string LireRessource(string nom)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(nom)
            ?? throw new InvalidOperationException($"Ressource embarquée introuvable : '{nom}'.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
