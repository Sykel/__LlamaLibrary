﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Buddy.Coroutines;
using Clio.Utilities;
using Clio.Utilities.Helpers;
using ff14bot;
using ff14bot.Behavior;
using ff14bot.Enums;
using ff14bot.Managers;
using ff14bot.Navigation;
using ff14bot.Objects;
using ff14bot.Pathing;
using ff14bot.RemoteWindows;
using LlamaLibrary.Logging;
using LlamaLibrary.RemoteWindows;
using TreeSharp;
using static ff14bot.RemoteWindows.Talk;

namespace LlamaLibrary.Helpers
{
    public static class Navigation
    {
        private static readonly LLogger Log = new LLogger("NavigationHelper", Colors.MediumPurple);

        public static readonly WaitTimer WaitTimer_0 = new WaitTimer(new TimeSpan(0, 0, 0, 15));

        internal static async Task<Queue<NavGraph.INode>> GenerateNodes(uint ZoneId, Vector3 xyz)
        {
            return await NavGraph.GetPathAsync(ZoneId, xyz);
        }

        public static async Task<bool> GetTo(uint ZoneId, Vector3 XYZ)
        {
            /*if (ZoneId == 620)
            {
                var AE = WorldManager.AetheryteIdsForZone(ZoneId).OrderBy(i => i.Item2.DistanceSqr(XYZ)).First();
                Log.Debug("Can teleport to AE");
                WorldManager.TeleportById(AE.Item1);
                await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                await Coroutine.Sleep(2000);
                return await FlightorMove(XYZ);
            }*/

            if (ZoneId == 401 && WorldManager.ZoneId == ZoneId)
            {
                return await FlightorMove(XYZ);
            }

            var path = await GenerateNodes(ZoneId, XYZ);

            if (ZoneId == 399 && path == null && WorldManager.ZoneId != ZoneId)
            {
                await GetToMap399();
            }

            if (path == null && WorldManager.ZoneId != ZoneId)
            {
                if (WorldManager.AetheryteIdsForZone(ZoneId).Length >= 1)
                {
                    var AE = WorldManager.AetheryteIdsForZone(ZoneId).OrderBy(i => i.Item2.DistanceSqr(XYZ)).First();

                    Log.Verbose("Can teleport to AE");
                    WorldManager.TeleportById(AE.Item1);
                    await Coroutine.Wait(20000, () => WorldManager.ZoneId == AE.Item1);
                    await Coroutine.Sleep(2000);
                    return await GetTo(ZoneId, XYZ);
                }
                else
                {
                    return false;
                }
            }

            if (path == null)
            {
                var result = await FlightorMove(XYZ);
                Navigator.Stop();
                return result;
            }

            if (path.Count < 1)
            {
                Log.Error($"Couldn't get a path to {XYZ} on {ZoneId}, Stopping.");
                return false;
            }

            var object_0 = new object();
            var composite = NavGraph.NavGraphConsumer(j => path);

            while (path.Count > 0)
            {
                composite.Start(object_0);
                await Coroutine.Yield();
                while (composite.Tick(object_0) == RunStatus.Running)
                {
                    await Coroutine.Yield();
                }

                composite.Stop(object_0);
                await Coroutine.Yield();
            }

            Navigator.Stop();

            return Navigator.InPosition(Core.Me.Location, XYZ, 3);
        }

        public static async Task OffMeshMove(Vector3 _target)
        {
            WaitTimer_0.Reset();
            Navigator.PlayerMover.MoveTowards(_target);
            while (_target.Distance2D(Core.Me.Location) >= 4 && !WaitTimer_0.IsFinished)
            {
                Navigator.PlayerMover.MoveTowards(_target);
                await Coroutine.Sleep(100);
            }

            Navigator.PlayerMover.MoveStop();
        }

        public static async Task<bool> OffMeshMoveInteract(GameObject _target)
        {
            WaitTimer_0.Reset();
            Navigator.PlayerMover.MoveTowards(_target.Location);
            while (!_target.IsWithinInteractRange && !WaitTimer_0.IsFinished)
            {
                Navigator.PlayerMover.MoveTowards(_target.Location);
                await Coroutine.Sleep(100);
            }

            Navigator.PlayerMover.MoveStop();
            return _target.IsWithinInteractRange;
        }

        public static async Task UseNpcTransition(uint oldzone, Vector3 transition, uint npcId, uint dialogOption)
        {
            await GetTo(oldzone, transition);

            var unit = GameObjectManager.GetObjectByNPCId(npcId);

            if (!unit.IsWithinInteractRange)
            {
                await OffMeshMoveInteract(unit);
            }

            unit.Target();
            unit.Interact();

            await Coroutine.Wait(5000, () => SelectIconString.IsOpen || DialogOpen);

            if (DialogOpen)
            {
                Next();
            }

            if (SelectIconString.IsOpen)
            {
                SelectIconString.ClickSlot(dialogOption);

                await Coroutine.Wait(5000, () => DialogOpen || SelectYesno.IsOpen);
            }

            if (DialogOpen)
            {
                Next();
            }

            await Coroutine.Wait(3000, () => SelectYesno.IsOpen);
            if (SelectYesno.IsOpen)
            {
                SelectYesno.Yes();
            }

            await Coroutine.Wait(3000, () => !SelectYesno.IsOpen);
        }

        public static async Task<bool> GetToMap399()
        {
            await GetTo(478, new Vector3(74.39938f, 205f, 140.4551f));
            Navigator.PlayerMover.MoveTowards(new Vector3(73.36626f, 205f, 142.026f));

            await Coroutine.Wait(10000, () => CommonBehaviors.IsLoading);
            Navigator.Stop();
            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            return WorldManager.ZoneId == 399;
        }

        public static async Task<bool> FlightorMove(Vector3 loc)
        {
            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(loc);
            while (!(moving == MoveResult.Done ||
                     moving == MoveResult.ReachedDestination ||
                     moving == MoveResult.Failed ||
                     moving == MoveResult.Failure ||
                     moving == MoveResult.PathGenerationFailed))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            return moving == MoveResult.ReachedDestination;
        }

        public static async Task<bool> FlightorMove(FateData fate)
        {
            if (fate == null)
            {
                return false;
            }

            var moving = MoveResult.GeneratingPath;
            var target = new FlyToParameters(fate.Location);
            while ((!(moving == MoveResult.Done ||
                      moving == MoveResult.ReachedDestination ||
                      moving == MoveResult.Failed ||
                      moving == MoveResult.Failure ||
                      moving == MoveResult.PathGenerationFailed)) && FateManager.ActiveFates.Any(i => i.Id == fate.Id && i.IsValid))
            {
                moving = Flightor.MoveTo(target);

                await Coroutine.Yield();
            }

            Navigator.PlayerMover.MoveStop();
            return moving == MoveResult.ReachedDestination;
        }

        public static async Task<bool> GetToIslesOfUmbra()
        {
            if (WorldManager.ZoneId == 138 && (WorldManager.SubZoneId == 461 || WorldManager.SubZoneId == 228))
            {
                return true;
            }

            await GetTo(138, new Vector3(317.4333f, -36.325f, 352.8649f));

            await UseNpcTransition(138, new Vector3(317.4333f, -36.325f, 352.8649f), 1003584, 2);

            await Coroutine.Sleep(1000);

            if (CommonBehaviors.IsLoading)
            {
                await Coroutine.Wait(-1, () => !CommonBehaviors.IsLoading);
            }

            await Coroutine.Sleep(1000);
            return WorldManager.ZoneId == 138 && (WorldManager.SubZoneId == 461 || WorldManager.SubZoneId == 228);
        }

        public static async Task<bool> GetToInteractNpc(uint npcId, ushort zoneId, Vector3 location, RemoteWindow window)
        {
            if (await GetTo(zoneId, location))
            {
                var unit = GameObjectManager.GetObjectByNPCId(npcId);

                if (unit != default(GameObject))
                {
                    if (!unit.IsWithinInteractRange)
                    {
                        await OffMeshMoveInteract(unit);
                    }

                    unit.Target();
                    unit.Interact();

                    await Coroutine.Wait(5000, () => window.IsOpen || DialogOpen);

                    if (DialogOpen)
                    {
                        await GeneralFunctions.SmallTalk();
                    }
                }
            }

            return window.IsOpen;
        }

        public static async Task<bool> GetToInteractNpcSelectString(uint npcId, ushort zoneId, Vector3 location, int selectStringIndex = -1, RemoteWindow nextWindow = null)
        {
            if (await GetTo(zoneId, location))
            {
                var unit = GameObjectManager.GetObjectByNPCId(npcId);

                if (unit != default(GameObject))
                {
                    if (!unit.IsWithinInteractRange)
                    {
                        await OffMeshMoveInteract(unit);
                    }

                    unit.Target();
                    unit.Interact();

                    await Coroutine.Wait(5000, () => Conversation.IsOpen || DialogOpen);

                    if (DialogOpen)
                    {
                        await GeneralFunctions.SmallTalk();
                        await Coroutine.Wait(5000, () => Conversation.IsOpen);
                    }
                }
            }

            if (selectStringIndex >= 0)
            {
                if (Conversation.IsOpen)
                {
                    Conversation.SelectLine((uint)selectStringIndex);
                    await Coroutine.Wait(5000, () => !Conversation.IsOpen || DialogOpen);

                    if (nextWindow != null)
                    {
                        await Coroutine.Wait(5000, () => nextWindow.IsOpen || DialogOpen);
                        if (DialogOpen)
                        {
                            await GeneralFunctions.SmallTalk();
                            await Coroutine.Wait(5000, () => nextWindow.IsOpen);
                        }

                        return nextWindow.IsOpen;
                    }

                    return true;
                }
            }

            return Conversation.IsOpen;
        }
    }
}