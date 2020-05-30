using HarmonyLib;
using Planetbase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tahvohck_Mods.JPFariasUpdates
{
    public class Mod : IMod, ITahvUtilMod
    {
        public void Init()
        {
            ZZZ_Modhooker.UtilsReadyEvent += Setup;
        }

        public void Update()
        { }

        public void Setup(object caller, EventArgs evArgs)
        {
            new Harmony(typeof(Mod).FullName).PatchAll();
            TahvUtil.Log("Patched.");
        }
    }


    /// <summary>
    /// Because we need an object that is disposed of during the original call, we have to grab that object
    /// during a prefix, then operate on it during a postfix. This works without any noticeable issues.
    /// </summary>
    [HarmonyPatch(typeof(GameStateGame), "placeModule")]
    public class PatchGameState
    {
        public static void Prefix(GameStateGame __instance, out State __state)
        {
            if (__instance.mActiveModule.isValidPosition()) {
                __state = new State(__instance.mActiveModule, __instance.mRenderTops);
            } else {
                __state = null;
            }
        }

        public static void Postfix(State __state)
        {
            // Don't run if module position was invalid
            if (__state is null) { return; }

#if DEBUG
            TahvUtil.Log($"Running connections on a {__state.ActiveModule.getName()}");
#endif
            List<Module> linkable = Module.mModules.FindAll((module) =>
                !(module is null)
                && module != __state.ActiveModule
                && Connection.canLink(
                    __state.ActiveModule, module,
                    __state.ActiveModule.getPosition(), module.getPosition())
            );

            linkable.ForEach(delegate (Module module) {
                Connection c = Connection.create(__state.ActiveModule, module);
                c.onUserPlaced();
                c.setRenderTop(__state.RenderTops);
                __state.ActiveModule.recycleLinkComponents();
                module.recycleLinkComponents();
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
