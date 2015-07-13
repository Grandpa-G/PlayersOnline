using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Web.Script.Serialization;

using Terraria;
using TShockAPI;
using System.Threading;
using TerrariaApi.Server;
using System.Net;


namespace PlayersOnline
{
    [ApiVersion(1, 19)]
    public class Players : TerrariaPlugin
    {

        public override string Name
        {
            get { return "PlayersOnline"; }
        }
        public override string Author
        {
            get { return "Granpa-G"; }
        }
        public override string Description
        {
            get { return "Provides a list of all players currently online."; }
        }
        public override Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        public Players(Main game)
            : base(game)
        {
            Order = -1;
        }
        public override void Initialize()
        {
             Commands.ChatCommands.Add(new Command("PlayersOnline.allow", PlayerList, "players"));
            Commands.ChatCommands.Add(new Command("PlayersOnline.allow", PlayerList, "po"));

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
        private void PlayerList(CommandArgs args)
        {
            bool verbose = false;
            bool playersFound = false;
            bool help = false;

            PlayerOnlineListArguments arguments = new PlayerOnlineListArguments(args.Parameters.ToArray());
            if (arguments.Contains("-help"))
                help = true;

            if (help)
            {
                args.Player.SendMessage("Syntax: /playersonline [-help] ", Color.Red);
                args.Player.SendMessage("Flags: ", Color.LightSalmon);
                args.Player.SendMessage("   -help     this information", Color.LightSalmon);
                return;
            }

            List<object> playerList = new List<object>();
            foreach (TSPlayer tsPlayer in TShock.Players.Where(p => null != p))
            {
                Player player = new Player(tsPlayer.Name, tsPlayer.IP, tsPlayer.Group.Name, tsPlayer.UserAccountName);
                playerList.Add(player);
            }

            if (playerList.Count == 0)
                args.Player.SendSuccessMessage("No players online at this time.");
            else
            {
                string plural = "";
                if (playerList.Count > 1)
                    plural = "s";
                args.Player.SendSuccessMessage(playerList.Count + "/" + TShock.Config.MaxSlots + " player" + plural + " online at this time.");
                // Query for ascending sort.
                IEnumerable<Player> playerSort =
                    from Player p in playerList
                    orderby p.Name //"ascending" is default 
                    select p;
                foreach (Player p in playerSort)
                {
                    args.Player.SendInfoMessage(String.Format(" {0} {1} Group:{2} UserAccount:{3}", p.Name, p.IP, p.Group, p.UserAccountName ?? "<none>"));
                    playersFound = true;
                }
                if (!playersFound)
                {
                    args.Player.SendSuccessMessage("No players online at this time.");
                }
            }

        }
    }
    public class Player
    {
        public string Name { get; set; }
        public string IP { get; set; }
        public string Group { get; set; }
        public string UserAccountName { get; set; }

        public Player(string name, string ip, string group, string useraccountname)
        {
            Name = name;
            IP = ip;
            Group = group;
            UserAccountName = useraccountname;
        }

        public Player()
        {
            Name = "";
            IP = "";
            Group = "";
            UserAccountName = "";
        }
    }

    #region application specific commands
    public class PlayerOnlineListArguments : InputArguments
    {
        public string Verbose
        {
            get { return GetValue("-verbose"); }
        }
        public string VerboseShort
        {
            get { return GetValue("-v"); }
        }

        public string Help
        {
            get { return GetValue("-help"); }
        }


        public PlayerOnlineListArguments(string[] args)
            : base(args)
        {
        }

        protected bool GetBoolValue(string key)
        {
            string adjustedKey;
            if (ContainsKey(key, out adjustedKey))
            {
                bool res;
                bool.TryParse(_parsedArguments[adjustedKey], out res);
                return res;
            }
            return false;
        }
    }
    #endregion

}
