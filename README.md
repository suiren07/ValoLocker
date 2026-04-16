# ValoLocker

A lightweight tool that automatically locks your preferred agent in Valorant as soon as a match is found.

## Features

- **Default Agent Selection**
  Choose a primary agent to instalock every game.

- **Map-Specific Overrides**
  Customize agent picks for specific maps.  
  Example:
  - Default: **Clove**
  - On **Ascent** → lock **Sova**

- **No API Spam**
  This app does **not** spam API requests.
  It intelligently waits and sends **one perfectly timed request**, so your agent is locked in before the “Match Found” screen even fully appears.

---

## Screenshot

<img width="350" height="400" alt="AppScreenshot" src="https://github.com/user-attachments/assets/b2ae4259-cbf2-44aa-925a-f2da07b99ded" />

---

## How It Works

The app listens for the game state and waits for the exact moment agent selection becomes available. Instead of repeatedly calling the API, it executes a single precise request.

---

## Disclaimer

Use at your own risk.
