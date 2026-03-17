# Instructions pour Claude

## Communication
- Répondre en français
- Réponses courtes et directes

## Approche de développement
- Une fonctionnalité à la fois
- Tests unitaires à chaque nouvelle fonctionnalité, avant de passer à la suivante
- Ne pas coder plusieurs fonctionnalités d'un coup

## Stack technique
- .NET 10, C#
- `System.CommandLine` — CLI
- `Tomlyn` — parsing TOML
- `Markdig` — parsing Markdown (CommonMark)
- `System.IO.Compression` — génération du ZIP ePub
- xUnit — tests unitaires

## Structure de la solution
- Projet principal : `epubst`
- Projet de tests : `epubst.Tests`

## Commits
- **Ne jamais commiter sans demande explicite de l'utilisateur** — c'est lui qui gère la granularité
- Utiliser la norme Conventional Commits : `<type>(<scope>): <description>`
- Types courants : `feat`, `fix`, `chore`, `test`, `refactor`
- Exemples : `feat(parsing): section [[contenu]] du book.toml`, `fix(cli): message d'erreur fichier introuvable`
- Préférer plusieurs petits commits explicites plutôt qu'un seul gros commit

## Specs du projet
Voir `SPECS.md` pour les spécifications fonctionnelles complètes.
