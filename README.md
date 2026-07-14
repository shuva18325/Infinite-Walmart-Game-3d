# Infinite-Walmart-Game-3d
Game about a infinite type scp style game in 3d goal is to make a fun game!

A 3D SCP-style survival horror set in an infinitely deep superstore. By day the
store is open and the employees smile. At night, the intercom tells you to leave —
and you can't.

## Core loop
Scavenge scrap by morning light → craft, weld barricades, and fortify → survive the
night shift → ride the freight elevator deeper → repeat, forever downward.

## Status
Core systems architecture is in place under `Assets/Scripts/` — event bus, shift/AI
state machines, player controller + stealth contract, power/vault race, crafting tiers,
weapons, floor streaming, welding/barricades, and the lighting/shadow-model framework.
Implementation passes (math, tuning, VFX/SFX, assets) are marked inline with
`TODO(Opus)`.

See [ARCHITECTURE.md](ARCHITECTURE.md) for the full system map.
