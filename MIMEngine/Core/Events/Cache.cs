﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMEngine.Core.Events
{

    using Room;
    using PlayerSetup;
    using System.Collections.Concurrent;

    public class Cache
    {
 
        /// <summary>
        /// Gets room cache
        /// </summary>
        /// <returns>returns room Cache</returns>
        private static ConcurrentDictionary<int, Room> getRoomCache()
        {
            return MIMHubServer.MimHubServer._AreaCache;
        }

        /// <summary>
        /// Gets player cache
        /// </summary>
        /// <returns>returns player Cache</returns>
        private static ConcurrentDictionary<string, Player> getPlayerCache()
        {
            return MIMHubServer.MimHubServer._PlayerCache;
        }

        /// <summary>
        /// Update room cache
        /// </summary>
        /// <param name="newRoom">The new room data</param>
        /// <param name="oldRoom">The old room data</param>
        public static bool updateRoom(Room newRoom, Room oldRoom)
        {
            if (MIMHubServer.MimHubServer._AreaCache.TryUpdate(oldRoom.areaId, newRoom, oldRoom))
            {
                return true;
            } 
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the room which the player is in
        /// </summary>
        /// <param name="player">Player that is in the room you wish to get</param>
        /// <returns></returns>
        public static Room getRoom(Player player)
        {
            Room roomData;
            MIMHubServer.MimHubServer._AreaCache.TryGetValue(player.AreaId, out roomData);

            return roomData;
        }

        /// <summary>
        /// Updates player in the cache
        /// </summary>
        /// <param name="newPlayer">The new player data</param>
        /// <param name="oldPlayer">The old player data</param>
        public static bool updatePlayer(Player newPlayer, Player oldPlayer)
        {

            if (MIMHubServer.MimHubServer._PlayerCache.TryUpdate(oldPlayer.HubGuid, newPlayer, oldPlayer))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns player from cache
        /// </summary>
        /// <param name="playerId">Player Hub ID to get from cache</param>
        /// <returns>PlayerData</returns>
        public static Player getPlayer(string playerId)
        {
            Player playerData;
            MIMHubServer.MimHubServer._PlayerCache.TryGetValue(playerId, out playerData);

            return playerData;
        }

    }
}
