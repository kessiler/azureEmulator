﻿using System.Threading;
using Azure.HabboHotel.Commands.Interfaces;
using Azure.HabboHotel.GameClients.Interfaces;
using Azure.Messages;
using Azure.Messages.Parsers;

namespace Azure.HabboHotel.Commands.Controllers
{
    /// <summary>
    ///     Class HotelAlert. This class cannot be inherited.
    /// </summary>
    internal sealed class EventAlert : Command
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventAlert" /> class.
        /// </summary>
        public EventAlert()
        {
            MinRank = 5;
            Description = "Alerts to all hotel a event.";
            Usage = ":eventha";
            MinParams = -1;
        }

        public override bool Execute(GameClient session, string[] pms)
        {
            var message = new ServerMessage(LibraryParser.OutgoingRequest("SuperNotificationMessageComposer"));
            message.AppendString("events");
            message.AppendInteger(4);
            message.AppendString("title");
            message.AppendString("Temos um novo Evento");
            message.AppendString("message");
            message.AppendString(
                "Tem um novo evento acontecendo agora mesmo!\n\nO evento está sendo feito por:    <b>" +
                session.GetHabbo().UserName +
                "</b>\n\nCorra para participar antes que o quarto seja fechado! Clique em " +
                "<i>Ir para o Evento</i>\n\nE o " +
                "evento vai ser:\n\n<b>" + string.Join(" ", pms) + "</b>\n\nEstamos esperando você lá em!");
            message.AppendString("linkUrl");
            message.AppendString("event:navigator/goto/" + session.GetHabbo().CurrentRoomId);
            message.AppendString("linkTitle");
            message.AppendString("Ir para o Evento");

            /*foreach (var client in Azure.GetGame().GetClientManager().Clients.Values)
            {
                if (client == null)
                    continue;
 
                if (session.GetHabbo().Id == client.GetHabbo().Id)
                {
                    client.SendWhisper("O Alerta de Evento foi Enviado com Sucesso", true);
                    continue;
                }
 
                if (client.GetHabbo().DisableEventAlert == false)
                    client.SendMessage(message);
 
                //Thread.Sleep(10);
            }*/

            Azure.GetGame().GetClientManager().QueueBroadcaseMessage(message);
            return true;
        }
    }
}