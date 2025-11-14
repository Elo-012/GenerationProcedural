# GENERATION PROCEDURAL

---

## ✨ DIFFERENTS SYSTEMS DE GENERATION PROCEDURAL


- Simple Room Placement 
- BSP
- Cellular Automata
- Noise

---

## 🎮 Aperçu

| Scène                | GIF                        |
|----------------------|----------------------------|
| SIMPLE ROOM PLACEMENT : Choisi une Zone aléatoire et place une nouvelle room en vérifiant que ça ne chevauche pas une autre  | ![Terrain](docs/SRP.gif)  |
| BSP : On trace un carré qu'on divise par deux en deux node. On réitère plusieurs fois. puis chaque on crée une room dans chaque node enfant. Ensuite on relie tout les nodes par paire. | ![Parallax](docs/BSP.gif) |
| CELLULAR AUTOMATA : Chaque tuile, vérifie ses voisins. Et si il y'en a Plus de X, elle se transforme en les autres. | ![Chunks](docs/CellAUTO.gif) |
| NOISE |

*(Les GIFs doivent être dans le dossier `docs/` ou hébergés en ligne)*

---

## 🛠️ Installation

```bash
# Cloner le repo
git clone https://github.com/tonpseudo/ton-projet.git

# Ouvrir dans Unity (ou autre moteur)
# puis lancer la scène principale