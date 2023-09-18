using Dalamud.Game;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace PiPiPlugin.PluginModule
{
    
    internal interface IPluginModule
    {
        [RequiresPreviewFeatures]
        internal abstract static bool Enabled { get; }
        [RequiresPreviewFeatures]
        static abstract void Init();
        [RequiresPreviewFeatures]
        static abstract void Dispose();
        [RequiresPreviewFeatures]
        static abstract void DrawSetting();

        public static List<Type> SubTypes
        {
            get
            {
                var sublist = new List<Type>();
                var thisType = typeof(IPluginModule);
                var assembly = thisType.Assembly;
                var assemblyTypes = assembly.GetTypes();
                foreach (var itemType in assemblyTypes)
                {
                    if (itemType.GetInterface(thisType.Name) != null && !itemType.IsAbstract)
                        sublist.Add(itemType);
                }
                return sublist;
            }
        }
        public static void LoadAll()
        {
            var tt = SubTypes;
            try
            {
                SubTypes.ForEach(t =>
                {
                    var a = t.GetMethod("Init", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(null, null);
                });
            }
            catch (Exception ex)
            {
                PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");

            }
        }
        public static void DisposeAll()
        {
            try
            {
                SubTypes.ForEach(t =>
                {
                    t.GetMethod("Dispose", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Invoke(null, null);
                });
            }
            catch (Exception ex)
            {
                PluginLog.Error($"{ex.Message}\n{ex.StackTrace}");

            }
        }
    }
}
