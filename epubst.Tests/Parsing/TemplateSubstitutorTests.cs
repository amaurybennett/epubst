using epubst.Models;
using epubst.Parsing;

namespace epubst.Tests.Parsing;

public class TemplateSubstitutorTests
{
    private static Metadonnees MetaComplete() => new()
    {
        Titre           = "Mon Roman",
        Auteurs         = ["Alice", "Bob"],
        Langue          = "fr",
        Isbn            = "978-2-07-054127-2",
        Editeur         = "Gallimard",
        DatePublication = "2024-01-15",
        SousTitre       = "Un sous-titre",
        Serie           = "La Trilogie",
        NumeroTome      = 2,
        BetaLecture     = "Claire Dupont",
        Correction      = "Marc Martin",
        Couverture      = "Studio Pixel",
        Copyright       = 2024
    };

    // ========== Substitutions présentes ==========

    [Fact]
    public void Substituer_Titre()
    {
        var result = TemplateSubstitutor.Substituer("Titre : %%meta.titre%%", MetaComplete());
        Assert.Equal("Titre : Mon Roman", result);
    }

    [Fact]
    public void Substituer_Auteurs_Jointure()
    {
        var result = TemplateSubstitutor.Substituer("Par %%meta.auteurs%%", MetaComplete());
        Assert.Equal("Par Alice, Bob", result);
    }

    [Fact]
    public void Substituer_Langue()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.langue%%", MetaComplete());
        Assert.Equal("fr", result);
    }

    [Fact]
    public void Substituer_Isbn()
    {
        var result = TemplateSubstitutor.Substituer("ISBN : %%meta.isbn%%", MetaComplete());
        Assert.Equal("ISBN : 978-2-07-054127-2", result);
    }

    [Fact]
    public void Substituer_Editeur()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.editeur%%", MetaComplete());
        Assert.Equal("Gallimard", result);
    }

    [Fact]
    public void Substituer_DatePublication()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.date_publication%%", MetaComplete());
        Assert.Equal("2024-01-15", result);
    }

    [Fact]
    public void Substituer_SousTitre()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.sous_titre%%", MetaComplete());
        Assert.Equal("Un sous-titre", result);
    }

    [Fact]
    public void Substituer_Serie()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.serie%%", MetaComplete());
        Assert.Equal("La Trilogie", result);
    }

    [Fact]
    public void Substituer_NumeroTome()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.numero_tome%%", MetaComplete());
        Assert.Equal("2", result);
    }

    [Fact]
    public void Substituer_PlusieursTags_DansUnMemeTexte()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.titre%% par %%meta.auteurs%%", MetaComplete());
        Assert.Equal("Mon Roman par Alice, Bob", result);
    }

    [Fact]
    public void Substituer_BetaLecture()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.beta_lecture%%", MetaComplete());
        Assert.Equal("Claire Dupont", result);
    }

    [Fact]
    public void Substituer_Correction()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.correction%%", MetaComplete());
        Assert.Equal("Marc Martin", result);
    }

    [Fact]
    public void Substituer_CouvertureCredit()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.couverture%%", MetaComplete());
        Assert.Equal("Studio Pixel", result);
    }

    [Fact]
    public void Substituer_Copyright()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.copyright%%", MetaComplete());
        Assert.Equal("2024", result);
    }

    // ========== Valeur absente — tag inchangé ==========

    [Fact]
    public void Substituer_IsbnAbsent_TagInchange()
    {
        var meta = MetaComplete() with { Isbn = null };
        var result = TemplateSubstitutor.Substituer("ISBN : %%meta.isbn%%", meta);
        Assert.Equal("ISBN : %%meta.isbn%%", result);
    }

    [Fact]
    public void Substituer_CleInconnue_TagInchange()
    {
        var result = TemplateSubstitutor.Substituer("%%meta.inconnu%%", MetaComplete());
        Assert.Equal("%%meta.inconnu%%", result);
    }

    // ========== Texte sans tag ==========

    [Fact]
    public void Substituer_SansTag_TexteInchange()
    {
        var result = TemplateSubstitutor.Substituer("Aucun tag ici.", MetaComplete());
        Assert.Equal("Aucun tag ici.", result);
    }
}
