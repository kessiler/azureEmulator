#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
        }

        public void OnRemove(GameClient session, RoomItem item)
        {
        }

        public void OnTrigger(GameClient session, RoomItem item, int request, bool hasRights)
        {
            if (!hasRights)
                return;
            if (item == null || item.GetBaseItem() == null || item.GetBaseItem().InteractionType != Interaction.Gate)
                return;

            var modes = item.GetBaseItem().Modes - 1;
            if (modes <= 0)
                item.UpdateState(false, true);

            if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null || item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                return;

            int currentMode;
            int.TryParse(item.ExtraData, out currentMode);
            int newMode;
            if (currentMode <= 0)
                newMode = 1;
            else if (currentMode >= modes)
                newMode = 0;
            else
                newMode = currentMode + 1;

            if (newMode == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                return;

            item.ExtraData = newMode.ToString();
            item.UpdateState();
            item.GetRoom().GetGameMap().UpdateMapForItem(item);
            item.GetRoom().GetWiredHandler().ExecuteWired(Interaction.TriggerStateChanged, item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id), item);
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
            {
                var num = item.GetBaseItem().Modes - 1;
                if (num <= 0)
                {
                    item.UpdateState(false, true);
                }
                if (item.GetRoom() == null || item.GetRoom().GetGameMap() == null || item.GetRoom().GetGameMap().SquareHasUsers(item.X, item.Y))
                    return;
                int num2 = 0;
                int.TryParse(item.ExtraData, out num2);
                int num3;
                if (num2 <= 0)
                {
                    num3 = 1;
                }
                else
                {
                    if (num2 >= num)
                    {
                        num3 = 0;
                    }
                    else
                    {
                        num3 = num2 + 1;
                    }
                }
                if (num3 == 0 && !item.GetRoom().GetGameMap().ItemCanBePlacedHere(item.X, item.Y))
                {
                    return;
                }
                item.ExtraData = num3.ToString();
                item.UpdateState();
                item.GetRoom().GetGameMap().UpdateMapForItem(item);
            }
        }
    }
}