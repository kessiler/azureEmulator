namespace Azure.HabboHotel.Groups.Structs
{
    /// <summary>
    /// Class GroupUser.
    /// </summary>
    internal class GroupMember
    {
        /// <summary>
        /// The identifier
        /// </summary>
        internal uint Id;

        /// <summary>
        /// The name
        /// </summary>
        internal string Name;

        /// <summary>
        /// The look
        /// </summary>
        internal string Look;

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
        /// Initializes a new instance of the <see cref="GroupMember"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="rank">The rank.</param>
        internal GroupMember(uint id, string name, string look, uint groupId, int rank, int dateJoin)
        {
            Id = id;
            Name = name;
            Look = look;
            GroupId = groupId;
            Rank = rank;
            DateJoin = dateJoin;
        }
    }
}