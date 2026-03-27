# Audit de sécurité — epubst

## Prompt d'audit

Tu es un expert en sécurité .NET. Effectue un audit de sécurité complet du projet epubst.

### Stack à auditer

- .NET 10 / C# — projet `epubst/` et tests `epubst.Tests/`
- `Tomlyn` — désérialisation TOML (source de données non fiable : `book.toml`)
- `Markdig` — parsing Markdown (source de données non fiable : fichiers `.md`)
- `System.IO.Compression` — génération ZIP (format ePub)
- GitHub Actions — workflows dans `.github/workflows/`

### Périmètre

Lis tous les fichiers `.cs`, `.csproj`, `.yml` et `.yaml` du projet.

### Points de contrôle obligatoires

#### 1. Path traversal / containment
- Tout chemin issu de `book.toml` (couverture, CSS, contenu, fontes) doit rester dans `projectDir` (= dossier parent du `book.toml`)
- Tout chemin d'image issu du Markdown doit rester dans `projectDir`
- Vérification : `Path.GetFullPath(chemin).StartsWith(Path.GetFullPath(projectDir) + Path.DirectorySeparatorChar)`
- Le nom du fichier ePub de sortie (dérivé du titre) doit être sanitisé avec `Path.GetInvalidFileNameChars()`
- Les noms de fichiers écrits dans `_tmp/` (mode debug) doivent être incapables de contenir `..`

#### 2. Injection dans le XHTML généré
- Toutes les valeurs insérées dans du XHTML doivent passer par `EscapeXml()` (`&`, `<`, `>`, `"`)
- Les noms de classes CSS issus des generic attributes Markdig doivent être sanitisés (uniquement `[a-zA-Z0-9_-]`)
- Les classes CSS du `<body>` doivent être sanitisées
- Aucun `innerHTML`-équivalent ne doit accepter du contenu brut utilisateur

#### 3. Injection dans le CSS généré
- Les noms de fontes insérés dans `font-family: "..."` doivent avoir `\` et `"` échappés
- Les URLs CSS (`src: url(...)`) doivent utiliser uniquement `FileInfo.Name` (pas le chemin complet)

#### 4. ZIP Slip
- Les chemins d'entrée dans l'archive ZIP doivent être des chemins relatifs sans `..`
- Les noms de fichiers utilisés dans le ZIP doivent venir de `FileInfo.Name` (pas d'un chemin brut)
- Vérifier chaque appel à `CreateEntry()` et `CreateEntryFromFile()`

#### 5. Désérialisation TOML
- Vérifier que `Tomlyn` n'exécute pas de code arbitraire à la désérialisation
- Vérifier que les champs désérialisés sont validés avant utilisation (null checks, whitespace checks)
- Aucun champ TOML ne doit être utilisé directement dans un chemin ou une commande sans validation

#### 6. Dépendances
- Exécuter `dotnet list package --vulnerable` à la racine du projet et inclure la sortie dans le rapport
- Signaler tout package avec une CVE connue ou une version obsolète
- Signaler l'usage de versions `preview` ou `beta` en production

#### 7. Secrets et credentials
- Aucune clé, token ou mot de passe en dur dans le code source
- Dans les workflows GitHub Actions : les secrets sont-ils passés via `${{ secrets.X }}` et jamais écrits dans les logs ?
- Les tokens ne doivent pas apparaître dans les URLs git (`https://token@github.com/...`)
- Préférer le credential store git ou `gh` CLI pour les opérations authentifiées

#### 8. Exécution de commandes
- Vérifier l'absence de `Process.Start()`, `cmd.exe`, `bash -c`, ou équivalents
- Si présent : les arguments doivent être passés comme tableau (jamais interpolés dans une chaîne)

#### 9. Écriture de fichiers
- Tout `File.WriteAllText()` ou `File.Create()` dont le chemin est partiellement contrôlé par l'utilisateur doit être validé
- Vérifier que les fichiers de sortie ne peuvent pas écraser des fichiers système

### Format de sortie attendu

Pour chaque vulnérabilité trouvée :

```
#### [SÉVÉRITÉ] Titre court

**Fichier :** `chemin/fichier.cs:ligne`
**Problème :** description du vecteur d'attaque avec exemple concret
**Correctif :** code C# ou YAML de correction
```

Sévérités : `CRITIQUE`, `HAUTE`, `MOYENNE`, `FAIBLE`, `INFO`

Terminer par :

1. Un tableau récapitulatif (sévérité / fichier / ligne)
2. Une section "Points positifs" listant ce qui est déjà bien fait
3. Une section "Recommandations" pour les améliorations non bloquantes

### Historique des vulnérabilités corrigées

_(À titre de référence — ne pas re-signaler sauf régression)_

| Commit | Sévérité | Description |
|---|---|---|
| `ac15e77` | HAUTE | Path traversal dans `ResoudreCheminFichier` et `ResoudreImage` |
| `ac15e77` | HAUTE | ZIP Slip — titre du livre utilisé comme nom de fichier sans sanitisation |
| `e2b5f9e` | MOYENNE | XSS — classes CSS des generic attributes non sanitisées |
| `841ad8c` | MOYENNE | Injection CSS — `font-family` non échappé dans `FontesCssGenerator` |
| `b1874fb` | MOYENNE | Token GitHub exposé dans les URLs `git clone` du workflow |
