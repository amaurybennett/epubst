using System.Text;
using epubst.Models;

namespace epubst.Epub;

public static class FontesCssGenerator
{
    public static string Generer(IReadOnlyList<FonteItem> fontes)
    {
        var sb = new StringBuilder();
        foreach (var fonte in fontes)
        {
            sb.AppendLine("@font-face {");
            sb.AppendLine($"  font-family: \"{fonte.Nom}\";");
            sb.AppendLine($"  src: url(\"../fonts/{fonte.Fichier.Name}\");");
            sb.AppendLine("}");
        }
        return sb.ToString();
    }
}
