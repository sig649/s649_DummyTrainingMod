using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BepInEx;
using HarmonyLib;

using UnityEngine;
using BepInEx.Configuration;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace s649DPM
{
    namespace PatchMain
    {
        [BepInPlugin("s649_DummyPracticeMod", "Dummy Practice Mod", "0.2.0.0")]

        public class Main : BaseUnityPlugin
        {
            //entry---------------------------------------------------
            private static ConfigEntry<int> CE_LogLevel;//デバッグ用のログの出力LV　-1:出力しない 0~:第二引数に応じて出力
            public static int cf_LogLevel =>  CE_LogLevel.Value;
            
            private static ConfigEntry<bool> CE_AllowFunction00;//
            public static bool cf_Allow_F00 =>  CE_AllowFunction00.Value;
            //loading-------------------------------------------------
            private void Start()
            {
                CE_LogLevel = Config.Bind("#zz-Debug","LogLevel", 0, "For debug use. If the value is -1, it won't output logs");
                CE_AllowFunction00 = Config.Bind("#general","Mod_Enable", true, "Enable Mod function");

                //var harmony = new Harmony("Main");
                new Harmony("Main").PatchAll();
            }
            internal static void Lg(string text, int lv = 0)
            {
                if(cf_LogLevel >= lv){Debug.Log(text);}
            }
            
        }
        //++++EXE++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        [HarmonyPatch]
        public class PatchExe
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(StatsStamina), "Mod")]
            public static bool Prefix(StatsStamina __instance, ref int a)
            {
                if (!Main.cf_Allow_F00) { return true; }//exclusive
                Chara CC = BaseStats.CC;
                if (!CC.IsPC || !(CC.ai is AI_PracticeDummy || CC.ai is AI_Torture) || !(a < 0)) { return true; }
                
                //bool eval = false;
                string dt = "[s649-DPM]CC:" + CC.NameSimple + "/ai:" + CC.ai.ToString() + "/sleep:" + CC.sleepiness.GetValue().ToString() + "/hunger:" + CC.hunger.GetValue().ToString();
                int sleepiness = CC.sleepiness.GetValue();
                int hunger = CC.hunger.GetValue();
                if (sleepiness < CC.sleepiness.max)
                {
                    if (hunger < CC.hunger.max / 2)
                    {
                        int seed = (hunger < CC.hunger.max / 4) ? 5 : 10;
                        if (EClass.rnd(seed) == 0)
                        { 
                            CC.hunger.Mod(1);
                            dt += "/hunger:Plus";
                            Main.Lg(dt);
                            return false;
                        }
                        //if (eval) { return false; }
                        
                    }
                    if(EClass.rnd(Lower(CC.LV + 100, 200)) > 100)//LVが高ければ眠気増加回避※上限100LV MAX 50%
                    {
                        //eval = true;
                        dt += "/Sleepiness:Eval";
                        Main.Lg(dt);
                        return false; 
                    }
                    int seed2 = (sleepiness > CC.sleepiness.max / 2) ? 2 : 4;
                    if (EClass.rnd(seed2) != 0) 
                    {
                        CC.sleepiness.Mod(1);
                        dt += "/Sleepiness:Add";
                        Main.Lg(dt);
                        return false;
                    }
                }
                dt += "/Vanilla:StaminaDown";
                Main.Lg(dt);
                return true;
            }

            internal static int Lower(int a, int b)
            {
                return a < b ? a : b;
            }

        }
    }
    
}
//------------template--------------------------------------------------------------------------------------------
/*
[HarmonyPatch]

[HarmonyPrefix]
[HarmonyPostfix]

[HarmonyPatch(typeof(----),"method")]
public class ------{}

public static void ----(type arg){}
public static bool Pre--(type arg){}

[HarmonyPatch]
public class PreExe{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(------), "+++++++")]
    public static bool Prefix(type arg){}
}

[HarmonyPatch]
public class PostExe{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(------), "+++++++")]
    public static void Postfix(type arg){}
}

*/

//////trash box//////////////////////////////////////////////////////////////////////////////////////////////////
///
/*

[HarmonyPrefix]
            [HarmonyPatch(typeof(AI_PracticeDummy), "CreateProgress")]
            internal static bool Prefix(AI_PracticeDummy __instance, AIProgress __result)
            {
                Progress_Custom progress_Custom = new Progress_Custom();
		        progress_Custom.canProgress = (() => !__instance.isFail());
		        progress_Custom.onProgressBegin = delegate()
		        {
		        };
		        progress_Custom.onProgress = delegate(Progress_Custom p)
		        {
			        if (p.progress % 10 == 0)
			        {
				    __instance.target.animeCounter = 0.01f;
			        }
			        if (__instance.throwItem != null)
			        {
				        if (!ActThrow.CanThrow(EClass.pc, __instance.throwItem, __instance.target, null))
				        {
					    p.Cancel();
					    return;
				        }
				        ActThrow.Throw(EClass.pc, __instance.target.pos, __instance.target, __instance.throwItem, ThrowMethod.Default);
			        }
			        else if (__instance.range && __instance.owner.GetCondition<ConReload>() == null)
			        {
				    if (!ACT.Ranged.CanPerform(__instance.owner, __instance.target, __instance.target.pos))
				        {
					        p.Cancel();
					        return;
				        }
				        if (!ACT.Ranged.Perform(__instance.owner, __instance.target, __instance.target.pos))
				        {
					    p.Cancel();
				        }
			        }
        			else
        			{
        				ACT.Melee.Perform(__instance.owner, __instance.target, null);
        			}
        			__instance.turn += 1L;
        			if (__instance.owner != null && EClass.rnd(5) < 2)
        			{
                        __instance.owner.sleepiness.Mod(1);
                        Lg("[s649-DPM]Sleep.add");
        				//__instance.owner.stamina.Mod(-1);
        			}
		        	if (__instance.owner != null && __instance.owner.stamina.value < 0)
        			{
        				p.Cancel();
        			}
        		};
        		progress_Custom.onProgressComplete = delegate()
        		{
        		};
        		__result = progress_Custom.SetDuration(10000, 2);

                return false;
            }
            internal static void Lg(string text, int lv = 0)
            {
                if(Main.cf_LogLevel >= lv){Debug.Log(text);}
            }
    [HarmonyPatch(typeof(Zone))]
    [HarmonyPatch(nameof(Zone.Activate))]
    public class HarmonyAct
    {
        //[HarmonyPostfix]
        
        static void Postfix(Zone __instance)
        {
            Zone z = __instance;
            Zone topZo = z.GetTopZone();
            FactionBranch br = __instance.branch;
            //Lg("[LS]Fooked!");
            if (Main.propFlagEnablelLogging)
            {
                Lg("[LS]CALLED : Zone.Activate ");
                string text;

                text = ("[LS]Ref : [Z:" + z.id.ToString() + "]");
                //text += (" [id:" + z.id.ToString() + "]");
                text += (" [Dlv:" + z.DangerLv.ToString() + "]");
                text += (" [blv:" + Mathf.Abs(z.lv).ToString() + "]");
                text += (" [bDLV:" + z._dangerLv.ToString() + "]");
                text += (" [Dlfi:" + z.DangerLvFix.ToString() + "]");
                if(topZo != null && z != topZo){text += (" [tpZ:" + topZo.NameWithLevel + "]");}
                if(br != null){text += (" [br:" + br.ToString() + "]");}
                if(z.ParentZone != null && z != z.ParentZone)text += (" [PaZ: " + z.ParentZone.id.ToString() + "]") ;
                 text += (" [Pce:" + z.isPeace.ToString() + "]");
                 text += (" [Twn:" + z.IsTown.ToString() + "]");
                Lg(text);
                //text = ("[LS]Charas : " + EClass._map.charas.Count);
                //text += (" [Stsn:" + z.isPeace.ToString() + "]");
            }
            
        }
        public static void Lg(string t)
        {
            UnityEngine.Debug.Log(t);
        }
        
    }

    [HarmonyPatch(typeof(HotItem))]
    public class HotPatch {
        //[HarmonyPrefix]
        [HarmonyPostfix]
        [HarmonyPatch(nameof(HotItem.TrySetAct))]
        static void FookPostExe(HotItem __instance){
            //Debug.Log("[LS]Fooking->" + __instance.ToString());
        }
    }  

    [HarmonyPatch]
    public class TickPatch{
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Chara),"TickConditions")]
        public static void TickExe(Chara __instance){
            Chara c = __instance;
            if(c.IsPC){
                //Debug.Log("[LS]QuestMain : " + QuestMain.Phase.ToString());
            }
        }
    }
    */
/*
    [HarmonyPatch(typeof(TraitDoor))]
    public class PatchAct2 {

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TraitDoor.TryOpen))]
        static void FookPreExe(TraitDoor __instance, Chara c, ref bool __state){
            __state = __instance.IsOpen() ? true : false;
            //if(c.IsPC){ Lg("[LS]TraitDoor.TryOpen Called! by->" + c.ToString());}
            
            
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TraitDoor.TryOpen))]
        static void FookPostExe(TraitDoor __instance, Chara c, bool __state){
            if(!__state && __instance.IsOpen()){
                if(c.IsPC){ 
                    //Lg("[LS]TraitDoor.Close->Open!" + c.ToString());
                    }
            }
           
        }
        
        
        public static void Lg(string t)
        {
            UnityEngine.Debug.Log(t);
        }
    }
   
public class Main : BaseUnityPlugin {
    private void Start() {
        var harmony = new Harmony("NerunTest");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(Zone))]
[HarmonyPatch(nameof(Zone.Activate))]
class ZonePatch {
    static void Prefix() {
        Debug.Log("Harmoney Prefix");
    }
    static void Postfix(Zone __instance) {
        Debug.Log("Harmoney Postfix");
        }
}
*/

/*
        
        */
        /*
        [HarmonyPatch(typeof(Zone))]
        [HarmonyPatch(nameof(Zone.Activate))]
        class ZonePatch {
        static void Postfix(Zone __instance) {
            Lg("[LS]CALLED : Zone.Activate " + __instance.ToString());
            Lg("[LS]Zone : [DLV : " + __instance.DangerLv.ToString() + "]");
            Lg("[LS]Player : [dst : " + EClass.player.stats.deepest.ToString() + "]");
            }
        }
        */

        /*
        static void PostMoveZone(Player __instance)
        {
            Lg("[LS]Fooked!MoveZone!");
            if (Main.propFlagEnablelLogging.Value)
            {
                int dst = EClass.player.stats.deepest;
                Lg("[LS]CALLED : Player.MoveZone ");
                Lg("[LS]Player : [dst : " + dst.ToString() + "]");
            }
        }

        */


        /*
        [HarmonyPatch(typeof(Card), "AddExp")]
        
        class CardPatch
        {
            [HarmonyPrefix]
            static bool AddExpHook(Card __instance)
            {
                Lg("[LS]Fooked:AddExp!");
                
                if (Main.propFlagEnablelLogging.Value)
                {
                    if(__instance.IsPC){
                        Lg("[LS]Card : [name : " + __instance.ToString() + "]");
                    }
                    //Lg("[LS]Card : [name : " + dst.ToString() + "]");
                    //Lg("[LS]Player : [dst : " + dst.ToString() + "]");
                }
                return true;
            }
        }
        */

        /*
        [HarmonyPatch(typeof(Zone), "DangerLv", MethodType.Getter)]
        class ZonePatch {
            [HarmonyPrefix]
            static bool Prefix(Zone __instance) {
                Lg("[LS]CALLED : Zone.DangerLV ");
                //Lg("[LS]Zone : [Z.toSt : " + __instance.ToString() + "]");
                //Lg("[LS]Zone : [DLV : " + __instance.DangerLv.ToString() + "]");
                //Lg("[LS]Player : [dst : " + EClass.player.stats.deepest.ToString() + "]");
                return true;
            }
        }
        */


/*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Map), "MineFloor")]
        public static void Postfix(Map __instance, Point point, Chara c, bool recoverBlock, bool removePlatform){
            string text = "[LS]MF [";
            text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "C:" + c.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "rP:" + removePlatform.ToString() + "][";
            text += "]";
            //Debug.Log(text);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Map), "DropBlockComponent")]
        public static void Postfix(Point point,TileRow r,SourceMaterial.Row mat, bool recoverBlock, bool isPlatform, Chara c){
            string text = "[LS]DBC [";
            //text += "Map:" + __instance.ToString() + "][";
            text += "P:" + point.ToString() + "][";
            text += "r:" + r.ToString() + "][";
            text += "rid:" + r.id.ToString() + "][";
            text += "mat:" + mat.ToString() + "][";
            text += "rB:" + recoverBlock.ToString() + "][";
            text += "iP:" + isPlatform.ToString() + "][";
            //text += "c:" + c.ToString() + "][";
            text += "]";
            Debug.Log(text);
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ThingGen), "CreateRawMaterial")]
        public static void Postfix(SourceMaterial.Row row){
            Debug.Log("[LS]TG->CRM : " + row.ToString());
        }*/