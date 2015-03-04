﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Herbfunk.GarrisonBase.Cache;
using Herbfunk.GarrisonBase.Garrison;
using Styx;
using Styx.CommonBot;
using Styx.CommonBot.Coroutines;
using Styx.WoWInternals.WoWObjects;

namespace Herbfunk.GarrisonBase
{
    public partial class Behaviors
    {
        public class BehaviorCache: Behavior
        {
            public override BehaviorType Type { get { return BehaviorType.Cache; } }

            public BehaviorCache()
                : base(MovementCache.GarrisonEntrance, GarrisonManager.GarrisonResourceCacheEntryId)
            {
            }

            public override void Initalize()
            {
                if (MovementCache.Garrison != null && !MovementCache.Garrison.LocationInsidePolygon(StyxWoW.Me.Location))
                    MovementPoints.Insert(0, MovementCache.Garrison.Exit);

                base.Initalize();
            }
            public override Func<bool> Criteria
            {
                get
                {
                    return () => (GarrisonResourceCacheObject != null && GarrisonResourceCacheObject.ref_WoWObject.IsValid);
                }
            }

            public C_WoWGameObject GarrisonResourceCacheObject
            {
                get { return ObjectCacheManager.GetWoWGameObjects(CacheStaticLookUp.ResourceCacheIds.ToArray()).FirstOrDefault(); }
            }

            public override async Task<bool> Movement()
            {
                TreeRoot.StatusText = String.Format("Behavior {0} Movement", Type.ToString());
                if (await base.Movement())
                    return true;

                TreeRoot.StatusText = String.Format("Behavior {0} Movement2", Type.ToString());
                //Move to the interaction object (within 6.7f)
                if (_movement == null)
                {
                    _movement = new Movement(GarrisonResourceCacheObject.Location, 5.75f);
                }

                if (await _movement.MoveTo())
                    return true;

                return false;
            }
            private Movement _movement;

            public override async Task<bool> Interaction()
            {
                TreeRoot.StatusText = String.Format("Behavior {0} Interaction", Type.ToString());
                if (GarrisonResourceCacheObject != null && GarrisonResourceCacheObject.ref_WoWObject.IsValid && GarrisonResourceCacheObject.GetCursor == WoWCursorType.InteractCursor)
                {
                    GarrisonResourceCacheObject.Interact();
                    await CommonCoroutines.SleepForRandomUiInteractionTime();
                    await CommonCoroutines.SleepForRandomReactionTime();
                    await Coroutine.Yield();
                    return true;
                }

                return false;
            }

            public override async Task<bool> BehaviorRoutine()
            {
                if (IsDone) return false;

                if (await base.BehaviorRoutine())
                    return true;

                if (await Movement())
                    return true;

                if (await Interaction())
                    return true;

                return false;
            }
        }
    }
}