# 🐛 Guide de Dépannage - Résultats Inattendus

## ⚠️ Problèmes Courants

### 1. **"Tous les agents sont pas contents (rouges)"**

**Causes possibles :**
- ✅ Phrase **trop neutre** (aucun mot-clé détecté)
- ✅ Phrase **ambiguë** avec mots-clés gauche ET droite en quantité égale

**Solution (appliquée dans la nouvelle version) :**
- Si |score| < 0.15 → **Tous restent contents** (phrase neutre)
- Consultez les logs pour voir les mots-clés détectés

**Exemples de phrases neutres :**
```
❌ AVANT : "Il faut améliorer l'éducation" → Tous rouges
✅ APRÈS : "Il faut améliorer l'éducation" → Tous verts (neutre)
```

---

### 2. **"Les agents de droite sont pas contents alors que la phrase est de droite"**

**Cause :** Détection hors contexte d'un mot-clé de gauche

**Exemple problématique :**
```
Phrase : "Il faut réguler l'immigration"
❌ AVANT : Détecte "immigration" (gauche) → Score gauche → Droite pas content
✅ APRÈS : Détecte "réguler l'immigration" (expression droite) → Droite content
```

**Solution :** Utiliser des **expressions** au lieu de mots isolés

---

### 3. **"La phrase contient des mots de gauche ET de droite"**

**Comportement :** Le système calcule un **score net**

**Exemple :**
```
Phrase : "Il faut taxer les riches pour libérer l'entreprise"
- Mots gauche : "taxer les riches" (+2.0)
- Mots droite : "libérer", "entreprise" (+2.0)
- Score net : 0 → Phrase neutre → Tous verts
```

**Conseil :** Pour une phrase claire, évitez de mélanger les orientations

---

### 4. **"Les mots-clés ne correspondent pas à ma définition gauche/droite"**

**Note importante :** Le système utilise des **stéréotypes politiques simplifiés**

**Personnalisation :**
1. Ouvrez `VoxPopuli.Client\Services\PoliticalPhraseAnalyzer.cs`
2. Modifiez les tableaux `LeftStrongPhrases`, `RightStrongPhrases`, etc.
3. Ajoutez vos propres expressions/mots-clés

**Exemple d'ajout :**
```csharp
private static readonly string[] LeftStrongPhrases = new[]
{
    // ... expressions existantes ...
    "votre nouvelle expression ici",
};
```

---

## 🔍 Activer les Logs de Débogage

Les logs détaillés vous montrent **exactement** ce qui est détecté.

### Comment consulter les logs :

**Dans Visual Studio :**
1. Lancez l'application en mode **Debug** (F5)
2. Ouvrez la fenêtre **Sortie** (Output)
3. Saisissez une phrase et analysez-la
4. Consultez les logs :

```
📢 Phrase politique analysée: 'il faut réguler l'immigration'
   [DROITE] Expression forte détectée: 'réguler l'immigration' (+2.0)
   → Score brut: Gauche=0, Droite=2.0
   → Score final normalisé: +1.00
   Score: +1.00 (Droite)
   Résultat: 252 contents (verts), 248 pas contents (rouges)
```

### Interpréter les logs :

- **Expression forte** : Poids +2.0 (bi-gramme)
- **Mot-clé simple** : Poids +1.0 (mot isolé)
- **Score brut** : Somme des poids gauche vs droite
- **Score normalisé** : Entre -1 (gauche) et +1 (droite)

---

## 🎯 Cas Spéciaux

### Phrase avec négation

⚠️ **Le système ne gère PAS les négations**

```
Phrase : "Il NE faut PAS taxer les riches"
❌ Détecte : "taxer les riches" → Score gauche (FAUX)
```

**Solution temporaire :** Évitez les négations, ou ajoutez l'expression inverse :
```csharp
RightStrongPhrases = { "ne pas taxer", "pas de taxes", ... }
```

### Phrase ironique/sarcastique

⚠️ **Le système ne comprend PAS l'ironie**

```
Phrase : "Bien sûr, taxer à 90% c'est une excellente idée !" (ironique)
❌ Détecte : "taxer" → Score gauche (littéral)
```

**Solution :** Le système analyse uniquement le **sens littéral**

---

## 📊 Seuil de Neutralité

**Valeur actuelle :** `NeutralThreshold = 0.15` (±15%)

### Modifier le seuil :

Dans `PoliticalPhraseAnalyzer.cs` :
```csharp
private const float NeutralThreshold = 0.15f; // Changez cette valeur
```

**Effets :**
- **Augmenter** (ex: 0.3) → Plus de phrases considérées neutres
- **Diminuer** (ex: 0.05) → Moins de phrases neutres, réactions plus sensibles

---

## ✅ Checklist de Validation

Avant de signaler un bug, vérifiez :

- [ ] Les logs de débogage sont activés (mode Debug)
- [ ] La phrase contient des mots-clés reconnus (voir listes complètes)
- [ ] Le score final est affiché dans les logs
- [ ] Le seuil de neutralité est approprié
- [ ] La phrase n'utilise pas de négations/ironie

---

## 🆘 Support

**Si le problème persiste :**

1. **Copiez les logs complets** de la console
2. **Notez la phrase testée**
3. **Décrivez le résultat attendu vs obtenu**
4. **Consultez** `TEST_PHRASES_VALIDATION.md` pour les cas de test validés

**Fichiers clés :**
- `PoliticalPhraseAnalyzer.cs` : Logique de détection
- `TEST_PHRASES_VALIDATION.md` : Cas de test validés
- `IMPLEMENTATION_GUIDE.md` : Architecture du système

---

**Bon debug ! 🔧**
