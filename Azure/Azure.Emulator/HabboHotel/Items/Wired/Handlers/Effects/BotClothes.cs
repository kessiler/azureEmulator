﻿using System.Collections.Generic;
using Azure.HabboHotel.Items.Interactions.Enums;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Items.Wired.Interfaces;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Items.Wired.Handlers.Effects
{
    public class BotClothes : IWiredItem
    {
        public BotClothes(RoomItem item, Room room)
        {
            Item = item;
            Room = room;
            OtherString = string.Empty;
            OtherExtraString = string.Empty;
            OtherExtraString2 = string.Empty;
        }

        public Interaction Type => Interaction.ActionBotClothes;

        public RoomItem Item { get; set; }

        public Room Room { get; set; }

        public List<RoomItem> Items
        {
            get { return new List<RoomItem>(); }
            set { }
        }

        public int Delay
        {
            get { return 0; }
            set { }
        }

        public string OtherString { get; set; }

        public string OtherExtraString { get; set; }

        public string OtherExtraString2 { get; set; }

        public bool OtherBool { get; set; }

        public bool Execute(params object[] stuff)
        {
            var bot = Room.GetRoomUserManager().GetBotByName(OtherString);

            if (bot == null || OtherExtraString == "null")
                return false;

            bot.BotData.Look = OtherExtraString;
            var serverMessage = new ServerMessage(LibraryParser.OutgoingRequest("SetRoomUserMessageComposer"));
            serverMessage.AppendInteger(1);
            bot.Serialize(serverMessage, false);
            Room.SendMessage(serverMessage);

            return true;
        }
    }
}