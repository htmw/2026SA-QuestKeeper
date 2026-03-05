# 🥋 Project Folder Structure & Guidelines

Welcome to the repo! To keep this project clean, prevent merge conflicts, and keep our game running smoothly, please follow these structure and naming rules. 

## 🚨 The Golden Rule
**EVERYTHING we make goes inside the `_Project` folder.** Do not put any original scripts, sprites, scenes, or audio in the root `Assets/` folder. The root folder is strictly for third-party Unity packages and plugins.

---

## 📁 Directory Breakdown

Please place your files in the appropriate subdirectories:

* **`Animations/`**: `.anim` files and Animator Controllers. (Keep sorted by character/entity).
* **`Audio/`**: BGM (Music), SFX (Sound Effects), and UI sounds.
* **`Data/`**: ScriptableObjects (e.g., Character stats, Enemy AI difficulty profiles, hit-box data).
* **`Prefabs/`**: Pre-built GameObjects. **Never edit a scene directly if you can edit a prefab instead.** 
* **`Scenes/`**: `.unity` scene files (Main Menu, Battle Stages, Sandbox/Testing).
* **`Scripts/`**: All C# code. Please use the subfolders (`Player`, `EnemyAI`, `UI`, `CoreManagers`).
* **`Sprites/`**: `.png` and `.psd` files. (Sorted into `Characters`, `Stages`, `UI`, `VFX`).
* **`UI/`**: Fonts, UI materials, and menu layouts.

---

## 🏷️ Naming Conventions

To make finding files easier, please use the following prefixes for your assets:

| Asset Type | Prefix | Example |
| :--- | :--- | :--- |
| **Scripts** | (None, PascalCase) | `PlayerController.cs`, `GameManager.cs` |
| **Sprites** | `Spr_` | `Spr_Ryu_PunchLight.png`, `Spr_UI_HealthBar.png` |
| **Prefabs** | `Prf_` | `Prf_Hitbox.prefab`, `Prf_Fireball.prefab` |
| **Animations** | `Anim_` | `Anim_Player_Idle.anim` |
| **Audio (SFX)** | `Sfx_` | `Sfx_Hit_Heavy.wav`, `Sfx_Menu_Click.wav` |
| **Audio (Music)**| `Bgm_` | `Bgm_Stage1.wav` |
| **Scenes** | `Scn_` | `Scn_MainMenu.unity`, `Scn_Battle_Alley.unity` |

---

## ⚠️ Git Best Practices for Unity
1. **Pull before you work:** Always get the latest changes before opening Unity.
2. **Never work on the same Scene file:** Git cannot merge Unity scenes. If two people edit `Scn_Battle_Alley.unity` at the same time, someone will lose their work. Communicate in chat before touching a scene file, or better yet, work on Prefabs!
3. **Commit often:** Keep your commits small and describe what you actually did.
4. **Use Feature Branches:** Never commit directly to the `main` branch! If you are starting a new task (e.g., coding enemy AI, adding UI elements, or importing new animations), create a new branch first and name it clearly (like `feature/enemy-ai-logic` or `art/ryu-sprites`). `main` must ALWAYS be a stable, playable version of the game. Once your feature is completely done and tested, we will merge it into `main`.