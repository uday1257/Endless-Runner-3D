<<<<<<< HEAD
# Jungle Endless Runner 3D (Unity 6)

## Open it
1. Unity Hub > Add > Add project from disk > select the `JungleEndlessRunner` folder.
2. Use any Unity 6 editor (tested target: 6000.6). If Hub offers to change the editor version, accept.
3. Wait for the first import. The scene `Assets/Scenes/JungleLevel.unity` is created automatically.
   If it is not, use the menu Tools > Jungle Runner > Create or Rebuild JungleLevel Scene.
4. Press Play.

## Controls
- Left / Right arrow (or A / D): switch one lane per key press (left, middle, right). No diagonal drifting.
- Up arrow (or W / Space): jump
- P or Esc: pause   - H: home   - R: restart after game over
- On-screen PAUSE and HOME buttons while playing.

## Features
- Home screen with a card for each biome (Grassland, Jungle, Desert, Ice World, City) plus "All Biomes".
  Pick one to run only in that biome; keys 1-6 also work.
- 3 lives (hearts in the HUD). A hit costs one life, then you flash for 2 seconds. Game over at zero.

## What is inside
- Five biomes that cycle every 8 segments: Grassland, Jungle, Desert, Ice World, City.
  Sky, fog, light, sides and decor are colourful; the running lane is a darker texture so the lane stays readable.
- Obstacles (logs, rocks, trunks, barriers), spinning coins, score, best score, game over screen.
- No external assets needed: models, textures and sounds are generated in code.
- Works with the Built-in pipeline (default here) and URP (shader chosen automatically).

## Scripts (Assets/Scripts)
PlayerController, CoinCollector, CameraFollow (as in the lab PDF), plus GameManager, WorldGenerator,
Biomes, Decor, Gfx, Fx, Inp, Coin, Obstacle.

## Tweaks
- Lane darkness: `trackBase` colours in `Biomes.cs`.
- Biome length: `SegsPerBiome` in `WorldGenerator.cs`.
- Speed: `speed`, `maxSpeed`, `acceleration` in `PlayerController.cs`.

## Folder layout (Assets)
Animations, Audio, Editor (auto-setup script), Materials, Models, Prefabs, Resources, Scenes (JungleLevel), Scripts, Textures, UI.
Empty folders are ready for your own imported assets. The game itself generates its models, textures and sounds in code.
=======
# Endless-Runner-3D
>>>>>>> 9da8f44bed80bf120f497936eed564c5275f0fc2
