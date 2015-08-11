#region

using System;
using System.Collections.Generic;
using Azure.HabboHotel.Rooms;

#endregion

namespace Azure.HabboHotel.PathFinding
{
    /// <summary>
    /// Class PathFinder.
    /// </summary>
    internal class PathFinder
    {
        /// <summary>
        /// The diag move points
        /// </summary>
        public static Vector2D[] DiagMovePoints =
        {
            new Vector2D(-1, -1),
            new Vector2D(0, -1),
            new Vector2D(1, -1),
            new Vector2D(1, 0),
            new Vector2D(1, 1),
            new Vector2D(0, 1),
            new Vector2D(-1, 1),
            new Vector2D(-1, 0)
        };

        /// <summary>
        /// The no diag move points
        /// </summary>
        public static Vector2D[] NoDiagMovePoints =
        {
            new Vector2D(0, -1),
            new Vector2D(1, 0),
            new Vector2D(0, 1),
            new Vector2D(-1, 0)
        };

        /// <summary>
        /// Finds the path.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="diag">if set to <c>true</c> [diag].</param>
        /// <param name="map">The map.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>List&lt;Vector2D&gt;.</returns>
        public static List<Vector2D> FindPath(RoomUser user, bool diag, Gamemap map, Vector2D start, Vector2D end)
        {
            var list = new List<Vector2D>();
            var pathFinderNode = FindPathReversed(user, diag, map, start, end);
            if (pathFinderNode == null)
                return list;
            list.Add(end);
            while (pathFinderNode.Next != null)
            {
                list.Add(pathFinderNode.Next.Position);
                pathFinderNode = pathFinderNode.Next;
            }
            return list;
        }

        /// <summary>
        /// Finds the path reversed.
        /// </summary>
        /// <param name="RoomUserable">The user.</param>
        /// <param name="WhatIsDiag">if set to <c>true</c> [diag].</param>
        /// <param name="GameLocalMap">The map.</param>
        /// <param name="StartMap">The start.</param>
        /// <param name="EndMap">The end.</param>
        /// <returns>PathFinderNode.</returns>
        public static PathFinderNode FindPathReversed(RoomUser RoomUserable, bool WhatIsDiag, Gamemap GameLocalMap, Vector2D StartMap, Vector2D EndMap)
        {
            MinHeap<PathFinderNode> MinSpanTreeCost = new MinHeap<PathFinderNode>(256);
            PathFinderNode[,] PathFinderMap = new PathFinderNode[GameLocalMap.Model.MapSizeX, GameLocalMap.Model.MapSizeY];
            PathFinderNode PathFinderStart = new PathFinderNode(StartMap) { Cost = 0 };
            PathFinderNode PathFinderEnd = new PathFinderNode(EndMap);

            PathFinderMap[PathFinderStart.Position.X, PathFinderStart.Position.Y] = PathFinderStart;
            MinSpanTreeCost.Add(PathFinderStart);

            int loop_variable_one, InternalSpanTreeCost, loop_total_cost;

            while (MinSpanTreeCost.Count > 0)
            {
                PathFinderStart = MinSpanTreeCost.ExtractFirst();
                PathFinderStart.InClosed = true;

                loop_variable_one = 0;

                while ((WhatIsDiag ? (loop_variable_one < DiagMovePoints.Length) : (loop_variable_one < NoDiagMovePoints.Length)))
                {
                    Vector2D RealEndPosition = PathFinderStart.Position + (WhatIsDiag ? DiagMovePoints[loop_variable_one] : NoDiagMovePoints[loop_variable_one]);

                    bool IsEndOfPath = ((RealEndPosition.X == EndMap.X) && (RealEndPosition.Y == EndMap.Y));

                    if (GameLocalMap.IsValidStep(RoomUserable, new Vector2D(PathFinderStart.Position.X, PathFinderStart.Position.Y), RealEndPosition, IsEndOfPath, RoomUserable.AllowOverride))
                    {
                        PathFinderNode PathFinderSecondNodeCalculation;

                        if (PathFinderMap[RealEndPosition.X, RealEndPosition.Y] == null)
                        {
                            PathFinderSecondNodeCalculation = new PathFinderNode(RealEndPosition);
                            PathFinderMap[RealEndPosition.X, RealEndPosition.Y] = PathFinderSecondNodeCalculation;
                        }
                        else
                        {
                            PathFinderSecondNodeCalculation = PathFinderMap[RealEndPosition.X, RealEndPosition.Y];
                        }

                        if (!PathFinderSecondNodeCalculation.InClosed)
                        {
                            InternalSpanTreeCost = 0;

                            if (PathFinderStart.Position.X != PathFinderSecondNodeCalculation.Position.X)
                                InternalSpanTreeCost++;

                            if (PathFinderStart.Position.Y != PathFinderSecondNodeCalculation.Position.Y)
                                InternalSpanTreeCost++;

                            loop_total_cost = PathFinderStart.Cost + InternalSpanTreeCost + PathFinderSecondNodeCalculation.Position.GetDistanceSquared(EndMap);

                            if (loop_total_cost < PathFinderSecondNodeCalculation.Cost)
                            {
                                PathFinderSecondNodeCalculation.Cost = loop_total_cost;
                                PathFinderSecondNodeCalculation.Next = PathFinderStart;
                            }

                            if (!PathFinderSecondNodeCalculation.InOpen)
                            {
                                if (PathFinderSecondNodeCalculation.Equals(PathFinderEnd))
                                {
                                    PathFinderSecondNodeCalculation.Next = PathFinderStart;
                                    return PathFinderSecondNodeCalculation;
                                }

                                PathFinderSecondNodeCalculation.InOpen = true;
                                MinSpanTreeCost.Add(PathFinderSecondNodeCalculation);
                            }
                        }
                    }

                    loop_variable_one++;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the rotation.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        /// <returns>System.Int32.</returns>
        internal static int CalculateRotation(int x1, int y1, int x2, int y2)
        {
            int dX = x2 - x1;
            int dY = y2 - y1;

            double d = Math.Atan2(dY, dX) * 180 / Math.PI;
            return ((int)d + 90) / 45;
        }

        /// <summary>
        /// Gets the distance.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="toX">To x.</param>
        /// <param name="toY">To y.</param>
        /// <returns>System.Int32.</returns>
        public static int GetDistance(int x, int y, int toX, int toY)
        {
            return Convert.ToInt32(Math.Sqrt(Math.Pow(toX - x, 2) + Math.Pow(toY - y, 2)));
        }
    }
}