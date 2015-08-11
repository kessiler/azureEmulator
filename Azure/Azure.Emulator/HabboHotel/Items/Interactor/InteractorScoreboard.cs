#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorScoreboard : IFurniInteractor
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
            {
                return;
            }

            int num = 0;
            int.TryParse(item.ExtraData, out num);

            {
                switch (request)
                {
                    case 1:
                        if (item.PendingReset && num > 0)
                        {
                            num = 0;
                            item.PendingReset = false;
                        }
                        else
                        {
                            num += 60;
                            item.UpdateNeeded = false;
                        }
                        break;

                    case 2:
                        item.UpdateNeeded = !item.UpdateNeeded;
                        item.PendingReset = true;
                        break;
                }
                item.ExtraData = num.ToString();
                item.UpdateState();
            }
        }

        public void OnUserWalk(GameClient session, RoomItem item, RoomUser user)
        {
        }

        public void OnWiredTrigger(RoomItem item)
        {
            int num;
            int.TryParse(item.ExtraData, out num);

            {
                num += 60;
                item.UpdateNeeded = false;
                item.ExtraData = num.ToString();
                item.UpdateState();
            }
        }
    }
}