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
using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Threading;
using TerrariaApi.Server;
using Newtonsoft.Json.Linq;
using System.Net;


namespace PlayersOnline
{
    [ApiVersion(1, 17)]
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
            //            ServerApi.Hooks.NetGetData.Register(this, GetData);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += OnLogin;

            Commands.ChatCommands.Add(new Command("PlayersOnline.allow", PlayerList, "PlayersOnline"));
            Commands.ChatCommands.Add(new Command("PlayersOnline.allow", PlayerList, "po"));

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= OnLogin;
            }
            base.Dispose(disposing);
        }
        private static void OnLeave(LeaveEventArgs args)
        {
        }

        private void OnLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs args)
        {
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
                args.Player.SendMessage("Syntax: /PlayersOnline [-help] ", Color.Red);
                args.Player.SendMessage("Flags: ", Color.LightSalmon);
                args.Player.SendMessage("   -help     this information", Color.LightSalmon);
                return;
            }
            foreach (TSPlayer tsPlayer in TShock.Players.Where(p => null != p))
            {
                args.Player.SendMessage(String.Format("   {0} {1} Group:{2} UserAccount:{3}", tsPlayer.Name, tsPlayer.IP, tsPlayer.Group.Name, tsPlayer.UserAccountName ?? "<none>"), Color.LightSalmon);
                playersFound = true;
                var player = new Dictionary<string, object>
				{
					{"nickname", tsPlayer.Name},
					{"index", tsPlayer.Index},
					{"ip", tsPlayer.IP},
					{"username", tsPlayer.UserAccountName ?? ""},
					{"account", tsPlayer.UserID},
					{"group", tsPlayer.Group.Name},
					{"active", tsPlayer.Active},
					{"state", tsPlayer.State},
					{"team", tsPlayer.Team},
				};
            }
            if (!playersFound)
            {
                args.Player.SendMessage("   No players online at this time.", Color.LightSalmon);
            }


        }

        #region application specific commands
        public class PlayerOnlineListArguments : InputArguments
        {
            public string CountLimit
            {
                get { return GetValue("limit"); }
            }

            public string Current
            {
                get { return GetValue("-current"); }
            }
            public string CurrentShort
            {
                get { return GetValue("-c"); }
            }

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

            public string Sort
            {
                get { return GetValue("-sort"); }
            }
            public string SortShort
            {
                get { return GetValue("-s"); }
            }
            public string Author
            {
                get { return GetValue("-author"); }
            }
            public string AuthorShort
            {
                get { return GetValue("-a"); }
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
}
