using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using PiPiPlugin.PluginModule;
using PixelPerfectEx.IPC;
using System.Collections.Concurrent;
using System.Security;

namespace PixelPerfectEx
{
    internal class Service
    {
        /// <summary>
        /// Gets or sets the plugin itself.
        /// </summary>
        internal static Plugin Plugin { get; set; } = null!;

        /// <summary>
        /// Gets or sets the plugin configuration.
        /// </summary>
        internal static Configuration Configuration { get; set; } = null!;

        /// <summary>
        /// Gets or sets the plugin address resolver.
        /// </summary>
        internal static PluginAddressResolver Address { get; set; } = null!;

        /// <summary>
        /// Gets the Dalamud plugin interface.
        /// </summary>
        [PluginService]
        internal static DalamudPluginInterface Interface { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud chat gui.
        /// </summary>
        [PluginService]
        internal static ChatGui ChatGui { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud client state.
        /// </summary>
        [PluginService]
        internal static ClientState ClientState { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud client Condition.
        /// </summary>
        [PluginService]
        internal static Condition Condition { get; private set; } = null!;


        /// <summary>
        /// Gets the FF Game Objects.
        /// </summary>
        [PluginService]
        internal static ObjectTable GameObjects { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud command manager.
        /// </summary>
        [PluginService]
        internal static CommandManager CommandManager { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud data manager.
        /// </summary>
        [PluginService]
        internal static DataManager DataManager { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud framework.
        /// </summary>
        [PluginService]
        internal static Framework Framework { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud game gui.
        /// </summary>
        [PluginService]
        internal static GameGui GameGui { get; private set; } = null!;

        /// <summary>
        /// Gets the Dalamud signature scanner.
        /// </summary>
        [PluginService]
        internal static SigScanner Scanner { get; private set; } = null!;

        [PluginService]
        internal static PartyList PartieList { get; private set; } = null!;
        [PluginService]
        internal static Dalamud.Game.Network.GameNetwork GameNetwork { get; private set; } = null!;
        [PluginService]
        internal static KeyState KeyState { get; private set; } = null!;

        internal static LogSender LogSender { get; private set; } = new();

        internal static HashSet<Drawing.IDrawData> DrawDatas = new();
    }
}
