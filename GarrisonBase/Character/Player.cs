﻿using System.Collections.Generic;
using Styx;
using Styx.Helpers;

namespace Herbfunk.GarrisonBase.Character
{
    public static class Player
    {
        public static PlayerInventory Inventory;
        public static PlayerProfessions Professions;

        
        internal static bool IsAlliance = false;
        internal static int Level = 0;
        internal static WoWClass Class= WoWClass.None;
        internal static bool Combat = false;
        internal static bool ActuallyInCombat = false;
        internal static WoWPoint Location = WoWPoint.Zero;
        internal static WoWPoint TraceLinePosition = WoWPoint.Zero;

        internal static CachedValue<uint> MapId;
        internal static CachedValue<int> GarrisonResource;
        internal static CachedValue<string> MinimapZoneText;
        internal static CachedValue<int> CurrentPendingCursorSpellId;
        internal static CachedValue<string> LastErrorMessage;

        internal static int AvailableGarrisonResource
        {
            get
            {
                var available = GarrisonResource - BaseSettings.CurrentSettings.ReservedGarrisonResources;
                return available < 0 ? 0 : available;
            }
        }
        internal static List<int> AuraSpellIds = new List<int>();

        internal static void Initalize()
        {
            
            IsAlliance = StyxWoW.Me.IsAlliance;
            Level = StyxWoW.Me.Level;
            Location = StyxWoW.Me.Location;
            Class = StyxWoW.Me.Class;
            Combat = StyxWoW.Me.Combat;
            ActuallyInCombat = StyxWoW.Me.IsActuallyInCombat;
            TraceLinePosition = StyxWoW.Me.GetTraceLinePos();
            Inventory = new PlayerInventory();
            Professions = new PlayerProfessions();

            MinimapZoneText = new CachedValue<string>(updateMinimapZoneText);
            MapId = new CachedValue<uint>(_updateMapId);
            GarrisonResource=new CachedValue<int>(_updateGarrisonResource);
            CurrentPendingCursorSpellId = new CachedValue<int>(_updateCurrentPendingCursorSpellId);
            LastErrorMessage = new CachedValue<string>(GetLastErrorMessage);

            LuaEvents.OnZoneChangedNewArea += () => MapId.Reset();
            LuaEvents.OnZoneChanged += () => MinimapZoneText.Reset();
            LuaEvents.OnCurrencyDisplayUpdate += () => GarrisonResource.Reset();
            LuaEvents.OnCurrentSpellCastChanged += () => CurrentPendingCursorSpellId.Reset();
            LuaEvents.OnUiErrorMessage += () => LastErrorMessage.Reset();

            Update();
        }

        internal static void Update()
        {
            using (StyxWoW.Memory.AcquireFrame())
            {
                Location = StyxWoW.Me.Location;
                TraceLinePosition = StyxWoW.Me.GetTraceLinePos();
                Combat = StyxWoW.Me.Combat;
                ActuallyInCombat = StyxWoW.Me.IsActuallyInCombat;
                RefreshAuraIds();
                Inventory.Update();
            }
            
        }

        
        
        private static string updateMinimapZoneText()
        {
            return StyxWoW.Me.MinimapZoneText;
        }
        private static uint _updateMapId()
        {
            return StyxWoW.Me.CurrentMap.MapId;
        }
        private static int _updateGarrisonResource()
        {
            return LuaCommands.GetCurrencyCount(824);
        }

        internal static void RefreshAuraIds()
        {
            AuraSpellIds.Clear();
            using (StyxWoW.Memory.AcquireFrame())
            {
                foreach (var item in StyxWoW.Me.GetAllAuras())
                {
                    AuraSpellIds.Add(item.SpellId);
                }
            }
        }

        private static int _updateCurrentPendingCursorSpellId()
        {
            if (StyxWoW.Me.CurrentPendingCursorSpell == null)
            {
                return -1;
            }
            return StyxWoW.Me.CurrentPendingCursorSpell.Id;
        }

        
        private static string GetLastErrorMessage()
        {
            string s = "";
            using (StyxWoW.Memory.AcquireFrame())
            {
                s = StyxWoW.LastRedErrorMessage;
            }
            return s;
        }



    }
}