using epubst.Assets;

namespace epubst.Tests.Assets;

public class EmbeddedAssetsTests
{
    [Fact]
    public void DefaultCss_NonVide()
    {
        Assert.NotEmpty(EmbeddedAssets.DefaultCss);
    }

    [Fact]
    public void DefaultCss_ContientRegleBody()
    {
        Assert.Contains("body", EmbeddedAssets.DefaultCss);
    }

    [Fact]
    public void DefaultCss_ContientRegleHr()
    {
        Assert.Contains("hr", EmbeddedAssets.DefaultCss);
    }
}
