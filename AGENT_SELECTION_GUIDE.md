# 🎯 Système de Sélection d'Agents

## ✨ Nouvelle Fonctionnalité

Vous pouvez maintenant **cliquer sur un agent individuellement** pour voir ses informations détaillées.

## 🖱️ Comment utiliser

### 1. Cliquer sur un agent
- **Cliquez/Tapez** directement sur un agent dans la simulation
- L'agent sélectionné sera **entouré d'un cercle jaune**
- Un panneau d'information apparaît à droite

### 2. Informations affichées

Le panneau affiche :
- 📍 **Position** : Coordonnées (X, Y) dans le monde virtuel
- 🏛️ **Orientation politique** : Gauche 🔴 ou Droite 🔵
- 😊 **État émotionnel** : Content 😊 ou Pas content 😠
- ⚡ **Vitesse** : Vitesse de déplacement en px/frame
- 🎨 **Groupe** : A ou B

### 3. Désélectionner
- Cliquez sur le bouton **✕** dans le panneau
- Ou cliquez dans le vide (zone sans agent)

## 🎨 Mise en évidence visuelle

- **Cercle jaune épais** autour de l'agent sélectionné
- **Panneau doré** (#FFF9E6) avec bordure jaune (#FFD700)
- Visible même pendant le déplacement de l'agent

## 🔧 Détails techniques

### Détection du clic
- **Rayon de détection** : 15 pixels (monde virtuel)
- **Algorithme** : Recherche de l'agent le plus proche
- **Conversion de coordonnées** : Écran → Monde virtuel (avec prise en compte du zoom)

### Performance
- ✅ Détection optimisée (calcul de distance euclidienne)
- ✅ Rendu temps réel avec mise en évidence
- ✅ Pas d'impact sur le FPS

## 📊 Exemple d'affichage

```
🎯 Agent Sélectionné

📍 Position: (452, 678)
🏛️ Orientation: Gauche 🔴
😊 État: Pas content 😠
⚡ Vitesse: 3.0 px/frame
🎨 Groupe: A
```

## 💡 Cas d'usage

1. **Analyse individuelle** : Comprendre le comportement d'un agent spécifique
2. **Debugging** : Vérifier l'orientation politique et l'état
3. **Pédagogie** : Expliquer le système à des utilisateurs
4. **Validation** : Confirmer que les agents réagissent correctement aux phrases

## 🎮 Intégration avec les autres fonctionnalités

- ✅ Compatible avec le **zoom** (détection adaptée)
- ✅ Compatible avec **pause/reprise**
- ✅ L'agent reste sélectionné même s'il se déplace
- ✅ Les informations se mettent à jour si l'agent change d'état

## 🐛 Notes

- Si plusieurs agents sont très proches, le **plus proche du clic** est sélectionné
- Le panneau d'information **s'affiche uniquement** quand un agent est sélectionné
- Cliquer dans le vide **désélectionne automatiquement**

---

**Astuce :** Utilisez cette fonctionnalité pour vérifier qu'environ 50% des agents sont de gauche et 50% de droite après l'initialisation !
