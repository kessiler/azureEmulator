using System;
using Azure.Game.GameClients.Interfaces;
using Azure.Game.Items.Interactions.Models;
using Azure.Game.Items.Interfaces;
using Azure.Game.Rooms.User;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.Game.Items.Interactions.Controllers
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