# SurakshaAR App

## Overview

SurakshaAR is an Augmented Reality (AR) based safety training application designed to provide immersive training experiences for fire safety and other emergency scenarios.

## Project Structure

```
app/
├── UI/                  # User Interface files (Unity UI Toolkit)
│   ├── UXML/           # UI layout files
│   ├── USS/            # UI style sheets
│   ├── Scripts/        # UI controller scripts
│   └── Components/     # Reusable UI components
├── Assets/             # Application assets
│   ├── Images/         # Image assets (Branding, Fire, Training, Icons, Backgrounds)
│   ├── Fonts/          # Font files
│   ├── Audio/          # Audio files
│   └── Animations/     # Animation files
├── Localization/       # Localization files
│   ├── English/        # English translations
│   ├── Hindi/          # Hindi translations
│   └── Santali/        # Santali translations
├── Data/               # Data files
│   ├── TrainingData/   # Training content data
│   ├── AssessmentData/ # Assessment questions and data
│   ├── UserData/       # User profile and progress data
│   └── AppSettings/    # Application settings
├── Events/             # Event system scripts
└── README.md           # This file
```

## Features

- **AR Training**: Immersive AR-based safety training scenarios
- **Multi-language Support**: English, Hindi, and Santali
- **Assessment System**: Track and evaluate training progress
- **Certificate Generation**: Award certificates upon successful completion
- **Offline Mode**: Training available without internet connection
- **User Profiles**: Track individual progress and achievements

## Main Menu (Fire Module)

The app main menu lives in `UI/Scripts/`:

- **`MainMenuController.cs`** — builds the main menu UI (UI Toolkit, fully in code):
  header, fire module intro card, "Top 7" section (placeholder leaderboard),
  and a **Start Module** button that loads the `FireTraining` scene.
- **`MainMenuBootstrapper.cs`** — `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`
  auto-creates the menu when the app opens, so the UI appears **without any
  scene or prefab wiring**. It also makes sure the menu does not re-appear over
  the AR training scene after "Start Module" is tapped.

### Scene wiring

`FireTraining` (in `ar_fire_foundation`) is already present in
`ProjectSettings/EditorBuildSettings.asset`, so `SceneManager.LoadScene("FireTraining")`
works out of the box.

These scripts live outside `Assets/` (in the `app` folder), so to make Unity
compile them, copy/sync `app/` into `Assets/` (e.g. `Assets/app/`) as part of
your build pipeline, or move the two scripts under any `Assets/**` folder.
No `ar_fire_foundation` files or scenes need to be modified for the main menu
to work.

## Localization (English, Hindi, Santali)

The main menu supports **three languages**: English (`en`), Hindi (`hi`) and
Santali (`sat`) - the local language.

- JSON translation files live in the `Localization` folder:

  ```
  app/Localization/English/main_menu.json
  app/Localization/Hindi/main_menu.json
  app/Localization/Santali/main_menu.json
  ```

- **`UI/Scripts/MainMenuLocalization.cs`** loads these strings. It first tries
  `Resources.Load<TextAsset>("Localization/main_menu_<code>")`; if that path is
  not available at build time, it falls back to built-in tables containing the
  same translations, so the menu still works in all three languages.

- The menu shows a **language selector** (English / Hindi / Santali)and
  remembers the choice via `PlayerPrefs` (`app_language`), matching the codebase's
  existing language keys (`en` / `hi` / `sat`).
- Add or fix translations by editing the JSON files - **no code changes needed**.
  Santali strings are best-effort (Devanagari script); review them with a native speaker।
- To serve JSON at runtime, keep a copy under a Unity Resources folder, e.g.:
  `Assets/Resources/Localization/main_menu_en.json` (plus `_hi`, `_sat`)。

## Getting Started

1. Ensure Unity 2021.3 LTS or later is installed
2. Open the project in Unity
3. Import required packages (AR Foundation, ARCore/ARKit)
4. Build and deploy to supported devices

## Supported Platforms

- Android (ARCore)
- iOS (ARKit)
