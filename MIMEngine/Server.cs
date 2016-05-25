﻿using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using MIMEngine;
using MIMEngine.Core;
using MIMEngine.Core.PlayerSetup;
using Owin;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MIMHubServer
{
    using MIMEngine.Core.Events;
    using MIMEngine.Core.Room;
    using MongoDB.Bson;
    using Newtonsoft.Json.Linq;

    class Server
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading Server...");

            // This will *ONLY* bind to localhost, if you want to bind to all addresses
            // use http://*:8080 to bind to all addresses. 
            // See http://msdn.microsoft.com/en-us/library/system.net.httplistener.aspx 
            // for more information.
            string url = "http://localhost:4000";
            using (WebApp.Start<Startup>(url))
            {

                Console.WriteLine("Server running on {0}", url);
                Console.ReadLine();
            }


        }
    }

    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class MimHubServer : Hub
    {
        public static ConcurrentDictionary<string, Player> _PlayerCache = new ConcurrentDictionary<string, Player>();
        public static ConcurrentDictionary<int, Room> _AreaCache = new ConcurrentDictionary<int, Room>();

        public static Player PlayerData { get; set; }


        public void Welcome()
        {
            var motd = Data.loadFile("/motd");
            // Call the broadcastMessage method to update clients.
            //  SendToClient(motd, true);            
        }

        #region input from user
        public void recieveFromClient(string message, String playerGuid)
        {

   
            Player PlayerData;
             Room RoomData;
            _PlayerCache.TryGetValue(playerGuid, out PlayerData);
             _AreaCache.TryGetValue(PlayerData.AreaId, out RoomData);

            this.SendToClient(message, PlayerData.HubGuid);
            Command.ParseCommand(message, PlayerData, RoomData);

        }
        #endregion

        #region load and display room
        public static Room getRoom(string playerId)
        {
            Player player;

            if (_PlayerCache.TryGetValue(playerId, out player))
            {
 
                var RoomData = new LoadRoom();

                RoomData.Region = player.Region;
                RoomData.Area = player.Area;
                RoomData.id = player.AreaId;

                Room getRoomData = null;
                if (_AreaCache.TryGetValue(RoomData.id, out getRoomData))
                {

                    return getRoomData;

                }
                else
                {
                    getRoomData = RoomData.LoadRoomFile();
                    _AreaCache.TryAdd(RoomData.id, getRoomData);

                    return getRoomData;
                }
            }

            return null;
        }

        public string ReturnRoom(string id)
        {
            Player player;

            if (_PlayerCache.TryGetValue(id, out player))
            {
                string room = string.Empty;
                var roomJSON = new LoadRoom();

                roomJSON.Region = player.Region;
                roomJSON.Area = player.Area;
                roomJSON.id = player.AreaId;

                Room roomData;

                if (_AreaCache.TryGetValue(roomJSON.id, out roomData))
                {


                    room = LoadRoom.DisplayRoom(roomData, player.Name);

                }
                else
                {

                    roomData = roomJSON.LoadRoomFile();
                    _AreaCache.TryAdd(roomJSON.id, roomData);
                    room = LoadRoom.DisplayRoom(roomData, player.Name);

                }

                return room;
            }

            return null;
        }

        public void SaveRoom(Room room)
        {

            _AreaCache.TryAdd(room.areaId, room);
         

        }

        public void loadRoom(string id)
        {

            string roomData = ReturnRoom(id);

            this.Clients.Caller.addNewMessageToPage(roomData, true);

        }

        #endregion
 
        #region send data to player
        public void SendToClient(string message)
        {
            Clients.All.addNewMessageToPage(message);
        }

        public void SendToClient(string message, string id)
        {
            Clients.Client(id).addNewMessageToPage(message);
        }
        #endregion


        #region  Character Wizard & Setup

        public void charSetup(string id, string name, string email, string password, string gender, string race, string selectedClass, int strength, int dexterity, int constitution, int wisdom, int intelligence, int charisma)
        {

            //Creates and saves player
            PlayerData = new Player(id, name, email, password, gender, race, selectedClass, strength, dexterity, constitution, wisdom, intelligence, charisma);

            PlayerData.SavePlayerInformation();

            JObject playerJson = PlayerData.ReturnPlayerInformation();

            _PlayerCache.TryAdd(id, PlayerData);

            loadRoom(id);
            //add player to room
            Room roomData = null;
            _AreaCache.TryGetValue(PlayerData.AreaId, out roomData);

            MIMEngine.Core.Room.PlayerManager.AddPlayerToRoom(roomData, PlayerData);
            Movement.EnterRoom(PlayerData, roomData);

            Save.SavePlayer(PlayerData);

            // addToRoom(PlayerData.AreaId, roomData, PlayerData, "player");


        }

        public void Login(string id, string name, string password)
        {
            Player player = Save.GetPlayer(name, password).Result;

            if (player != null)
            {
                //update hubID
                player.HubGuid = id;

                _PlayerCache.TryAdd(id, player);

                loadRoom(player.HubGuid);

                //add player to room
                Room roomData = null;
                _AreaCache.TryGetValue(player.AreaId, out roomData);

                MIMEngine.Core.Room.PlayerManager.AddPlayerToRoom(roomData, player);
                Movement.EnterRoom(player, roomData);
            }
            else
            {
                //something went wrong
            }

           


        }

        public void getStats()
        {
            var playerStats = new PlayerStats();

            int[] stats = playerStats.rollStats();
            Clients.Caller.setStats(stats);
        }

        public void characterSetupWizard(string value, string step)
        {


            if (step == "race")
            {
                var selectedRace = PlayerRace.selectRace(value);
                Clients.Caller.updateCharacterSetupWizard("race", selectedRace.name, selectedRace.help, selectedRace.imgUrl);
            }
            else if (step == "class")
            {
                var selectedClass = PlayerClass.selectClass(value);
                Clients.Caller.updateCharacterSetupWizard("class", selectedClass.name, selectedClass.description, selectedClass.imgUrl);
            }

        }

        #endregion
    }
}
