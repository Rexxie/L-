using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

namespace RexxieAIO
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (ObjectManager.Player.ChampionName.ToLowerInvariant())
            {
                case "ahri":
                    new Plugins.Ahri();
                    Game.PrintChat("RexxieAIO - Ahri : Loaded!");
                    break;

                case "malzahar":
                    new Plugins.Malzahar();
                    Game.PrintChat("RexxieAIO - Malzahar : Loaded!");
                    break;
                default:
                    Game.PrintChat("RexxieAIO - Champion not Supported!");
                    break;
            }
        }
    }
}
