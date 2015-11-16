using System;
using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;
using Azure.Game.Achievements.Structs;
using Azure.IO;

namespace Azure.Game.Achievements.Factories
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

            foreach (DataRow dataRow in dbClient.GetTable().Rows)
            {
                string achievementName = dataRow["achievement_name"].ToString();

                AchievementLevel level = new AchievementLevel((int)dataRow["achievement_level"], (int)dataRow["reward_pixels"], (int)dataRow["reward_points"], (int)dataRow["progress_needed"]);

                if (!achievements.ContainsKey(achievementName))
                    achievements.Add(achievementName, new Achievement((uint)dataRow["id"], achievementName, dataRow["achievement_category"].ToString()));        

                if (!achievements[achievementName].CheckLevel(level))
                    achievements[achievementName].AddLevel(level);
                else
                    Writer.WriteLine("Was Found a Duplicated Level for: " + achievementName + ", Level: " + level.Level, "[Azure.Achievements]", ConsoleColor.Cyan);
            }
        }
    }
}