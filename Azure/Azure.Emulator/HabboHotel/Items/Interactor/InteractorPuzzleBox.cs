#region

using System;
using System.Drawing;
using System.Linq;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.PathFinding;
using Azure.HabboHotel.Rooms;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorPuzzleBox : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (session == null)
            {
                return;
            }
            RoomUser roomUserByHabbo = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUserByHabbo == null)
            {
                return;
            }

            if (PathFinder.GetDistance(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y) > 1)
            {
                roomUserByHabbo.MoveTo(item.X + 1, item.Y);
            }

            if (Math.Abs(roomUserByHabbo.X - item.X) < 2 && Math.Abs(roomUserByHabbo.Y - item.Y) < 2)
            {
                roomUserByHabbo.SetRot(PathFinder.CalculateRotation(roomUserByHabbo.X, roomUserByHabbo.Y, item.X, item.Y), false);
                Room room = item.GetRoom();
                var point = new Point(0, 0);
                switch (roomUserByHabbo.RotBody)
                {
                    case 4:
                        point = new Point(item.X, item.Y + 1);
                        break;

                    case 0:
                        point = new Point(item.X, item.Y - 1);
                        break;

                    case 6:
                        point = new Point(item.X - 1, item.Y);
                        break;

                    default:
                        if (roomUserByHabbo.RotBody != 2)
                        {
                            return;
                        }
                        point = new Point(item.X + 1, item.Y);
                        break;
                }
                if (!room.GetGameMap().ValidTile2(point.X, point.Y))
                {
                    return;
                }
                var coordinatedItems = room.GetGameMap().GetCoordinatedItems(point);
                if (coordinatedItems.Any(i => !i.GetBaseItem().Stackable)) return;
                double num = item.GetRoom().GetGameMap().SqAbsoluteHeight(point.X, point.Y);
                var serverMessage = new ServerMessage();
                serverMessage.Init(LibraryParser.OutgoingRequest("ItemAnimationMessageComposer"));
                serverMessage.AppendInteger(item.X);
                serverMessage.AppendInteger(item.Y);
                serverMessage.AppendInteger(point.X);
                serverMessage.AppendInteger(point.Y);
                serverMessage.AppendInteger(1);
                serverMessage.AppendInteger(item.Id);
                serverMessage.AppendString(item.Z.ToString(Azure.CultureInfo));
                serverMessage.AppendString(num.ToString(Azure.CultureInfo));
                serverMessage.AppendInteger(0);
                room.SendMessage(serverMessage);
                item.GetRoom().GetRoomItemHandler().SetFloorItem(roomUserByHabbo.GetClient(), item, point.X, point.Y, item.Rot, false, false, false);
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
        }
    }
}