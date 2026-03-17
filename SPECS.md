# Spécifications — epubst

## Vue d'ensemble

`epubst` est un outil en ligne de commande .NET 10 cross-plateforme qui compile un projet Markdown en fichier ePub3.

Il est générique : il ne dépend pas d'un outil de génération de Markdown particulier. Le Markdown peut venir de n'importe quelle source (exporteur Word, Obsidian, éditeur de texte, etc.).

---

## CLI

```bash
epubst compile book.toml
epubst compile book.toml -o monlivre.epub
```

---

## Structure d'un projet

```
monlivre/
├── book.toml
├── corps.md                  # corps principal (navigation = true)
├── remerciements.md
├── mentions_legales.md
├── a_propos_auteur.md
├── assets/
│   └── cover.jpg
└── style.css                 # optionnel
```

---

## Format de `book.toml`

```toml
[metadonnees]
titre = "À l'école des sorciers"
sous_titre = "Harry Potter"        # optionnel
serie = "Harry Potter"             # optionnel
numero_tome = 1                    # optionnel
auteurs = ["J.K. Rowling"]
langue = "fr"
editeur = "Gallimard"              # optionnel
isbn = "978-2-07-054127-2"         # optionnel
date_publication = "1998-10-09"    # optionnel, format ISO

[epub]
couverture = "assets/cover.jpg"
css = "style.css"                  # optionnel
table_des_matieres = false         # défaut : false

[[contenu]]
fichier = "remerciements.md"       # navigation = false implicite

[[contenu]]
fichier = "corps.md"
navigation = true

[[contenu]]
fichier = "mentions_legales.md"    # navigation = false implicite

[[contenu]]
fichier = "a_propos_auteur.md"     # navigation = false implicite
```

### Règles

- `navigation` vaut `false` par défaut — seul le fichier principal le porte à `true`
- `table_des_matieres` vaut `false` par défaut
- Seuls `true` et `false` sont acceptés pour les booléens (pas de `oui`/`non`)
- Les champs optionnels peuvent être omis sans erreur

---

## Markdown supporté (v1)

| Syntaxe           | Rendu                                       |
|-------------------|---------------------------------------------|
| `# Titre`         | Titre de chapitre — alimente `nav.xhtml`    |
| `## Titre`        | Séparateur de scène — rendu `* * *` via CSS |
| Paragraphe normal | `<p>`                                       |
| `*italique*`      | `<em>`                                      |
| `**gras**`        | `<strong>`                                  |

Hors scope v1 : tableaux, listes, images inline, notes de bas de page, code.

---

## Génération ePub3

### Structure du ZIP

```
mimetype                          (non compressé, en premier)
META-INF/
  container.xml
OEBPS/
  content.opf
  nav.xhtml
  cover.xhtml                     (générée automatiquement)
  styles/
    default.css                   (embarqué dans epubst)
    style.css                     (copié depuis le projet, si présent)
  images/
    cover.jpg                     (copié depuis le projet)
  text/
    001_remerciements.xhtml
    002_corps_ch001.xhtml         (un fichier par H1)
    002_corps_ch002.xhtml
    ...
    003_mentions_legales.xhtml
    004_a_propos_auteur.xhtml
```

### Règles de découpage

- Fichier avec `navigation = true` : découpé en **un fichier XHTML par H1**
- Fichier avec `navigation = false` : converti en **un seul fichier XHTML**

### `nav.xhtml`

- Généré automatiquement depuis les H1 des fichiers `navigation = true`
- Non inclus dans le `spine` (invisible dans le flux de lecture)
- Obligatoire pour la conformité ePub3

### Page de couverture

- Générée automatiquement depuis l'image définie dans `[epub] couverture`
- Incluse en première position dans le `spine`

### Page table des matières

- Générée uniquement si `table_des_matieres = true`
- Non implémentée en v1 (option réservée pour une version future)

---

## CSS

### CSS par défaut (embarqué)

Minimaliste, conforme ePub3, compatible toutes liseuses :

- Police système (pas de font embarquée)
- Taille de texte relative (`em`)
- Indentation de première ligne (norme typographique française)
- `text-align: justify`
- Titres de chapitres centrés et aérés
- Séparateur de scène (`<hr>`) rendu `* * *` centré avec espace autour
- Pas de margin/padding surprenants

### Surcharge

Si un fichier `style.css` est déclaré dans `[epub]`, il est embarqué dans l'ePub et chargé **après** le CSS par défaut, permettant de surcharger n'importe quelle règle.

---

## Architecture du code

**Librairies :**
- `System.CommandLine` — CLI
- `Tomlyn` — parsing TOML
- `Markdig` — parsing Markdown (CommonMark)
- `System.IO.Compression` — génération du ZIP

**Structure du projet :**
```
epubst/
├── Program.cs
├── Commands/
│   └── CompileCommand.cs
├── Models/
│   ├── BookProject.cs
│   ├── Metadata.cs
│   └── SpineItem.cs
├── Parsing/
│   ├── ProjectParser.cs        lit et valide book.toml → BookProject
│   └── MarkdownConverter.cs   .md → XHTML + extraction des H1
├── Epub/
│   ├── EpubBuilder.cs          orchestre la construction
│   ├── OpfGenerator.cs         génère content.opf
│   ├── NavGenerator.cs         génère nav.xhtml
│   └── CoverGenerator.cs       génère cover.xhtml
└── Assets/
    └── default.css             embarqué comme ressource
```

**Pipeline de compilation :**
```
CompileCommand
  → ProjectParser         lit book.toml → BookProject
  → MarkdownConverter     chaque .md → XHTML + extrait les H1
  → EpubBuilder
      → CoverGenerator    cover.xhtml
      → NavGenerator      nav.xhtml (depuis les H1 collectés)
      → OpfGenerator      content.opf
      → ZipWriter         assemble le tout → .epub
```
