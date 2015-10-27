using System;
using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.HabboHotel.Achievements.Structs;
using Azure.Util;

namespace Azure.HabboHotel.Achievements.Factories
{
    /// <summary>
    ///     Class AchievementLevelFactory.
    /// </summary>
    internal class AchievementLevelFactory
    {
        /// <summary>
        ///     Gets the achievement levels.
        /// </summary>
        /// <param name="achievements">The achievements.</param>
        /// <param name="dbClient">The database client.</param>
        internal static void GetAchievementLevels(out Dictionary<string, Achievement> achievements, IQueryAdapter dbClient)
        {
            achievements = new Dictionary<string, Achievement>();

            dbClient.SetQuery("SELECT * FROM achievements_data");

            DataTable table = dbClient.GetTable();

            foreach (DataRow dataRow in table.Rows)
            {
                string achievementName = (string)dataRow["group_name"];

                AchievementLevel level = new AchievementLevel((int)dataRow["level"], (int)dataRow["reward_pixels"], (int)dataRow["reward_points"], (int)dataRow["progress_needed"]);

                if (!achievements.ContainsKey(achievementName))
                {
                    Achievement achievement = new Achievement((uint)dataRow["id"], achievementName, (string)dataRow["category"]);

                    achievements.Add(achievementName, achievement);
                    achievement.AddLevel(level);            
                }
                else
                {
                    if (!achievements[achievementName].CheckLevel(level))
                        achievements[achievementName].AddLevel(level);
                    else
                        Out.WriteLine("Was Found a Duplicated Level for: " + achievementName + ", Level: " + level.Level, "[Azure.Achievements]", ConsoleColor.Cyan);
                }
            }
        }
    }
}