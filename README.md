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

- **Precise Lock Timing**  
  Sends a single API request at the exact right moment—so your agent is locked in before the “Match Found” screen even fully appears.

- **No API Spam**  
  This app does **not** spam REST API calls.  
  It intelligently waits and sends **one perfectly timed request**, reducing unnecessary traffic and avoiding instability.

---

## Screenshot

<img width="350" height="400" alt="AppScreenshot" src="https://github.com/user-attachments/assets/b2ae4259-cbf2-44aa-925a-f2da07b99ded" />

---

## How It Works

The app listens for the game state and waits for the exact moment agent selection becomes available. Instead of repeatedly calling the API, it executes a single precise request to guarantee an instant lock-in.

---

## Configuration

- Set your **default agent**
- Optionally define **map-specific agents**
- Run the app before queueing into a match

---

## Disclaimer

Use at your own risk.
