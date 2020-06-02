using HarmonyLib;
using Planetbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tahvohck_Mods.JPFariasUpdates
{
    using Module = Planetbase.Module;
    using ModuleReflect = System.Reflection.Module;

    public class AutoConnections
    {
        public static void Init()
        {
            new Harmony(typeof(AutoConnections).FullName).PatchAll();
        }
    }


    /// <summary>
    /// Because we need an object that is disposed of during the original call, we have to grab that object
    /// during a prefix, then operate on it during a postfix. This works without any noticeable issues.
    /// </summary>
    [HarmonyPatch(typeof(GameStateGame), "placeModule")]
    public class PatchGameState
    {
        private static MethodInfo recycleLinkComponents = typeof(Module)
            .GetMethod("recycleLinkComponents", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Prefix(GameStateGame __instance, out State __state)
        {
            Module mActiveModule = __instance.GetType()
                .GetField("mActiveModule", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(__instance) as Module;
            bool? mRenderTops = __instance.GetType()
                .GetField("mRenderTops", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(__instance) as bool?;


            if (mActiveModule.isValidPosition()) {
                __state = new State(mActiveModule, (bool)mRenderTops);
            } else {
                __state = null;
            }
        }

        public static void Postfix(State __state)
        {
            // Don't run if module position was invalid
            if (__state is null) { return; }

            var linkable = ModuleHelper.GetAllModules((module) =>
                module != __state.ActiveModule
                && Connection.canLink(module, __state.ActiveModule)
            );

            linkable.ForEach(delegate (Module module) {
                Connection c = Connection.create(__state.ActiveModule, module);
                c.onUserPlaced();
                c.setRenderTop(__state.RenderTops);

                recycleLinkComponents.Invoke(__state.ActiveModule, null);
                recycleLinkComponents.Invoke(module, null);
            });
        }


        /// <summary>
        /// Internal state holder
        /// </summary>
        public class State
        {
            public readonly Module ActiveModule;
            public readonly bool RenderTops;

            public State(Module m, bool renderTops)
            {
                ActiveModule = m;
                RenderTops = renderTops;
            }
        }
    }
}
