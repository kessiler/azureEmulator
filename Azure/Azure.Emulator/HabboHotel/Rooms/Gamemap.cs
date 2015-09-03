#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using Azure.Configuration;
using Azure.HabboHotel.GameClients;
using Azure.HabboHotel.Items;
using Azure.HabboHotel.Pathfinding;
using Azure.HabboHotel.PathFinding;
using Azure.HabboHotel.Rooms.Games;
using Azure.HabboHotel.Rooms.Wired.Handlers;
using Azure.Messages;
using Azure.Messages.Parsers;

#endregion

namespace Azure.HabboHotel.Rooms
{
    /// <summary>
    /// Class Gamemap.
    /// </summary>
    internal class Gamemap
    {
        /// <summary>
        /// The diagonal enabled
        /// </summary>
        internal bool DiagonalEnabled;

        /// <summary>
        /// The got public pool
        /// </summary>
        internal bool GotPublicPool;

        /// <summary>
        /// The serialized floormap
        /// </summary>
        internal ServerMessage SerializedFloormap;

        /// <summary>
        /// The walkable list
        /// </summary>
        internal HashSet<Point> WalkableList;

        /// <summary>
        /// The _room
        /// </summary>
        private Room UserRoom;

        /// <summary>
        /// The _user map
        /// </summary>
        private Dictionary<Point, List<RoomUser>> _userMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="Gamemap"/> class.
        /// </summary>
        /// <param name="room">The room.</param>
        /// <exception cref="System.Exception"></exception>
        public Gamemap(Room room)
        {
            UserRoom = room;
            DiagonalEnabled = true;
            StaticModel = Azure.GetGame().GetRoomManager().GetModel(room.RoomData.ModelName, room.RoomId);
            if (StaticModel == null)
                throw new ArgumentNullException(string.Format("No modeldata found for roomID {0}", room.RoomId));
            Model = new DynamicRoomModel(StaticModel, room);
            CoordinatedItems = new HybridDictionary();
            GotPublicPool = room.RoomData.Model.GotPublicPool;
            GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            _userMap = new Dictionary<Point, List<RoomUser>>();
            WalkableList = GetWalkablePoints();
            GuildGates = new Dictionary<Point, RoomItem>();
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>The model.</value>
        internal DynamicRoomModel Model { get; private set; }

        /// <summary>
        /// Gets the static model.
        /// </summary>
        /// <value>The static model.</value>
        internal RoomModel StaticModel { get; private set; }

        /// <summary>
        /// Gets the effect map.
        /// </summary>
        /// <value>The effect map.</value>
        internal byte[,] EffectMap { get; private set; }

        /// <summary>
        /// Gets the coordinated items.
        /// </summary>
        /// <value>The coordinated items.</value>
        internal HybridDictionary CoordinatedItems { get; private set; }

        /// <summary>
        /// The guild gates
        /// </summary>
        internal Dictionary<Point, RoomItem> GuildGates;

        /// <summary>
        /// Gets the game map.
        /// </summary>
        /// <value>The game map.</value>
        internal byte[,] GameMap { get; private set; }

        /// <summary>
        /// Gets the item height map.
        /// </summary>
        /// <value>The item height map.</value>
        internal double[,] ItemHeightMap { get; private set; }

        /// <summary>
        /// Determines whether this instance can walk the specified p state.
        /// </summary>
        /// <param name="pState">State of the p.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        /// <returns><c>true</c> if this instance can walk the specified p state; otherwise, <c>false</c>.</returns>
        internal static bool CanWalk(byte pState, bool pOverride)
        {
            return pOverride || pState == 3 || pState == 1;
        }

        /// <summary>
        /// Gets the affected tiles.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="width">The width.</param>
        /// <param name="posX">The position x.</param>
        /// <param name="posY">The position y.</param>
        /// <param name="rotation">The rotation.</param>
        /// <returns>Dictionary&lt;System.Int32, ThreeDCoord&gt;.</returns>
        internal static Dictionary<int, ThreeDCoord> GetAffectedTiles(int length, int width,
            int posX, int posY, int rotation)
        {
            var x = 0;
            var pointList = new Dictionary<int, ThreeDCoord>();
            if (length == 1 && width == 1)
                pointList.Add(x++, new ThreeDCoord(posX, posY, 0));
            if (length > 1)
                switch (rotation)
                {
                    case 4:
                    case 0:
                        for (var i = 0; i < length; i++)
                        {
                            pointList.Add(x++, new ThreeDCoord(posX, posY + i, i));
                            for (var j = 1; j < width; j++)
                                pointList.Add(x++, new ThreeDCoord(posX + j, posY + i, (i < j) ? j : i));
                        }
                        break;

                    case 6:
                    case 2:
                        for (var i = 0; i < length; i++)
                        {
                            pointList.Add(x++, new ThreeDCoord(posX + i, posY, i));
                            for (var j = 1; j < width; j++)
                                pointList.Add(x++, new ThreeDCoord(posX + i, posY + j, (i < j) ? j : i));
                        }
                        break;
                }
            if (width <= 1)
                return pointList;
            switch (rotation)
            {
                case 4:
                case 0:
                    for (var i = 0; i < width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(posX + i, posY, i));
                        for (var j = 1; j < length; j++)
                            pointList.Add(x++, new ThreeDCoord(posX + i, posY + j, (i < j) ? j : i));
                    }
                    break;

                case 6:
                case 2:
                    for (var i = 0; i < width; i++)
                    {
                        pointList.Add(x++, new ThreeDCoord(posX, posY + i, i));
                        for (var j = 1; j < length; j++)
                            pointList.Add(x++, new ThreeDCoord(posX + j, posY + i, (i < j) ? j : i));
                    }
                    break;
            }

            return pointList;
        }

        /// <summary>
        /// Tileses the touching.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal static bool TilesTouching(int x1, int y1, int x2, int y2)
        {
            return (Math.Abs(x1 - x2) <= 1 && Math.Abs(y1 - y2) <= 1) || (x1 == x2 && y1 == y2);
        }

        /// <summary>
        /// Tiles the distance.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>System.Int32.</returns>
        internal static int TileDistance(int x1, int y1, int x2, int y2)
        {
            return (Math.Abs(x1 - x2) + Math.Abs(y1 - y2));
        }

        /// <summary>
        /// Adds the user to map.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="coord">The coord.</param>
        internal void AddUserToMap(RoomUser user, Point coord)
        {
            if (_userMap.ContainsKey(coord))
            {
                _userMap[coord].Add(user);
                return;
            }
            _userMap.Add(coord, new List<RoomUser> { user });
        }

        /// <summary>
        /// Teleports to item.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="item">The item.</param>
        internal void TeleportToItem(RoomUser user, RoomItem item)
        {
            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y),
                new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.X;
            user.Y = item.Y;
            user.Z = item.Z;
            user.SqState = GameMap[item.X, item.Y];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rot;
            user.RotHead = item.Rot;
            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        /// <summary>
        /// Updates the user movement.
        /// </summary>
        /// <param name="oldCoord">The old coord.</param>
        /// <param name="newCoord">The new coord.</param>
        /// <param name="user">The user.</param>
        internal void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        /// <summary>
        /// Removes the user from map.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="coord">The coord.</param>
        internal void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (user == null) return;
            if (_userMap.ContainsKey(coord))
                _userMap[coord].Remove(user);
        }

        /// <summary>
        /// Maps the got user.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool MapGotUser(Point coord)
        {
            return GetRoomUsers(coord).Any();
        }

        /// <summary>
        /// Gets the room users.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>List&lt;RoomUser&gt;.</returns>
        internal List<RoomUser> GetRoomUsers(Point coord)
        {
            return _userMap.ContainsKey(coord) ? _userMap[coord] : new List<RoomUser>();
        }

        /// <summary>
        /// Gets the random walkable square.
        /// </summary>
        /// <returns>Point.</returns>
        internal Point GetRandomWalkableSquare()
        {
            var list = WalkableList;
            if (!list.Any())
                return new Point(0, 0);

            {
                var randomNumber = new Random().Next(0, list.Count);
                var num = 0;
                foreach (var current in list)
                {
                    if (num == randomNumber)
                        return current;
                    num++;
                }
                return new Point(0, 0);
            }
        }

        /// <summary>
        /// Generates the map dump.
        /// </summary>
        /// <returns>System.String.</returns>
        internal string GenerateMapDump()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Game map:");
            for (var i = 0; i < Model.MapSizeY; i++)
            {
                var stringBuilder2 = new StringBuilder();
                for (var j = 0; j < Model.MapSizeX; j++) stringBuilder2.Append(GameMap[j, i].ToString());
                stringBuilder.AppendLine(stringBuilder2.ToString());
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Item height map:");
            for (var k = 0; k < Model.MapSizeY; k++)
            {
                var stringBuilder3 = new StringBuilder();
                for (var l = 0; l < Model.MapSizeX; l++) stringBuilder3.AppendFormat("[{0}]", ItemHeightMap[l, k]);
                stringBuilder.AppendLine(stringBuilder3.ToString());
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Static data:");
            for (var m = 0; m < Model.MapSizeY; m++)
            {
                var stringBuilder4 = new StringBuilder();
                for (var n = 0; n < Model.MapSizeX; n++) stringBuilder4.AppendFormat("[{0}]", Model.SqState[n][m]);
                stringBuilder.AppendLine(stringBuilder4.ToString());
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Static data height:");
            for (var num = 0; num < Model.MapSizeY; num++)
            {
                var stringBuilder5 = new StringBuilder();
                for (var num2 = 0; num2 < Model.MapSizeX; num2++) stringBuilder5.AppendFormat("[{0}]", Model.SqFloorHeight[num2][num]);
                stringBuilder.AppendLine(stringBuilder5.ToString());
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("Pool map:");
            for (var num3 = 0; num3 < Model.MapSizeY; num3++)
            {
                var stringBuilder6 = new StringBuilder();
                for (var num4 = 0; num4 < Model.MapSizeX; num4++) stringBuilder6.AppendFormat("[{0}]", EffectMap[num4, num3]);
                stringBuilder.AppendLine(stringBuilder6.ToString());
            }
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Adds to map.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void AddToMap(RoomItem item)
        {
            AddItemToMap(item, true);
        }

        /// <summary>
        /// Updates the map for item.
        /// </summary>
        /// <param name="item">The item.</param>
        internal void UpdateMapForItem(RoomItem item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        /// <summary>
        /// Generates the maps.
        /// </summary>
        /// <param name="checkLines">if set to <c>true</c> [check lines].</param>
        internal void GenerateMaps(bool checkLines = true)
        {
            try
            {
                var xMap = 0;
                var yMap = 0;

                CoordinatedItems = new HybridDictionary();

                if (checkLines)
                {
                    var roomItema = UserRoom.GetRoomItemHandler().FloorItems.Values.ToArray();
                    foreach (var roomItems in roomItema)
                    {
                        if (roomItems.X > Model.MapSizeX && roomItems.X > xMap)
                            xMap = roomItems.X;
                        if (roomItems.Y > Model.MapSizeY && roomItems.Y > yMap)
                            yMap = roomItems.Y;
                    }

                    Array.Clear(roomItema, 0, roomItema.Length);
                }

                if (yMap > Model.MapSizeY - 1 || xMap > Model.MapSizeX - 1)
                {
                    if (xMap < Model.MapSizeX)
                        xMap = Model.MapSizeX;
                    if (yMap < Model.MapSizeY)
                        yMap = Model.MapSizeY;
                    Model.SetMapsize(xMap + 7, yMap + 7);

                    GenerateMaps(false);

                    return;
                }

                if (xMap != StaticModel.MapSizeX || yMap != StaticModel.MapSizeY)
                {
                    EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                    for (var j = 0; j < Model.MapSizeY; j++)
                        for (var k = 0; k < Model.MapSizeX; k++)
                        {
                            GameMap[k, j] = 0;
                            EffectMap[k, j] = 0;
                            if (k == Model.DoorX && j == Model.DoorY)
                            {
                                GameMap[k, j] = 3;
                            }
                            else
                            {
                                switch (Model.SqState[k][j])
                                {
                                    case SquareState.Open:
                                        GameMap[k, j] = 1;
                                        break;

                                    case SquareState.Seat:
                                        GameMap[k, j] = 2;
                                        break;

                                    case SquareState.Pool:
                                        EffectMap[k, j] = 6;
                                        break;
                                }
                            }
                        }
                    if (GotPublicPool)
                    {
                        for (var l = 0; l < StaticModel.MapSizeY; l++)
                        {
                            for (var m = 0; m < StaticModel.MapSizeX; m++)
                            {
                                if (StaticModel.MRoomModelfx[m][l] != 0)
                                {
                                    EffectMap[m, l] = StaticModel.MRoomModelfx[m][l];
                                }
                            }
                        }
                    }
                }
                else
                {
                    EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    ItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                    for (var n = 0; n < Model.MapSizeY; n++)
                    {
                        for (var num3 = 0; num3 < Model.MapSizeX; num3++)
                        {
                            GameMap[num3, n] = 0;
                            EffectMap[num3, n] = 0;
                            if (num3 == Model.DoorX && n == Model.DoorY)
                            {
                                GameMap[num3, n] = 3;
                            }
                            else
                            {
                                switch (Model.SqState[num3][n])
                                {
                                    case SquareState.Open:
                                        GameMap[num3, n] = 1;
                                        break;

                                    case SquareState.Seat:
                                        GameMap[num3, n] = 2;
                                        break;

                                    case SquareState.Pool:
                                        EffectMap[num3, n] = 6;
                                        break;
                                }
                            }
                        }
                    }
                    if (GotPublicPool)
                    {
                        for (var num4 = 0; num4 < StaticModel.MapSizeY; num4++)
                        {
                            for (var num5 = 0; num5 < StaticModel.MapSizeX; num5++)
                            {
                                if (StaticModel.MRoomModelfx[num5][num4] != 0)
                                {
                                    EffectMap[num5, num4] = StaticModel.MRoomModelfx[num5][num4];
                                }
                            }
                        }
                    }
                }

                var roomItem = UserRoom.GetRoomItemHandler().FloorItems.Values;
                roomItem.Any(t => !AddItemToMap(t));

                if (!UserRoom.RoomData.AllowWalkThrough)
                {
                    foreach (var current in UserRoom.GetRoomUserManager().UserList.Values)
                    {
                        current.SqState = GameMap[current.X, current.Y];
                        GameMap[current.X, current.Y] = 0;
                    }
                }

                GameMap[Model.DoorX, Model.DoorY] = 3;
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception RoomData Loading on (GenerateMaps): " + ex);
            }
        }

        /// <summary>
        /// Adds the coordinated item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="coord">The coord.</param>
        internal void AddCoordinatedItem(RoomItem item, Point coord)
        {
            List<RoomItem> list;
            if (!CoordinatedItems.Contains(coord))
            {
                list = new List<RoomItem> { item };
                CoordinatedItems.Add(coord, list);
                return;
            }
            list = (List<RoomItem>)CoordinatedItems[coord];
            if (list.Contains(item))
                return;
            list.Add(item);
            CoordinatedItems[coord] = list;
        }

        /// <summary>
        /// Gets the coordinated items.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            if (CoordinatedItems.Contains(point) && CoordinatedItems[point] != null)
                return (List<RoomItem>)CoordinatedItems[point];
            return new List<RoomItem>();
        }

        /// <summary>
        /// Removes the coordinated item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="coord">The coord.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemoveCoordinatedItem(RoomItem item, Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            if (!CoordinatedItems.Contains(point))
                return false;
            ((List<RoomItem>)CoordinatedItems[point]).Remove(item);
            return true;
        }

        /// <summary>
        /// Removes from map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="handleGameItem">if set to <c>true</c> [handle game item].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemoveFromMap(RoomItem item, bool handleGameItem)
        {
            RemoveSpecialItem(item);
            if (UserRoom.GotSoccer())
                UserRoom.GetSoccer().OnGateRemove(item);
            var result = false;
            foreach (var current in item.GetCoords.Where(current => RemoveCoordinatedItem(item, current)))
                result = true;
            var hybridDictionary = new HybridDictionary();
            foreach (var current2 in item.GetCoords)
            {
                var point = new Point(current2.X, current2.Y);
                if (CoordinatedItems.Contains(point))
                {
                    var value = (List<RoomItem>)CoordinatedItems[point];
                    if (!hybridDictionary.Contains(current2))
                        hybridDictionary.Add(current2, value);
                }
                SetDefaultValue(current2.X, current2.Y);
            }
            foreach (Point point2 in hybridDictionary.Keys)
            {
                var list = (List<RoomItem>)hybridDictionary[point2];
                foreach (var current3 in list)
                    ConstructMapForItem(current3, point2);
            }
            if (GuildGates.ContainsKey(item.Coordinate))
                GuildGates.Remove(item.Coordinate);
            UserRoom.GetRoomItemHandler().OnHeightMapUpdate(hybridDictionary.Keys);
            hybridDictionary.Clear();
            hybridDictionary = null;

            return result;
        }

        /// <summary>
        /// Removes from map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RemoveFromMap(RoomItem item)
        {
            return RemoveFromMap(item, true);
        }

        /// <summary>
        /// Adds the item to map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="handleGameItem">if set to <c>true</c> [handle game item].</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool AddItemToMap(RoomItem item, bool handleGameItem, bool newItem = true)
        {
            if (handleGameItem)
            {
                AddSpecialItems(item);
                var interactionType = item.GetBaseItem().InteractionType;
                if (interactionType != Interaction.Roller)
                    switch (interactionType)
                    {
                        case Interaction.FootballGoalGreen:
                        case Interaction.FootballCounterGreen:
                        case Interaction.BanzaiGateGreen:
                        case Interaction.BanzaiScoreGreen:
                        case Interaction.FreezeGreenCounter:
                        case Interaction.FreezeGreenGate:
                            UserRoom.GetGameManager().AddFurnitureToTeam(item, Team.green);
                            break;

                        case Interaction.FootballGoalYellow:
                        case Interaction.FootballCounterYellow:
                        case Interaction.BanzaiGateYellow:
                        case Interaction.BanzaiScoreYellow:
                        case Interaction.FreezeYellowCounter:
                        case Interaction.FreezeYellowGate:
                            UserRoom.GetGameManager().AddFurnitureToTeam(item, Team.yellow);
                            break;

                        case Interaction.FootballGoalBlue:
                        case Interaction.FootballCounterBlue:
                        case Interaction.BanzaiGateBlue:
                        case Interaction.BanzaiScoreBlue:
                        case Interaction.FreezeBlueCounter:
                        case Interaction.FreezeBlueGate:
                            UserRoom.GetGameManager().AddFurnitureToTeam(item, Team.blue);
                            break;

                        case Interaction.FootballGoalRed:
                        case Interaction.FootballCounterRed:
                        case Interaction.BanzaiGateRed:
                        case Interaction.BanzaiScoreRed:
                        case Interaction.FreezeRedCounter:
                        case Interaction.FreezeRedGate:
                            UserRoom.GetGameManager().AddFurnitureToTeam(item, Team.red);
                            break;

                        case Interaction.FreezeExit:
                            UserRoom.GetFreeze().ExitTeleport = item;
                            break;

                        case Interaction.GuildGate:
                            {
                                if (!GuildGates.ContainsKey(item.Coordinate))
                                    GuildGates.Add(item.Coordinate, item);
                                break;
                            }
                    }
                else
                    UserRoom.GetRoomItemHandler().Rollers.TryAdd(item.Id, item);
            }
            if (item.GetBaseItem().Type != 's')
                return true;
            foreach (var coord in item.GetCoords.Select(current => new Point(current.X, current.Y)))
                AddCoordinatedItem(item, coord);

            {
                if (item.X > Model.MapSizeX - 1)
                {
                    Model.AddX();
                    GenerateMaps(true);
                    return false;
                }
                if (item.Y <= Model.MapSizeY - 1)
                    return item.GetCoords.All(current2 => ConstructMapForItem(item, current2));
                Model.AddY();
                GenerateMaps(true);
                return false;
            }
        }

        /// <summary>
        /// Determines whether this instance can walk the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="Override">if set to <c>true</c> [override].</param>
        /// <param name="horseId">The horse identifier.</param>
        /// <returns><c>true</c> if this instance can walk the specified x; otherwise, <c>false</c>.</returns>
        internal bool CanWalk(int x, int y, bool Override, uint horseId = 0u)
        {
            return (UserRoom.RoomData.AllowWalkThrough || Override || (UserRoom.GetRoomUserManager().GetUserForSquare(x, y) == null));
        }

        /// <summary>
        /// Adds the item to map.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="newItem">if set to <c>true</c> [new item].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool AddItemToMap(RoomItem item, bool newItem = true)
        {
            return AddItemToMap(item, true, newItem);
        }

        /// <summary>
        /// Gets the floor status.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>System.Byte.</returns>
        internal byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
                return 1;
            return GameMap[coord.X, coord.Y];
        }

        /// <summary>
        /// Gets the height for square from data.
        /// </summary>
        /// <param name="coord">The coord.</param>
        /// <returns>System.Double.</returns>
        internal double GetHeightForSquareFromData(Point coord)
        {
            try
            {
                var high = Model.SqFloorHeight.GetUpperBound(0);
                if (coord.X > high || coord.Y > Model.SqFloorHeight[high].GetUpperBound(0))
                    return 1.0;
                return Model.SqFloorHeight[coord.X][coord.Y];
            }
            catch (Exception)
            {
                return 1.0;
            }
        }

        /// <summary>
        /// Determines whether this instance [can roll item here] the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if this instance [can roll item here] the specified x; otherwise, <c>false</c>.</returns>
        internal bool CanRollItemHere(int x, int y)
        {
            return ValidTile(x, y) && Model.SqState[x][y] != SquareState.Blocked;
        }

        /// <summary>
        /// Squares the is open.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="pOverride">if set to <c>true</c> [p override].</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareIsOpen(int x, int y, bool pOverride)
        {
            return (Model.MapSizeX - 1 >= x && Model.MapSizeY - 1 >= y) && CanWalk(GameMap[x, y], pOverride);
        }

        /// <summary>
        /// Determines whether [is valid step3] [the specified user].
        /// </summary>
        /// <param name="RoomUser">The user.</param>
        /// <param name="ActualPosition">From.</param>
        /// <param name="NextPosition">To.</param>
        /// <param name="endOfPath">if set to <c>true</c> [end of path].</param>
        /// <param name="HasOverride">if set to <c>true</c> [override].</param>
        /// <param name="client">The client.</param>
        /// <returns><c>true</c> if [is valid step3] [the specified user]; otherwise, <c>false</c>.</returns>
        internal bool IsValidStep3(RoomUser RoomUser, Vector2D ActualPosition, Vector2D NextPosition, bool endOfPath, bool HasOverride, GameClient client)
        {
            if (RoomUser == null)
                return false;

            var NextPositionSquare = new Point(NextPosition.X, NextPosition.Y);
            var RoomUsersInTile = UserRoom.GetRoomUserManager().GetUserForSquare(NextPosition.X, NextPosition.Y);
            var RoomItemsInTile = GetCoordinatedItems(NextPositionSquare);

            if ((GuildGates.ContainsKey(NextPositionSquare)) && (RoomUser.GetClient() != null) && (RoomUser.GetClient().GetHabbo() != null) && (RoomUser.GetClient().GetHabbo().UserGroups != null))
            {
                var roomItem = GuildGates[NextPositionSquare];
                var guildId = roomItem.GroupId;

                if ((guildId > 0) && (RoomUser.GetClient().GetHabbo().UserGroups.Any(member => (member != null) && (member.GroupId == guildId))))
                {
                    roomItem.ExtraData = "1";
                    roomItem.UpdateState();
                    return true;
                }
            }

            if (!ValidTile(NextPosition.X, NextPosition.Y))
                return false;

            if (HasOverride)
                return true;

            if (!CanWalk(NextPosition.X, NextPosition.Y, HasOverride))
                return false;

            if (((GameMap[NextPosition.X, NextPosition.Y] == 3) && (!endOfPath)) || (GameMap[NextPosition.X, NextPosition.Y] == 0) || ((GameMap[NextPosition.X, NextPosition.Y] == 2) && (!endOfPath)))
            {
                RoomUser.Path.Clear();
                RoomUser.PathRecalcNeeded = false;
                return false;
            }

            if ((RoomUsersInTile != null) && (!RoomUsersInTile.IsWalking) && (endOfPath))
                return false;

            if (RoomItemsInTile.All(item => item.GetBaseItem().Walkable))
                return true;

            return ((SqAbsoluteHeight(NextPosition.X, NextPosition.Y) - SqAbsoluteHeight(ActualPosition.X, ActualPosition.Y)) <= 1.5);
        }

        /// <summary>
        /// Antis the choques.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="user">The user.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool AntiChoques(int x, int y, RoomUser user)
        {
            RoomUser roomUser;
            UserRoom.GetRoomUserManager().ToSet.TryGetValue(new Point(x, y), out roomUser);
            return (roomUser == null || roomUser == user);
        }

        /// <summary>
        /// Gets the random valid walkable square.
        /// </summary>
        /// <returns>Point.</returns>
        internal Point GetRandomValidWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1) - 1; y++)
                for (var x = 0; x < GameMap.GetUpperBound(0) - 1; x++)
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
            var randomNumber = Azure.GetRandomNumber(0, walkableSquares.Count);
            var i = 0;
            foreach (var coord in walkableSquares)
            {
                if (i == randomNumber)
                    return coord;
                i++;
            }
            return new Point(0, 0);
        }

        /// <summary>
        /// Determines whether [is valid step] [the specified user].
        /// </summary>
        /// <param name="RoomUser">The user.</param>
        /// <param name="StartPosition">From.</param>
        /// <param name="NextPosition">To.</param>
        /// <param name="endOfPath">if set to <c>true</c> [end of path].</param>
        /// <param name="HasOverride">if set to <c>true</c> [override].</param>
        /// <returns><c>true</c> if [is valid step] [the specified user]; otherwise, <c>false</c>.</returns>
        internal bool IsValidStep(RoomUser RoomUser, Vector2D StartPosition, Vector2D NextPosition, bool endOfPath, bool HasOverride)
        {
            if (RoomUser == null)
                return false;

            Point NextPositionSquare = new Point(NextPosition.X, NextPosition.Y);
            RoomUser RoomUsersInTile = UserRoom.GetRoomUserManager().GetUserForSquare(NextPosition.X, NextPosition.Y);
            List<RoomItem> RoomItemsInTile = GetCoordinatedItems(NextPositionSquare);

            if ((!RoomUser.IsBot) && (!RoomUser.IsPet) && (RoomUser.GetClient() != null))
            {
                if (GuildGates.ContainsKey(NextPositionSquare))
                {
                    uint GroupId = GuildGates[NextPositionSquare].GroupId;

                    if (GroupId > 0 && RoomUser.GetClient().GetHabbo().UserGroups.Any(member => ((member != null) && (member.GroupId == GroupId))))
                        return true;
                }
            }

            if (!ValidTile(NextPosition.X, NextPosition.Y))
                return false;

            if (HasOverride)
                return true;

            if (!CanWalk(NextPosition.X, NextPosition.Y, HasOverride))
                return false;

            if ((RoomUsersInTile != null) && (!RoomUsersInTile.IsWalking) && (endOfPath))
                return false;

            if (((GameMap[NextPosition.X, NextPosition.Y] == 3) && (!endOfPath)) || (GameMap[NextPosition.X, NextPosition.Y] == 0) || ((GameMap[NextPosition.X, NextPosition.Y] == 2) && (!endOfPath)))
                return false;

            if ((RoomUsersInTile != null) && (endOfPath) && (!UserRoom.RoomData.AllowWalkThrough))
            {
                RoomUser.HasPathBlocked = true;
                RoomUser.Path.Clear();
                RoomUser.IsWalking = false;
                RoomUser.RemoveStatus("mv");
                UserRoom.GetRoomUserManager().UpdateUserStatus(RoomUser, false);

                if (!RoomUser.RidingHorse || RoomUser.IsPet || RoomUser.IsBot)
                    return true;

                RoomUser roomUserByVirtualId = UserRoom.GetRoomUserManager().GetRoomUserByVirtualId(Convert.ToInt32(RoomUser.HorseId));
                ServerMessage message = new ServerMessage(LibraryParser.OutgoingRequest("UpdateUserStatusMessageComposer"));
                message.AppendInteger(1);

                if (roomUserByVirtualId != null)
                {
                    roomUserByVirtualId.IsWalking = false;
                    roomUserByVirtualId.ClearMovement();
                    roomUserByVirtualId.RemoveStatus("mv");
                    roomUserByVirtualId.SerializeStatus(message, "");
                }

                RoomUser.GetClient().GetHabbo().CurrentRoom.SendMessage(message);
            }
            else if (RoomItemsInTile.All(item => item.GetBaseItem().Walkable))
                return true;
            else if ((RoomUsersInTile != null) && (!UserRoom.RoomData.AllowWalkThrough) && (!RoomUsersInTile.IsWalking))
                return false;

            return ((SqAbsoluteHeight(NextPosition.X, NextPosition.Y) - SqAbsoluteHeight(StartPosition.X, StartPosition.Y)) <= 1.5);
        }

        /// <summary>
        /// Valids the tile2.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ValidTile2(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;
            bool result;
            try
            {
                result = (Model.SqState[x][y] == SquareState.Open);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Items the can be placed here.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ItemCanBePlacedHere(int x, int y)
        {
            return (Model.MapSizeX - 1 >= x && Model.MapSizeY - 1 >= y) && (x != Model.DoorX || y != Model.DoorY) && GameMap[x, y] == 1;
        }

        /// <summary>
        /// Sqs the height of the absolute.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>System.Double.</returns>
        internal double SqAbsoluteHeight(int x, int y)
        {
            try
            {
                var point = new Point(x, y);
                if (!CoordinatedItems.Contains(point))
                    return Model.SqFloorHeight[x][y];
                var itemsOnSquare = (List<RoomItem>)CoordinatedItems[point];
                return SqAbsoluteHeight(x, y, itemsOnSquare);
            }
            catch (Exception ex)
            {
                Logging.LogException("Exception RoomData Loading on (SqAbsoluteHeight): " + ex);
                return 0.0;
            }
        }

        /// <summary>
        /// Sqs the height of the absolute.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="itemsOnSquare">The items on square.</param>
        /// <returns>System.Double.</returns>
        internal double SqAbsoluteHeight(int x, int y, List<RoomItem> itemsOnSquare)
        {
            try
            {
                var highestStack = (double)Model.SqFloorHeight[x][y];
                var deductable = 0.0;

                foreach (
                    var item in
                        itemsOnSquare.Where(
                            item => item != null && item.GetBaseItem() != null && item.TotalHeight > highestStack))
                {
                    if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == Interaction.Bed || item.GetBaseItem().InteractionType == Interaction.PressurePadBed) deductable = item.GetBaseItem().Height;
                    highestStack = item.TotalHeight;
                }

                highestStack -= deductable;
                return highestStack < 0 ? 0 : highestStack;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SqAbsoluteHeight");
                return 0.0;
            }
        }

        /// <summary>
        /// Valids the tile.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ValidTile(int x, int y)
        {
            return ((x > 0) && (y > 0) && (x < Model.MapSizeX) && (y < Model.MapSizeY));
        }

        /// <summary>
        /// Gets the room item for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetRoomItemForSquare(int x, int y)
        {
            var point = new Point(x, y);
            var list = new List<RoomItem>();
            if (!CoordinatedItems.Contains(point))
                return list;

            var list2 = (List<RoomItem>)CoordinatedItems[point];
            list.AddRange(list2.Where(current => current.Coordinate.X == point.X && current.Coordinate.Y == point.Y));
            return list;
        }

        /// <summary>
        /// Squares the has furni.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareHasFurni(int x, int y, Interaction type)
        {
            var point = new Point(x, y);
            if (!CoordinatedItems.Contains(point))
                return false;

            var list = (List<RoomItem>)CoordinatedItems[point];
            return
                list.Any(
                    item =>
                        item.Coordinate.X == point.X && item.Coordinate.Y == point.Y &&
                        item.GetBaseItem().InteractionType == type);
        }

        /// <summary>
        /// Squares the has furni.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareHasFurni(int x, int y)
        {
            var point = new Point(x, y);
            if (!CoordinatedItems.Contains(point))
                return false;

            var list = (List<RoomItem>)CoordinatedItems[point];
            return list.Any(item => item.Coordinate.X == point.X && item.Coordinate.Y == point.Y);
        }

        /// <summary>
        /// Gets all room item for square.
        /// </summary>
        /// <param name="pX">The p x.</param>
        /// <param name="pY">The p y.</param>
        /// <returns>List&lt;RoomItem&gt;.</returns>
        internal List<RoomItem> GetAllRoomItemForSquare(int pX, int pY)
        {
            var point = new Point(pX, pY);
            var list = new List<RoomItem>();
            if (!CoordinatedItems.Contains(point))
                return list;
            var list2 = (List<RoomItem>)CoordinatedItems[point];
            foreach (var current in list2.Where(current => !list.Contains(current)))
                list.Add(current);
            return list;
        }

        /// <summary>
        /// Squares the has users.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool SquareHasUsers(int x, int y)
        {
            return MapGotUser(new Point(x, y));
        }

        /// <summary>
        /// Destroys this instance.
        /// </summary>
        internal void Destroy()
        {
            _userMap.Clear();
            Model.Destroy();
            CoordinatedItems.Clear();
            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(ItemHeightMap, 0, ItemHeightMap.Length);
            _userMap = null;
            GameMap = null;
            EffectMap = null;
            ItemHeightMap = null;
            CoordinatedItems = null;
            Model = null;
            UserRoom = null;
            StaticModel = null;
        }

        /// <summary>
        /// Gets the highest item for square.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>RoomItem.</returns>
        internal RoomItem GetHighestItemForSquare(int x, int y, out double z, RoomItem exception = null)
        {
            RoomItem roomItem = null;
            var num = -1.0;
            var num2 = 0.0;
            foreach (var current in GetRoomItemForSquare(x, y))
            {
                if (current.Z > num)
                {
                    num = current.Z;
                    num2 = current.GetBaseItem().Height;
                    roomItem = current;
                }
                if (exception == null || exception != roomItem)
                    continue;
                num = -1.0;
                num2 = 0.0;
                roomItem = null;
            }
            z = num + num2;
            return roomItem;
        }

        /// <summary>
        /// Gets the new heightmap.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        internal ServerMessage GetNewHeightmap()
        {
            if (SerializedFloormap != null)
                return SerializedFloormap;
            SerializedFloormap = NewHeightMap();
            return SerializedFloormap;
        }

        /// <summary>
        /// Sets the default value.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        private void SetDefaultValue(int x, int y)
        {
            try
            {
                GameMap[x, y] = 0;
                if (x == Model.DoorX && y == Model.DoorY)
                {
                    GameMap[x, y] = 3;
                    return;
                }
                switch (Model.SqState[x][y])
                {
                    case SquareState.Open:
                        GameMap[x, y] = 1;
                        return;

                    case SquareState.Seat:
                        GameMap[x, y] = 2;
                        break;
                }
                ItemHeightMap[x, y] = 0.0;
                EffectMap[x, y] = 0;
            }
            catch (Exception e)
            {
                Writer.Writer.LogException(e.ToString());
            }
        }

        /// <summary>
        /// Gets the walkable points.
        /// </summary>
        /// <returns>HashSet&lt;Point&gt;.</returns>
        private HashSet<Point> GetWalkablePoints()
        {
            var list = new HashSet<Point>();

            {
                for (var i = 0; i < GameMap.GetUpperBound(1) - 1; i++)
                    for (var j = 0; j < GameMap.GetUpperBound(0) - 1; j++)
                        if (StaticModel.DoorX != j && StaticModel.DoorY != i && GameMap[j, i] == 0)
                            list.Add(new Point(j, i));
                return list;
            }
        }

        /// <summary>
        /// Constructs the map for item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="coord">The coord.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ConstructMapForItem(RoomItem item, Point coord)
        {
            try
            {
                if (coord.X > (Model.MapSizeX - 1))
                {
                    Model.AddX();
                    GenerateMaps(true);
                    return false;
                }
                if (coord.Y > (Model.MapSizeY - 1))
                {
                    Model.AddY();
                    GenerateMaps(true);
                    return false;
                }
                if (Model.SqState[coord.X][coord.Y] == SquareState.Blocked)
                {
                    Model.OpenSquare(coord.X, coord.Y, item.Z);
                    Model.SetUpdateState();
                }

                if (ItemHeightMap[coord.X, coord.Y] <= item.TotalHeight)
                {
                    ItemHeightMap[coord.X, coord.Y] = item.TotalHeight - Model.SqFloorHeight[item.X][item.Y];

                    EffectMap[coord.X, coord.Y] = 0;

                    Interaction interactionType = item.GetBaseItem().InteractionType;

                    switch (interactionType)
                    {
                        case Interaction.IceSkates:
                            EffectMap[coord.X, coord.Y] = 3;
                            break;

                        case Interaction.Normslaskates:
                            EffectMap[coord.X, coord.Y] = 2;
                            break;

                        case Interaction.LowPool:
                            EffectMap[coord.X, coord.Y] = 4;
                            break;

                        case Interaction.HaloweenPool:
                            EffectMap[coord.X, coord.Y] = 5;
                            break;

                        case Interaction.SnowBoardSlope:
                            EffectMap[coord.X, coord.Y] = 7;
                            break;

                        case Interaction.Pool:
                            EffectMap[coord.X, coord.Y] = 1;
                            break;

                        case Interaction.PressurePadBed:
                        case Interaction.Bed:
                        case Interaction.Guillotine:
                        case Interaction.BedTent:
                            GameMap[coord.X, coord.Y] = 3;
                            break;
                    }


                    if (item.GetBaseItem().Walkable)
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    }
                    else if (item.Z <= Model.SqFloorHeight[item.X][item.Y] + 0.1 &&
                             item.GetBaseItem().InteractionType == Interaction.Gate && item.ExtraData == "1")
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    }
                    else if (item.GetBaseItem().IsSeat)
                        GameMap[coord.X, coord.Y] = 3;
                    else if (item.GetBaseItem().InteractionType == Interaction.Bed || item.GetBaseItem().InteractionType == Interaction.Guillotine || item.GetBaseItem().InteractionType == Interaction.BedTent)
                    {
                        //if (coord.X == item.X && coord.Y == item.Y)
                        GameMap[coord.X, coord.Y] = 3;
                    }
                    else if (GameMap[coord.X, coord.Y] != 3)
                        GameMap[coord.X, coord.Y] = 0;
                }
                if (item.GetBaseItem().InteractionType == Interaction.Bed || item.GetBaseItem().InteractionType == Interaction.Guillotine || item.GetBaseItem().InteractionType == Interaction.BedTent)
                    GameMap[coord.X, coord.Y] = 3;
            } 
            catch (Exception ex)
            {
                Logging.LogException(string.Concat("Error during map generation for room ", UserRoom.RoomId, ". Exception: ", ex.ToString()));
            }
            return true;
        }

        /// <summary>
        /// Adds the special items.
        /// </summary>
        /// <param name="item">The item.</param>
        private void AddSpecialItems(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.BanzaiFloor:
                    UserRoom.GetBanzai().AddTile(item, item.Id);
                    break;

                case Interaction.BanzaiTele:
                    UserRoom.GetGameItemHandler().AddTeleport(item, item.Id);
                    item.ExtraData = string.Empty;
                    break;

                case Interaction.BanzaiPuck:
                    UserRoom.GetBanzai().AddPuck(item);
                    break;

                case Interaction.BanzaiPyramid:
                    UserRoom.GetGameItemHandler().AddPyramid(item, item.Id);
                    break;

                case Interaction.FreezeExit:
                    var exitTeleport = UserRoom.GetFreeze().ExitTeleport;
                    if (exitTeleport == null || (int)item.Id != (int)exitTeleport.Id)
                        break;
                    UserRoom.GetFreeze().ExitTeleport = null;
                    break;

                case Interaction.FreezeTileBlock:
                    UserRoom.GetFreeze().AddFreezeBlock(item);
                    break;

                case Interaction.FreezeTile:
                    UserRoom.GetFreeze().AddFreezeTile(item);
                    break;

                case Interaction.Football:
                    UserRoom.GetSoccer().AddBall(item);
                    break;
            }
        }

        /// <summary>
        /// Removes the special item.
        /// </summary>
        /// <param name="item">The item.</param>
        private void RemoveSpecialItem(RoomItem item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case Interaction.BanzaiFloor:
                    UserRoom.GetBanzai().RemoveTile(item.Id);
                    break;

                case Interaction.BanzaiTele:
                    UserRoom.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;

                case Interaction.BanzaiPuck:
                    UserRoom.GetBanzai().RemovePuck(item.Id);
                    break;

                case Interaction.BanzaiPyramid:
                    UserRoom.GetGameItemHandler().RemovePyramid(item.Id);
                    break;

                case Interaction.FreezeTileBlock:
                    UserRoom.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;

                case Interaction.FreezeTile:
                    UserRoom.GetFreeze().RemoveFreezeTile(item.Id);
                    break;

                case Interaction.FootballGate:
                    //   this.room.GetSoccer().UnRegisterGate(item);
                    break;

                case Interaction.Football:
                    UserRoom.GetSoccer().RemoveBall(item.Id);
                    break;
            }
        }

        /// <summary>
        /// News the height map.
        /// </summary>
        /// <returns>ServerMessage.</returns>
        private ServerMessage NewHeightMap()
        {
            var serverMessage = new ServerMessage();
            serverMessage.Init(LibraryParser.OutgoingRequest("HeightMapMessageComposer"));
            serverMessage.AppendInteger(Model.MapSizeX);

            {
                serverMessage.AppendInteger(Model.MapSizeX * Model.MapSizeY);
                for (var i = 0; i < Model.MapSizeY; i++)
                    for (var j = 0; j < Model.MapSizeX; j++)
                        serverMessage.AppendShort((short)(SqAbsoluteHeight(j, i) * 256));
                //  serverMessage.AppendShort(this.Model.SqFloorHeight[j, i] * 256);
                return serverMessage;
            }
        }

        internal MovementState GetChasingMovement(int x, int y)
        {
            bool moveToLeft = true, moveToRight = true, moveToUp = true, moveToDown = true;

            for (var i = 1; i < 4; i++)
            {
                // Left
                if (i == 1 && !IsValidValueItem(x - i, y))
                    moveToLeft = false;
                else if (moveToLeft && SquareHasUsers(x - i, y))
                    return MovementState.Left;

                // Right
                if (i == 1 && !IsValidValueItem(x + i, y))
                    moveToRight = false;
                else if (moveToRight && SquareHasUsers(x + i, y))
                    return MovementState.Right;

                // Up
                if (i == 1 && !IsValidValueItem(x, y - i))
                    moveToUp = false;
                else if (moveToUp && SquareHasUsers(x, y - i))
                    return MovementState.Up;

                // Down
                if (i == 1 && !IsValidValueItem(x, y + i))
                    moveToDown = false;
                else if (moveToDown && SquareHasUsers(x, y + i))
                    return MovementState.Down;

                // Breaking bucle
                if (i == 1 && !moveToLeft && !moveToRight && !moveToUp && !moveToDown)
                    return MovementState.None;
            }

            var movements = new List<MovementState>();
            if (moveToLeft)
                movements.Add(MovementState.Left);
            if (moveToRight)
                movements.Add(MovementState.Right);
            if (moveToUp)
                movements.Add(MovementState.Up);
            if (moveToDown)
                movements.Add(MovementState.Down);

            return movements[Azure.GetRandomNumber(0, movements.Count)];
        }

        internal bool IsValidValueItem(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Model.MapSizeX || y >= Model.MapSizeY)
                return false;

            if (SquareHasUsers(x, y))
                return true;

            if (GetCoordinatedItems(new Point(x, y)).Count > 0 && !SquareIsOpen(x, y, false))
                return false;

            return Model.SqState[x][y] == SquareState.Open;
        }
    }
}