﻿using System.Data;
using System.Text;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class FastWalk. This class cannot be inherited.
    /// </summary>
    internal sealed class UserFaq : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FastWalk" /> class.
        /// </summary>
        public UserFaq()
        {
            MinRank = 0;
            Description = "FAQ";
            Usage = ":faq";
            MinParams = 0;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var targetRoom = session.GetHabbo().CurrentRoom;
            DataTable data;
            using (var dbClient = Azure.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT question, answer FROM rooms_faq");
                data = dbClient.GetTable();
            }

            var builder = new StringBuilder();
            builder.Append(" - FAQ - \r\r");

            foreach (DataRow row in data.Rows)
            {
                builder.Append("Q: " + (string)row["question"] + "\r");
                builder.Append("A: " + (string)row["answer"] + "\r\r");
            }
            session.SendNotif(builder.ToString());
            return true;
        }
    }
}