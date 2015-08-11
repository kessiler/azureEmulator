#region

using System;
using System.Collections.Generic;
using System.Data;
using Azure.HabboHotel.GameClients;

#endregion

namespace Azure.Security
{
    class BobbaFilter
    {
        internal static List<string> Word;

        internal static bool canTalk(GameClient Session, string Message)
        {
            if (CheckForBannedPhrases(Message) && Session.GetHabbo().Rank < 4)
            {
                if (!Azure.MutedUsersByFilter.ContainsKey(Session.GetHabbo().Id))
                    Session.GetHabbo().BobbaFiltered++;

                if (Session.GetHabbo().BobbaFiltered < 3)
                {
                    Session.SendNotif("Your language is inappropriate. If you do not change this , measures are being taken by the automated system of Habbo.");
                }
                else if (Session.GetHabbo().BobbaFiltered >= 3)
                {
                    if (Session.GetHabbo().BobbaFiltered == 3)
                    {
                        Session.GetHabbo().BobbaFiltered = 4;
                        Azure.MutedUsersByFilter.Add(Session.GetHabbo().Id, uint.Parse((Azure.GetUnixTimeStamp() + (300*60)).ToString()));

                        return false;
                    }
                    if (Session.GetHabbo().BobbaFiltered == 4)
                    {
                        Session.SendNotif("Now you can not talk for 5 minutes . This is because your exhibits inappropriate language in Habbo Hotel.");
                    }
                    else if (Session.GetHabbo().BobbaFiltered == 5)
                    {
                        Session.SendNotif("You risk a ban if you continue to scold it . This is your last warning");
                    }
                    else if (Session.GetHabbo().BobbaFiltered >= 7)
                    {
                        Session.GetHabbo().BobbaFiltered = 0;

                        int Length = 3600;
                        Azure.GetGame().GetBanManager().BanUser(Session, "Auto-system-ban", Length, "ban.", false, false);
                    }
                }
            }

            if (Azure.MutedUsersByFilter.ContainsKey(Session.GetHabbo().Id))
            {
                if (Azure.MutedUsersByFilter[Session.GetHabbo().Id] < Azure.GetUnixTimeStamp())
                {
                    Azure.MutedUsersByFilter.Remove(Session.GetHabbo().Id);
                }
                else
                {
                    DateTime Now = DateTime.Now;
                    TimeSpan TimeStillBanned = Now - Azure.UnixToDateTime(Azure.MutedUsersByFilter[Session.GetHabbo().Id]);

                    Session.SendNotif("Damn! you can't talk for " + TimeStillBanned.Minutes.ToString().Replace("-", "") + " minutes and " + TimeStillBanned.Seconds.ToString().Replace("-", "") + " seconds.");
                    return false;
                }
            }

            return true;
        }

        internal static void InitSwearWord()
        {
            Word = new List<string>();
            Word.Clear();
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("SELECT `word` FROM wordfilter");
                var table = adapter.GetTable();
                if (table == null) return;

                foreach (DataRow row in table.Rows)
                {
                    Word.Add(row[0].ToString().ToLower());
                }
            }

            Out.WriteLine("Loaded " + Word.Count + " Bobba Filters", "Azure.Security.BobbaFilter");
            Console.WriteLine();

        }


        internal static bool CheckForBannedPhrases(string Message)
        {
            Message = Message.ToLower();

            Message = Message.Replace(".", "");
            Message = Message.Replace(" ", "");
            Message = Message.Replace("-", "");
            Message = Message.Replace(",", "");

            foreach (string mWord in Word)
            {
                if (Message.Contains(mWord.ToLower()))
                    return true;
            }

            return false;
        }

        private static string ReplaceEx(string original,
                    string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern, position0, StringComparison.Ordinal)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }



    }
}

