# epubst

Outil en ligne de commande .NET 10 qui compile un projet Markdown en fichier ePub3.

Générique par conception : le Markdown peut venir de n'importe quelle source (Obsidian, exporteur Word, éditeur de texte, etc.).

---

## Installation

Télécharger le binaire autonome depuis les [releases](../../releases) et le placer dans votre `PATH`.

Aucune dépendance requise — le runtime .NET n'est pas nécessaire.

---

## Utilisation

```bash
epubst compile book.toml
epubst compile book.toml -o monlivre.epub
epubst compile book.toml --debug-output   # génère les XHTML intermédiaires dans _tmp/
```

---

## Structure d'un projet

```
monlivre/
├── book.toml
├── corps.md
├── remerciements.md
├── mentions_legales.md
├── a_propos_auteur.md
├── assets/
│   └── cover.jpg
└── style.css          # optionnel
```

---

## book.toml

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
fichier = "remerciements.md"
titre = "Remerciements"            # optionnel — surcharge la balise <title> du XHTML

[[contenu]]
fichier = "corps.md"
navigation = true                  # alimente la table des matières ePub

[[contenu]]
fichier = "mentions_legales.md"

[[contenu]]
fichier = "a_propos_auteur.md"

[[fonte]]
nom = "Ma Fonte"
fichier = "assets/mafonte.otf"     # .otf, .ttf, .woff, .woff2

[[fonte]]
nom = "Ma Fonte Bold"
fichier = "assets/mafonte-bold.otf"
```

### Règles

- `navigation = false` par défaut — le fichier avec `navigation = true` alimente `nav.xhtml`
- Les fichiers `navigation = false` sont convertis en un seul XHTML
- Les fichiers `navigation = true` sont découpés en **un XHTML par H1**
- `[[fonte]]` est optionnel — si présent, génère automatiquement les règles `@font-face`

---

## Markdown supporté

| Syntaxe | Rendu |
|---|---|
| `# Titre` | Titre de chapitre — alimente `nav.xhtml` |
| `## Titre` | Séparateur de scène — rendu `* * *` via CSS |
| Paragraphe | `<p>` |
| `*italique*` | `<em>` |
| `**gras**` | `<strong>` |
| `![alt](chemin)` | `<img>` |
| `{.classe}` avant un bloc | Attribut `class` sur `<p>`, `<h1>`, `<hr>` |
| `::: classe` … `:::` | `<div class="classe">` |

---

## CSS

Un CSS par défaut est embarqué (typographie française, conforme ePub3, compatible toutes liseuses). Il peut être surchargé via le fichier `css` déclaré dans `[epub]`.

---

## Build

```bash
dotnet build
dotnet test
dotnet publish -c Release -r osx-arm64 --self-contained
```

---

## Stack

- .NET 10, C#
- [Markdig](https://github.com/xoofx/markdig) — parsing Markdown
- [Tomlyn](https://github.com/xoofx/Tomlyn) — parsing TOML
- [System.CommandLine](https://github.com/dotnet/command-line-api) — CLI
- xUnit — tests
