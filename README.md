# Infinite-Walmart-Game-3d
Game about a infinite type scp style game in 3d goal is to make a fun game!

A 3D SCP-style survival horror set in an infinitely deep superstore. By day the
store is open and the employees smile. At night, the intercom tells you to leave —
and you can't.

## Core loop
Scavenge scrap by morning light → craft, weld barricades, and fortify → survive the
night shift → ride the freight elevator deeper → repeat, forever downward.

## ▶ Play it
**`web/index.html` is a complete, playable WebGL build** — a single self-contained file
(Three.js embedded, all audio synthesized live, zero external assets). Open it in any
desktop browser and click CLOCK IN. Every system below is implemented and running:

- Day/night shift cycle driven by spoken intercom announcements
- 7 entity types: blind Stockers, cart-riding Rollers, screeching Greeters, the Night
  Shift Manager, light-fearing Backroom Crawlers, the Lost Child, blackout Specters
- Crouch-stealth noise/visibility contract, fear meter, stamina, register hiding
- Hold-E looting with cancellation fail-safes, crowbar armory breach
- Blackout events + generator repair race + permanent vault outcomes
- Two-tier crafting (Pipe Shotgun → Industrial Converter → EMP/fences/NVG/armor)
- Grab-rotate-weld barricades with durability, sieged by AI
- Freight elevator descent through seeded procedural floors and biomes
- Indoor weather: leak rain, growing slippery puddles, fog banks, lightning + thunder
- Night events: cart stampedes, price-check sirens, creepy intercom
- 15 collectible lore documents and a journal
- Pooled lights, instanced geometry, flow-field AI pathing, silhouette shadow models

## Status
`Assets/Scripts/` holds the engine-portable C# architecture (Unity/Godot) — event bus,
state machines, and system contracts. `web/index.html` is the playable reference
implementation of that architecture.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full system map.
