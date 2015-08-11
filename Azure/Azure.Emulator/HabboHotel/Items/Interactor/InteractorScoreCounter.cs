#region

using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Rooms;
using Azure.HabboHotel.Rooms.Games;

#endregion

namespace Azure.HabboHotel.Items.Interactor
{
    internal class InteractorScoreCounter : IFurniInteractor
    {
        public void OnPlace(GameClient session, RoomItem item)
        {
            if (item.Team == Team.none)
            {
                return;
            }
            item.ExtraData = item.GetRoom().GetGameManager().Points[(int)item.Team].ToString();
            item.UpdateState(false, true);
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
                        num++;
                        break;

                    case 2:
                        num--;
                        break;

                    case 3:
                        num = 0;
                        break;
                }
                item.ExtraData = num.ToString();
                item.UpdateState(false, true);
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
                num++;
                item.ExtraData = num.ToString();
                item.UpdateState(false, true);
            }
        }
    }
}