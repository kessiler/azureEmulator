#region

using System;
using System.Collections.Generic;
using System.Data;
using Azure.Database.Manager.Database.Session_Details.Interfaces;

#endregion

namespace Azure.HabboHotel.Pets
{
    /// <summary>
    ///     Class PetCommandHandler.
    /// </summary>
    internal class PetCommandHandler
    {
        /// <summary>
        ///     The _table
        /// </summary>
        private static DataTable _table;

        /// <summary>
        ///     The _pet commands
        /// </summary>
        private static Dictionary<string, PetCommand> _petCommands;

        /// <summary>
        ///     Gets the pet commands.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>Dictionary&lt;System.Int16, System.Boolean&gt;.</returns>
        internal static Dictionary<short, bool> GetPetCommands(Pet pet)
        {
            var output = new Dictionary<short, bool>();
            var qLevel = (short) pet.Level;

            switch (pet.Type)
            {
                default:
                    /*
                    case 0: //Dog
                    case 1: //Cat
                    case 2: // croco
                    case 3: // terrier dog
                    case 4: // bear
                    case 5: // pig
                    case 6: // lion
                    case 7: // rhino
                    */
                {
                    output.Add(0, true); // SIÉNTATE sit
                    output.Add(1, true); // DESCANSA free
                    output.Add(13, true); // A CASA
                    output.Add(2, qLevel >= 2); // TÚMBATE lay
                    output.Add(4, qLevel >= 3); // PIDE beg
                    output.Add(3, qLevel >= 4); // VEN AQUÍ comehere
                    output.Add(5, qLevel >= 4); // HAZ EL MUERTO play dead
                    output.Add(43, qLevel >= 5); // COMER
                    output.Add(14, qLevel >= 5); // BEBE
                    output.Add(6, qLevel >= 6); // QUIETO
                    output.Add(17, qLevel >= 6); // FÚTBOL
                    output.Add(8, qLevel >= 8); // LEVANTA
                    output.Add(7, qLevel >= 9); // SÍGUEME
                    output.Add(9, qLevel >= 11); // SALTA
                    output.Add(11, qLevel >= 11); // JUEGA
                    output.Add(12, qLevel >= 12); // CALLA
                    output.Add(10, qLevel >= 12); // HABLA
                    output.Add(15, qLevel >= 16); // IZQUIERDA
                    output.Add(16, qLevel >= 16); // DERECHA
                    output.Add(24, qLevel >= 17); // ADELANTE

                    if (pet.Type == 3 || pet.Type == 4)
                    {
                        output.Add(46, true); //Breed
                    }
                }
                    break;

                case 8: // Spider
                    output.Add(1, true); // DESCANSA
                    output.Add(2, true); // TÚMBATE
                    output.Add(3, qLevel >= 2); // VEN AQUÍ
                    output.Add(17, qLevel >= 3); // FÚTBOL
                    output.Add(6, qLevel >= 4); // QUIETO
                    output.Add(5, qLevel >= 4); // HAZ EL MUERTO
                    output.Add(7, qLevel >= 5); // SÍGUEME
                    output.Add(23, qLevel >= 6); // ENCIENDE TV
                    output.Add(9, qLevel >= 7); // SALTA
                    output.Add(10, qLevel >= 8); // HABLA
                    output.Add(11, qLevel >= 8); // JUEGA
                    output.Add(24, qLevel >= 9); // ADELANTE
                    output.Add(15, qLevel >= 10); // IZQUIERDA
                    output.Add(16, qLevel >= 10); // DERECHA
                    output.Add(13, qLevel >= 12); // A CASA
                    output.Add(14, qLevel >= 13); // BEBE
                    output.Add(19, qLevel >= 14); // BOTA
                    output.Add(20, qLevel >= 14); // ESTATUA
                    output.Add(22, qLevel >= 15); // GIRA
                    output.Add(21, qLevel >= 16); // BAILA
                    break;

                case 16:
                    break;
            }

            return output;
        }

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal static void Init(IQueryAdapter dbClient)
        {
            dbClient.SetQuery("SELECT * FROM pets_commands");
            _table = dbClient.GetTable();
            _petCommands = new Dictionary<string, PetCommand>();
            foreach (DataRow row in _table.Rows)
            {
                _petCommands.Add(row[1].ToString(),
                    new PetCommand(Convert.ToInt32(row[0].ToString()), row[1].ToString()));
            }
        }

        /// <summary>
        ///     Tries the invoke.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>System.Int32.</returns>
        internal static int TryInvoke(string input)
        {
            PetCommand command;
            return _petCommands.TryGetValue(input, out command) ? command.CommandId : 0;
        }
    }
}