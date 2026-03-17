namespace epubst.Epub;

public static class CoverGenerator
{
    public static string Generer(string nomFichierImage) => $$"""
        <?xml version="1.0" encoding="utf-8"?>
        <!DOCTYPE html>
        <html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
        <head>
          <meta charset="utf-8"/>
          <title>Couverture</title>
          <style type="text/css">
            body { margin: 0; padding: 0; }
            img { max-width: 100%; height: 100vh; display: block; margin: 0 auto; }
          </style>
        </head>
        <body epub:type="cover">
          <img src="images/{{nomFichierImage}}" alt="Couverture"/>
        </body>
        </html>
        """;
}
