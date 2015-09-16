#region

using System;
using Azure.HabboHotel.GameClients;
using Azure.Messages.Parsers;

#endregion

namespace Azure.Messages.Handlers
{
    /// <summary>
    /// Class GameClientMessageHandler.
    /// </summary>
    partial class GameClientMessageHandler
    {
        /// <summary>
        /// Calls the guide.
        /// </summary>
        internal void CallGuide()
        {
            Request.GetBool();
            var userId = Request.GetIntegerFromString();
            var message = Request.GetString();
            var guideManager = Azure.GetGame().GetGuideManager();

            if (guideManager.GuidesCount <= 0)
            {
                var errorTrue = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionError"));
                errorTrue.AppendInteger(0);
                Session.SendMessage(errorTrue);
                return;
            }

            var guide = guideManager.GetRandomGuide();
            var onGuideSessionAttached = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionAttachedMessageComposer"));
            onGuideSessionAttached.AppendBool(false);
            onGuideSessionAttached.AppendInteger(userId);
            onGuideSessionAttached.AppendString(message);
            onGuideSessionAttached.AppendInteger(30);
            Session.SendMessage(onGuideSessionAttached);

            var onGuideSessionAttached2 = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionAttachedMessageComposer"));
            onGuideSessionAttached2.AppendBool(true);
            onGuideSessionAttached2.AppendInteger(userId);
            onGuideSessionAttached2.AppendString(message);
            onGuideSessionAttached2.AppendInteger(15);

            guide.SendMessage(onGuideSessionAttached2);
            guide.GetHabbo().GuideOtherUser = Session;
            Session.GetHabbo().GuideOtherUser = guide;
        }

        /// <summary>
        /// Answers the guide request.
        /// </summary>
        internal void AnswerGuideRequest()
        {
            var state = Request.GetBool();

            if (!state)
                return;

            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionStartedMessageComposer"));
            message.AppendInteger(requester.GetHabbo().Id);
            message.AppendString(requester.GetHabbo().UserName);
            message.AppendString(requester.GetHabbo().Look);
            message.AppendInteger(Session.GetHabbo().Id);
            message.AppendString(Session.GetHabbo().UserName);
            message.AppendString(Session.GetHabbo().Look);
            requester.SendMessage(message);
            Session.SendMessage(message);
        }

        ///TODO: IMPORTANT
        /// <summary>
        /// Cancels the call guide.
        /// </summary>
        internal void CancelCallGuide()
        {
            //Response.Init(3485); ///BUG: IMPORTANT 
            //SendResponse();

            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            Session.SendMessage(message);
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_GuideFeedbackGiver", 1, false);
        }

        /// <summary>
        /// Opens the guide tool.
        /// </summary>
        internal void OpenGuideTool()
        {
            var guideManager = Azure.GetGame().GetGuideManager();
            var onDuty = Request.GetBool();

            Request.GetBool();
            Request.GetBool();
            Request.GetBool();

            if (onDuty)
                guideManager.AddGuide(Session);         
            else
                guideManager.RemoveGuide(Session);
                
            Session.GetHabbo().OnDuty = onDuty;
            Response.Init(LibraryParser.OutgoingRequest("HelperToolConfigurationMessageComposer"));
            Response.AppendBool(onDuty);
            Response.AppendInteger(guideManager.GuidesCount);
            Response.AppendInteger(guideManager.HelpersCount);
            Response.AppendInteger(guideManager.GuardiansCount);
            SendResponse();
        }

        /// <summary>
        /// Invites to room.
        /// </summary>
        internal void InviteToRoom()
        {
            var requester = Session.GetHabbo().GuideOtherUser;
            var room = Session.GetHabbo().CurrentRoom;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionInvitedToGuideRoomMessageComposer"));
            if (room == null)
            {
                message.AppendInteger(0);
                message.AppendString("");
            }
            else
            {
                message.AppendInteger(room.RoomId);
                message.AppendString(room.RoomData.Name);
            }
            requester.SendMessage(message);
            Session.SendMessage(message);
        }

        /// <summary>
        /// Visits the room.
        /// </summary>
        internal void VisitRoom()
        {
            if (Session.GetHabbo().GuideOtherUser == null)
                return;
            var requester = Session.GetHabbo().GuideOtherUser;
            var VisitRoom = new ServerMessage(LibraryParser.OutgoingRequest("RoomForwardMessageComposer"));
            VisitRoom.AppendInteger(requester.GetHabbo().CurrentRoomId);
            Session.SendMessage(VisitRoom);
        }

        /// <summary>
        /// Guides the speak.
        /// </summary>
        internal void GuideSpeak()
        {
            var message = Request.GetString();
            var requester = Session.GetHabbo().GuideOtherUser;
            var messageC = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionMsgMessageComposer"));
            messageC.AppendString(message);
            messageC.AppendInteger(Session.GetHabbo().Id);
            requester.SendMessage(messageC);
            Session.SendMessage(messageC);
        }

        /// <summary>
        /// Closes the guide request.
        /// </summary>
        internal void CloseGuideRequest()
        {
            Request.GetBool();
            var requester = Session.GetHabbo().GuideOtherUser;
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            message.AppendInteger(2);
            requester.SendMessage(message);
            requester.GetHabbo().GuideOtherUser = null;
            Session.GetHabbo().GuideOtherUser = null;
        }

        /// <summary>
        /// Guides the feedback.
        /// </summary>
        internal void GuideFeedback()
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("OnGuideSessionDetachedMessageComposer"));
            Session.SendMessage(message);
            Azure.GetGame().GetAchievementManager().ProgressUserAchievement(Session, "ACH_GuideFeedbackGiver", 1, false);
        }

        /// <summary>
        /// Ambassadors the alert.
        /// </summary>
        internal void AmbassadorAlert()
        {
            if (Session.GetHabbo().Rank < Convert.ToUInt32(Azure.GetDbConfig().DbData["ambassador.minrank"])) return;
            uint userId = Request.GetUInteger();
            GameClient user = Azure.GetGame().GetClientManager().GetClientByUserId(userId);
            if (user == null) return;
            user.SendNotif("${notification.ambassador.alert.warning.message}", "${notification.ambassador.alert.warning.title}");
        }
    }
}