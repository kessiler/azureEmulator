#region

using System;
using System.Data;

#endregion

namespace Azure.HabboHotel.Pets
{
    /// <summary>
    /// Class MoplaBreed.
    /// </summary>
    internal class MoplaBreed
    {
        /// <summary>
        /// The growing status
        /// </summary>
        internal int GrowingStatus;

        /// <summary>
        /// The live state
        /// </summary>
        internal MoplaState LiveState;

        /// <summary>
        /// The _pet
        /// </summary>
        private readonly Pet _pet;

        /// <summary>
        /// The _pet identifier
        /// </summary>
        private readonly uint _petId;

        /// <summary>
        /// The _rarity
        /// </summary>
        private readonly int _rarity;

        /// <summary>
        /// The _DB update needed
        /// </summary>
        private bool _dbUpdateNeeded;

        /// <summary>
        /// Initializes a new instance of the <see cref="MoplaBreed"/> class.
        /// </summary>
        /// <param name="row">The row.</param>
        internal MoplaBreed(DataRow row)
        {
            _petId = uint.Parse(row["pet_id"].ToString());
            _rarity = int.Parse(row["rarity"].ToString());
            Name = row["plant_name"].ToString();
            PlantData = row["plant_data"].ToString();
            LiveState = (MoplaState)int.Parse(row["plant_state"].ToString());
            GrowingStatus = int.Parse(row["growing_status"].ToString());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoplaBreed"/> class.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <param name="petId">The pet identifier.</param>
        /// <param name="rarity">The rarity.</param>
        /// <param name="moplaName">Name of the mopla.</param>
        /// <param name="breedData">The breed data.</param>
        /// <param name="liveState">State of the live.</param>
        /// <param name="growingStatus">The growing status.</param>
        internal MoplaBreed(Pet pet, uint petId, int rarity, string moplaName, string breedData, int liveState,
            int growingStatus)
        {
            _pet = pet;
            _petId = petId;
            _rarity = rarity;
            Name = moplaName;
            PlantData = breedData;
            LiveState = (MoplaState)liveState;
            GrowingStatus = growingStatus;
        }

        /// <summary>
        /// Gets the grow status.
        /// </summary>
        /// <value>The grow status.</value>
        internal string GrowStatus
        {
            get
            {
                if (LiveState == MoplaState.Dead)
                    return "rip";
                return LiveState == MoplaState.Grown ? "std" : string.Format("grw{0}", GrowingStatus);
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        internal string Name { get; private set; }

        /// <summary>
        /// Gets the plant data.
        /// </summary>
        /// <value>The plant data.</value>
        internal string PlantData { get; private set; }

        /// <summary>
        /// Creates the monsterplant breed.
        /// </summary>
        /// <param name="pet">The pet.</param>
        /// <returns>MoplaBreed.</returns>
        internal static MoplaBreed CreateMonsterplantBreed(Pet pet)
        {
            if (pet.Type != 16)
                return null;
            var tuple = GeneratePlantData(pet.Rarity);
            var breed = new MoplaBreed(pet, pet.PetId, pet.Rarity, tuple.Item1, tuple.Item2, 0, 1);
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery(
                    "INSERT INTO pets_plants (pet_id, rarity, plant_name, plant_data) VALUES (@petid , @rarity , @plantname , @plantdata)");
                adapter.AddParameter("petid", pet.PetId);
                adapter.AddParameter("rarity", pet.Rarity);
                adapter.AddParameter("plantname", tuple.Item1);
                adapter.AddParameter("plantdata", tuple.Item2);
                adapter.RunQuery();
            }
            return breed;
        }

        /// <summary>
        /// Generates the plant data.
        /// </summary>
        /// <param name="rarity">The rarity.</param>
        /// <returns>Tuple&lt;System.String, System.String&gt;.</returns>
        internal static Tuple<string, string> GeneratePlantData(int rarity)
        {
            var str = string.Empty;
            int num;
            int num2;
            var random = new Random();
            switch (rarity)
            {
                case 1:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 2) == 0)
                        {
                            num = 0;
                            str = string.Format("{0}Aenueus ", str);
                        }
                        else
                        {
                            num = 3;
                            str = string.Format("{0}Viridulus ", str);
                        }
                    else
                    {
                        num = 9;
                        str = string.Format("{0}Fulvus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 1;
                        str = string.Format("{0}Blungon", str);
                    }
                    else
                    {
                        num2 = 3;
                        str = string.Format("{0}Stumpy", str);
                    }
                    break;

                case 2:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 2) == 0)
                        {
                            num = 5;
                            str = string.Format("{0}Incarnatus ", str);
                        }
                        else
                        {
                            num = 2;
                            str = string.Format("{0}Phoenicus ", str);
                        }
                    else
                    {
                        num = 1;
                        str = string.Format("{0}Griseus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 3;
                        str = string.Format("{0}Stumpy", str);
                    }
                    else
                    {
                        num2 = 2;
                        str = string.Format("{0}Wailzor", str);
                    }
                    break;

                case 3:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 1;
                            str = string.Format("{0}Griseus ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 10;
                            str = string.Format("{0}Cinereus ", str);
                        }
                        else
                        {
                            num = 8;
                            str = string.Format("{0}Amethyst ", str);
                        }
                    else
                    {
                        num = 2;
                        str = string.Format("{0}Phoenicus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 2;
                        str = string.Format("{0}Wailzor", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 0)
                    {
                        num2 = 6;
                        str = string.Format("{0}Shroomer", str);
                    }
                    else
                    {
                        num2 = 9;
                        str = string.Format("{0}Weggytum", str);
                    }
                    break;

                case 4:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 8;
                            str = string.Format("{0}Amethyst ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 6;
                            str = string.Format("{0}Azureus ", str);
                        }
                        else if (random.Next(0, 5) == 4)
                        {
                            num = 10;
                            str = string.Format("{0}Cinereus ", str);
                        }
                        else if (random.Next(0, 7) % 2 != 0)
                        {
                            num = 8;
                            str = string.Format("{0}Amethyst ", str);
                        }
                        else
                        {
                            num = 7;
                            str = string.Format("{0}Amatasc ", str);
                        }
                    else
                    {
                        num = 5;
                        str = string.Format("{0}Incarnatus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 7;
                        str = string.Format("{0}Zuchinu", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 0)
                    {
                        num2 = 6;
                        str = string.Format("{0}Shroomer", str);
                    }
                    else
                    {
                        num2 = 4;
                        str = string.Format("{0}Sunspike", str);
                    }
                    break;

                case 5:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 4;
                            str = string.Format("{0}Cyaneus ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 6;
                            str = string.Format("{0}Azureus ", str);
                        }
                        else
                        {
                            num = 7;
                            str = string.Format("{0}Amatasc ", str);
                        }
                    else
                    {
                        num = 3;
                        str = string.Format("{0}Viridulus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 7;
                        str = string.Format("{0}Zuchinu", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 11;
                        str = string.Format("{0}Hairbullis", str);
                    }
                    else
                    {
                        num2 = 9;
                        str = string.Format("{0}Weggytum", str);
                    }
                    break;

                case 6:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 8;
                            str = string.Format("{0}Amethyst ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 7;
                            str = string.Format("{0}Atamasc ", str);
                        }
                        else
                        {
                            num = 2;
                            str = string.Format("{0}Phoenicus ", str);
                        }
                    else
                    {
                        num = 6;
                        str = string.Format("{0}Azureus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 10;
                        str = string.Format("{0}Wystique", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 11;
                        str = string.Format("{0}Hairbullis", str);
                    }
                    else
                    {
                        num2 = 3;
                        str = string.Format("{0}Stumpy", str);
                    }
                    break;

                case 7:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 6;
                            str = string.Format("{0}Azureus ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 7;
                            str = string.Format("{0}Atamasc ", str);
                        }
                        else
                        {
                            num = 1;
                            str = string.Format("{0}Griseus ", str);
                        }
                    else
                    {
                        num = 4;
                        str = string.Format("{0}Cyaneus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 2;
                        str = string.Format("{0}Wailzor", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 4;
                        str = string.Format("{0}Sunspike", str);
                    }
                    else if (random.Next(0, 3) == 2)
                    {
                        num2 = 10;
                        str = string.Format("{0}Wystique", str);
                    }
                    else
                    {
                        num2 = 6;
                        str = string.Format("{0}Shroomer", str);
                    }
                    break;

                case 8:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 7;
                            str = string.Format("{0}Atamasc ", str);
                        }
                        else if (random.Next(0, 2) == 0)
                        {
                            num = 10;
                            str = string.Format("{0}Cinereus ", str);
                        }
                        else if ((random.Next(12, 0x13) % 2) == 1)
                        {
                            num = 6;
                            str = string.Format("{0}Azureus ", str);
                        }
                        else
                        {
                            num = 8;
                            str = string.Format("{0}Amethyst ", str);
                        }
                    else
                    {
                        num = 4;
                        str = string.Format("{0}Cyaneus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 11;
                        str = string.Format("{0}Hairbullis", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 10;
                        str = string.Format("{0}Wystique", str);
                    }
                    else if (random.Next(0, 3) == 2)
                    {
                        num2 = 7;
                        str = string.Format("{0}Zuchinu", str);
                    }
                    else
                    {
                        num2 = 6;
                        str = string.Format("{0}Shroomer", str);
                    }
                    break;

                case 9:
                    if ((random.Next(0, 4) % 2) != 0)
                        if (random.Next(0, 7) == 5)
                        {
                            num = 7;
                            str = string.Format("{0}Atamasc ", str);
                        }
                        else
                        {
                            num = 6;
                            str = string.Format("{0}Azureus ", str);
                        }
                    else
                    {
                        num = 4;
                        str = string.Format("{0}Cyaneus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 11;
                        str = string.Format("{0}Hairbullis", str);
                    }
                    else if ((random.Next(0, 5) % 2) == 2)
                    {
                        num2 = 10;
                        str = string.Format("{0}Wystique", str);
                    }
                    else
                    {
                        num2 = 8;
                        str = string.Format("{0}Abysswirl", str);
                    }
                    break;

                case 10:
                    num = 4;
                    str = string.Format("{0}Cyaneus ", str);
                    num2 = 8;
                    str = string.Format("{0}Abysswirl", str);
                    break;

                case 11:
                    num = 4;
                    str = string.Format("{0}Cyaneus ", str);
                    num2 = 12;
                    str = string.Format("{0}Snozzle", str);
                    break;

                default:
                    if ((random.Next(0, 4) % 2) == 0)
                    {
                        num = 9;
                        str = string.Format("{0}Fulvus ", str);
                    }
                    else
                    {
                        num = 0;
                        str = string.Format("{0}Aenueus ", str);
                    }
                    if (random.Next(0, 2) == 1)
                    {
                        num2 = 5;
                        str = string.Format("{0}Squarg", str);
                    }
                    else
                    {
                        num2 = 1;
                        str = string.Format("{0}Blungon", str);
                    }
                    break;
            }
            return new Tuple<string, string>(str,
                string.Concat("16 ", num, " ffffff 2 1 ", num2, " ", num, " 0 -1 7"));
        }

        /// <summary>
        /// Kills the plant.
        /// </summary>
        internal void KillPlant()
        {
            LiveState = MoplaState.Dead;
            _dbUpdateNeeded = true;
        }

        /// <summary>
        /// Called when [timer tick].
        /// </summary>
        /// <param name="lastHealth">The last health.</param>
        /// <param name="untilGrown">The until grown.</param>
        internal void OnTimerTick(DateTime lastHealth, DateTime untilGrown)
        {
            if ((int)LiveState != 0)
                return;
            var span = lastHealth - DateTime.Now;
            if (span.TotalSeconds <= 0)
                KillPlant();
            else if (GrowingStatus != 7)
            {
                var span2 = untilGrown - DateTime.Now;
                if (span2.TotalSeconds <= 10 && GrowingStatus == 6)
                {
                    GrowingStatus = 7;
                    LiveState = MoplaState.Grown;
                    _dbUpdateNeeded = true;
                }
                else if (span2.TotalSeconds <= 24000 && GrowingStatus == 5)
                {
                    GrowingStatus = 6;
                    _dbUpdateNeeded = true;
                }
                else if (span2.TotalSeconds <= 48000 && GrowingStatus == 4)
                {
                    GrowingStatus = 5;
                    _dbUpdateNeeded = true;
                }
                else if (span2.TotalSeconds <= 96000 && GrowingStatus == 3)
                {
                    GrowingStatus = 4;
                    _dbUpdateNeeded = true;
                }
                else if (span2.TotalSeconds <= 110000 && GrowingStatus == 2)
                {
                    GrowingStatus = 3;
                    _dbUpdateNeeded = true;
                }
                else if (span2.TotalSeconds <= 160000 && GrowingStatus == 1)
                {
                    GrowingStatus = 2;
                    _dbUpdateNeeded = true;
                }

                if (Math.Abs(span2.TotalSeconds % 8) < 0)
                    _pet.Energy--;
            }
            if (!_dbUpdateNeeded)
                return;
            UpdateInDb();
        }

        /// <summary>
        /// Revives the plant.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool RevivePlant()
        {
            if (LiveState != MoplaState.Dead)
                return false;
            LiveState = GrowingStatus < 7 ? MoplaState.Alive : MoplaState.Grown;
            _dbUpdateNeeded = true;
            return true;
        }

        internal void UpdateInDb()
        {
            using (var adapter = Azure.GetDatabaseManager().GetQueryReactor())
            {
                adapter.SetQuery("REPLACE INTO pets_plants (pet_id, rarity, plant_name, plant_data, plant_state, growing_status) VALUES (@petid , @rarity , @plantname , @plantdata , @plantstate , @growing)");
                adapter.AddParameter("petid", _petId);
                adapter.AddParameter("rarity", _rarity);
                adapter.AddParameter("plantname", Name);
                adapter.AddParameter("plantdata", PlantData);
                adapter.AddParameter("plantstate", ((int)LiveState).ToString());
                adapter.AddParameter("growing", GrowingStatus);
                adapter.RunQuery();
            }
        }
    }
}