# Distribution via Homebrew — Guide de référence

## Vue d'ensemble

L'objectif est de permettre l'installation via :
```bash
brew tap <owner>/mon-outil
brew install mon-outil
```

Cela nécessite :
1. Un repo GitHub **tap** (`homebrew-mon-outil`)
2. Une formule Ruby (`Formula/mon-outil.rb`)
3. Un job GitHub Actions qui met à jour la formule à chaque release

---

## Étape 1 — Créer le repo tap

Sur GitHub, créer un repo public nommé **`homebrew-mon-outil`** (le préfixe `homebrew-` est obligatoire pour Homebrew).

Dans ce repo, créer le fichier **`Formula/mon-outil.rb`** avec ce contenu placeholder :
```ruby
class MonOutil < Formula
end
```
L'action va l'écraser à la première release.

> Ne pas oublier d'ajouter une **description** au repo principal sur GitHub (page du repo → icône engrenage → Description). Sinon le job plante avec `TypeError: 'NoneType' object is not subscriptable`.

---

## Étape 2 — Créer un PAT

Sur GitHub → **Settings → Developer settings → Personal access tokens → Tokens (classic)** → Generate new token :

- **Note** : `homebrew-tap-mon-outil`
- **Expiration** : selon préférence
- **Scope** : cocher uniquement **`repo`**

---

## Étape 3 — Ajouter le secret dans le repo principal

Dans le repo de l'outil (pas le tap) :
**Settings → Secrets and variables → Actions → New repository secret**

- **Name** : `HOMEBREW_TAP_TOKEN`
- **Secret** : le PAT généré à l'étape 2

---

## Étape 4 — Job GitHub Actions

À ajouter dans `release.yml`, après le job `release`. Le job suppose qu'un artifact nommé `mon-outil-osx-arm64.tar.gz` a été uploadé par le job `build`.

```yaml
homebrew:
  name: Update Homebrew Tap
  needs: release
  runs-on: ubuntu-latest
  steps:
    - uses: actions/download-artifact@v4
      with:
        name: mon-outil-osx-arm64.tar.gz

    - name: Compute SHA256
      id: sha256
      run: echo "value=$(sha256sum mon-outil-osx-arm64.tar.gz | awk '{print $1}')" >> "$GITHUB_OUTPUT"

    - name: Clone tap
      run: |
        git clone https://x-access-token:${{ secrets.HOMEBREW_TAP_TOKEN }}@github.com/<owner>/homebrew-mon-outil.git tap

    - name: Generate formula
      env:
        TAG: ${{ github.ref_name }}
        VERSION: ${{ github.ref_name }}
        SHA256: ${{ steps.sha256.outputs.value }}
      run: |
        python3 - <<'PY'
        import os, textwrap
        tag     = os.environ["TAG"]
        version = os.environ["VERSION"].lstrip("v")
        sha256  = os.environ["SHA256"]
        formula = textwrap.dedent(f"""\
          # typed: true
          # frozen_string_literal: true

          class MonOutil < Formula
            desc "Description courte de l'outil"
            homepage "https://github.com/<owner>/mon-outil"
            license "MIT"
            version "{version}"

            on_macos do
              if Hardware::CPU.arm?
                url "https://github.com/<owner>/mon-outil/releases/download/{tag}/mon-outil-osx-arm64.tar.gz"
                sha256 "{sha256}"
              end
            end

            def install
              bin.install "mon-outil"
            end

            test do
              system "#{{bin}}/mon-outil", "--help"
            end
          end
        """)
        with open("tap/Formula/mon-outil.rb", "w") as f:
            f.write(formula)
        PY

    - name: Commit and push
      env:
        TAG: ${{ github.ref_name }}
      run: |
        cd tap
        git config user.email "github-actions@github.com"
        git config user.name "GitHub Actions"
        git add Formula/mon-outil.rb
        git commit -m "chore: bump mon-outil to ${TAG}"
        git push
```

---

## Points d'attention

| Problème | Cause | Fix |
|---|---|---|
| `You must provide all necessary environment variables` | Secret `HOMEBREW_TAP_TOKEN` absent ou dans le mauvais repo | Le mettre dans le repo **principal**, pas dans le tap |
| `TypeError: 'NoneType' object is not subscriptable` | Repo GitHub sans description | Ajouter une description sur la page GitHub du repo |
| `[Errno 2] No such file or directory: '.../mon-outil.rb'` | Fichier formula absent du tap | Créer `Formula/mon-outil.rb` avec le placeholder `class MonOutil < Formula\nend` |
| Version parsée depuis le nom de fichier (ex: `64` depuis `arm64`) | Pas de `version` explicite dans la formule | Toujours déclarer `version "{version}"` dans la formule |
| 404 en téléchargeant l'asset depuis la release (dans le job CI) | Race condition entre upload et job homebrew | Utiliser `actions/download-artifact` plutôt que `curl` sur l'URL de release |
| 404 lors de `brew install` (curl sans auth) | Repo GitHub **privé** — les assets de release ne sont pas accessibles publiquement | Passer le repo en **public** ; Homebrew ne peut pas s'authentifier auprès de GitHub |

---

## Pièges à éviter

- **Ne pas utiliser `homebrew-releaser`** (Justintime50) : ne détecte pas les assets binaires dont le nom contient `osx` au lieu de `darwin`. Le script Python custom est plus fiable.
- Le secret doit être dans le repo **source** (ex: `epubst`), pas dans le repo tap.
- Homebrew infère la version depuis l'URL/nom de fichier si elle n'est pas déclarée explicitement — toujours la déclarer.
- **Le repo source doit être public** — les assets de release GitHub ne sont pas accessibles sans authentification sur un repo privé, et Homebrew ne peut pas s'authentifier.
