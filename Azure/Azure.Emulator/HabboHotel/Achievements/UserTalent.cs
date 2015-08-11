namespace Azure.HabboHotel.Achievements
{
    /// <summary>
    /// Class UserTalent.
    /// </summary>
    internal class UserTalent
    {
        /// <summary>
        /// The talent identifier
        /// </summary>
        internal int TalentId;

        /// <summary>
        /// The state
        /// </summary>
        internal int State;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserTalent"/> class.
        /// </summary>
        /// <param name="TalentId">The talent identifier.</param>
        /// <param name="State">The state.</param>
        public UserTalent(int TalentId, int State)
        {
            this.TalentId = TalentId;
            this.State = State;
        }
    }
}