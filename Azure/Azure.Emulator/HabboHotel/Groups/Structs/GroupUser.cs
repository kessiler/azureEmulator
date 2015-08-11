namespace Azure.HabboHotel.Groups.Structs
{
    /// <summary>
    /// Class GroupUser.
    /// </summary>
    internal class GroupUser
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        /// The rank
        /// </summary>
        internal int Rank;

        /// <summary>
        /// The group identifier
        /// </summary>
        internal uint GroupId;

        /// <summary>
        /// The date of join on group
        /// </summary>
        internal int DateJoin;

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupUser"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="rank">The rank.</param>
        internal GroupUser(uint id, uint groupId, int rank, int dateJoin)
        {
            Id = id;
            GroupId = groupId;
            Rank = rank;
            DateJoin = dateJoin;
        }
    }
}