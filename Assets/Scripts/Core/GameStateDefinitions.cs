// ============================================================================
// INFINITE WALMART — CORE STATE DEFINITIONS
// Shared enums, flags, and data payloads used across every system module.
// This file is the single source of truth for cross-system vocabulary.
// No logic lives here — only contracts.
// ============================================================================

using System;
using UnityEngine;

namespace InfiniteWalmart.Core
{
    // ------------------------------------------------------------------ SHIFT
    public enum ShiftPhase
    {
        MorningShift,   // Store open. Employees passive/friendly.
        NightShift      // Store closed. Employees hyper-hostile.
    }

    // ------------------------------------------------------------------ POWER
    public enum PowerState
    {
        PoweredOn,
        Blackout
    }

    public enum VaultOutcome
    {
        Unresolved,         // Race still in progress.
        PlayerUnlocked,     // Player repaired generator first → vault open forever.
        PermanentlyLocked   // Background AI repair finished first → vault sealed.
    }

    // ----------------------------------------------------------------- PLAYER
    public enum MovementState
    {
        Idle,
        Walking,
        Running,
        Crouching
    }

    /// How detectable the player currently is to entity vision sensors.
    public enum VisibilityState
    {
        Exposed,    // Standing / running in the open.
        Lowered,    // Crouching — reduced sight profile and noise radius.
        Hidden      // Fully concealed (e.g. under a cash register).
    }

    // --------------------------------------------------------------- EMPLOYEE
    public enum EmployeeBehaviorState
    {
        PassiveWander,      // Morning: friendly aisle wandering.
        Investigating,      // Heard/saw something — moving to check.
        Hunting,            // Night: active pursuit of the player.
        AttackingBarricade, // Breaking down a welded obstruction.
        Stunned             // Hit by electric fence / disabled.
    }

    public enum EmployeeTier
    {
        Stocker,            // Blind, hyper sound-sensitive.
        RollbackRoller,     // Fast, motorized cart rider.
        Greeter,            // Screeching alerter — pulls other tiers to player.
        NightShiftManager   // Towering boss entity.
    }

    // ------------------------------------------------------------------ ITEMS
    public enum ItemId
    {
        None = 0,

        // Raw survival scrap (early game)
        HardwareBolts,
        GunpowderContainer,
        SalvagedMetalPipe,
        WoodenDisplayHandle,

        // Tools
        Crowbar,
        WeldingTorch,

        // Vault registry items
        HighGradeBattery,
        CommonWires,
        PressureMachine,
        MilitaryAssaultRifle,

        // Crafted weapons
        ImprovisedPipeShotgun,

        // Industrial tier outputs
        IndustrialGradeConverter,
        AutomatedElectricFence,
        NightVisionGoggles,
        RiotHelmet,
        BulletproofVest,
        CombatBoots,

        // Base building
        BarricadeBlock
    }

    public enum ItemCategory
    {
        Scrap,
        Tool,
        Weapon,
        Ammo,
        BuildingBlock,
        Utility,
        Armor
    }

    public enum AmmoTier
    {
        Improvised,     // Shotgun scrap loads.
        Standard,
        MilitaryGrade
    }

    [Flags]
    public enum WeaponAttachmentFlags
    {
        None      = 0,
        Silencer  = 1 << 0,
        DrumMag   = 1 << 1,
        Flashlight= 1 << 2,
        ExtraGrip = 1 << 3
    }

    // --------------------------------------------------------------- CRAFTING
    public enum CraftingTier
    {
        Improvised,     // Bare-hands scrap assembly. Always available.
        Industrial      // Requires an activated Industrial-Grade Converter node.
    }

    // ------------------------------------------------------------------ NOISE
    /// Broadcast payload for anything audible. Sound-sensitive AI (Stockers)
    /// subscribe to these through the event bus rather than polling.
    public readonly struct NoiseEvent
    {
        public readonly Vector3 Origin;
        public readonly float Radius;       // World-space audible radius.
        public readonly float Loudness;     // 0..1 priority weight for AI arbitration.
        public readonly string SourceTag;   // "Footstep", "CrowbarBreach", "Gunshot"...

        public NoiseEvent(Vector3 origin, float radius, float loudness, string sourceTag)
        {
            Origin = origin;
            Radius = radius;
            Loudness = loudness;
            SourceTag = sourceTag;
        }
    }

    // ----------------------------------------------------------------- FLOORS
    public enum FloorBiome
    {
        RetailStandard,     // Classic aisles + departments.
        DeepBackrooms,      // Storage labyrinth. Sparse light.
        ColdStorage,
        GardenCenterOvergrown,
        ArmorySportingGoods // Rare — hosts the breachable armory department.
    }

    public readonly struct FloorTransitionInfo
    {
        public readonly int FromFloorIndex;
        public readonly int ToFloorIndex;
        public readonly FloorBiome ToBiome;
        public readonly int GenerationSeed;

        public FloorTransitionInfo(int from, int to, FloorBiome biome, int seed)
        {
            FromFloorIndex = from;
            ToFloorIndex = to;
            ToBiome = biome;
            GenerationSeed = seed;
        }
    }

    // --------------------------------------------------------------- INTERCOM
    public enum AnnouncementType
    {
        StoreOpening,       // "The store is now open. Welcome, valued customers!"
        StoreClosing,       // "The store is now closed. Please make your way to the exits immediately."
        BlackoutNotice,
        ElevatorTransit,    // Distorted static / voice clips during floor transit.
        AmbientCreepy       // Randomized unsettling filler lines.
    }

    public readonly struct IntercomAnnouncement
    {
        public readonly AnnouncementType Type;
        public readonly string ClipKey;     // Audio bank lookup key. Opus wires actual clips.

        public IntercomAnnouncement(AnnouncementType type, string clipKey)
        {
            Type = type;
            ClipKey = clipKey;
        }
    }
}
