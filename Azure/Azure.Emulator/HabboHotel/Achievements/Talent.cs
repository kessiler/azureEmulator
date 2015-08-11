namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    /// Class Talent.
    /// </summary>
    internal class Talent
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal int Id;

        /// <summary>
        /// The type
        /// </summary>
        internal string Type;

        /// <summary>
        /// The parent category
        /// </summary>
        internal int ParentCategory;

        /// <summary>
        /// The level
        /// </summary>
        internal int Level;

        /// <summary>
        /// The achievement group
        /// </summary>
        internal string AchievementGroup;

        /// <summary>
        /// The achievement level
        /// </summary>
        internal int AchievementLevel;

        /// <summary>
        /// The prize
        /// </summary>
        internal string Prize;

        /// <summary>
        /// The prize base item
        /// </summary>
        internal uint PrizeBaseItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="Talent"/> class.
        /// </summary>
        /// <param name="Id">The identifier.</param>
        /// <param name="Type">The type.</param>
        /// <param name="ParentCategory">The parent category.</param>
        /// <param name="Level">The level.</param>
        /// <param name="AchId">The ach identifier.</param>
        /// <param name="AchLevel">The ach level.</param>
        /// <param name="Prize">The prize.</param>
        /// <param name="PrizeBaseItem">The prize base item.</param>
        internal Talent(int Id, string Type, int ParentCategory, int Level, string AchId, int AchLevel, string Prize, uint PrizeBaseItem)
        {
            this.Id = Id;
            this.Type = Type;
            this.ParentCategory = ParentCategory;
            this.Level = Level;
            AchievementGroup = AchId;
            AchievementLevel = AchLevel;
            this.Prize = Prize;
            this.PrizeBaseItem = PrizeBaseItem;
        }

        /// <summary>
        /// Gets the achievement.
        /// </summary>
        /// <returns>Achievement.</returns>
        internal Achievement GetAchievement()
        {
            if (string.IsNullOrEmpty(AchievementGroup) || ParentCategory == -1)
                return null;
            return Azure.GetGame().GetAchievementManager().GetAchievement(AchievementGroup);
        }
    }
}