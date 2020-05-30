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


    [HarmonyPatch(typeof(GameStateGame), "placeModule")]
    public class PatchGameState
    {
        public static void Prefix(GameStateGame __instance, out object[] __state)
        {
            if (__instance.mActiveModule.isValidPosition()) {
                __state = new object[2];
                __state[0] = __instance.mActiveModule;
                __state[1] = __instance.mRenderTops;
            } else {
                __state = null;
            }
        }

        public static void Postfix(object[] __state)
        {
            // Don't run if instance is null
            if (__state is null) { return; }

            Module currentModule = __state[0] as Module;
            bool renderTops = (bool)__state[1];
            TahvUtil.Log($"Running connections on a {currentModule.getName()}");

            List<Module> linkable = Module.mModules.FindAll((module) =>
                !(module is null)
                && module != currentModule
                && Connection.canLink(
                    currentModule, module,
                    currentModule.getPosition(), module.getPosition())
            );

            linkable.ForEach(delegate (Module module) {
                Connection c = Connection.create(currentModule, module);
                c.onUserPlaced();
                c.setRenderTop(renderTops);
                currentModule.recycleLinkComponents();
                module.recycleLinkComponents();
            });
        }
    }
}
