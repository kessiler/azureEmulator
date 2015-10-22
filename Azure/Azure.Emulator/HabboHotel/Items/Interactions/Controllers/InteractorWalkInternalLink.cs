using System;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.HabboHotel.Items.Interactions.Models;
using Azure.HabboHotel.Items.Interfaces;
using Azure.HabboHotel.Rooms.User;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Items.Interactions.Controllers
{
    internal class InteractorWalkInternalLink : FurniInteractorModel
    {
        public override void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
            if (item == null || user == null)
                return;

            var data = item.ExtraData.Split(Convert.ToChar(9));

            if (item.ExtraData == "" || data.Length < 4)
                return;

            var message = new ServerMessage(LibraryParser.OutgoingRequest("InternalLinkMessageComposer"));

            message.AppendString(data[3]);
            session.SendMessage(message);
        }
    }
}