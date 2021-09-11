using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modding;
using UnityEngine;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ModCommon;
using ModCommon.Util;

namespace UnnSnailMod
{
    public class UnnSnail : Mod
    {

        class HeroScript : MonoBehaviour
        {
            public UnnSnail UnnSnail = null;
            bool c = false;
            void FixedUpdate()
            {
                if (!c)
                {
                    try
                    {
                        PlayMakerFSM pm = FindObjectsOfType<PlayMakerFSM>().FirstOrDefault(x => x.FsmName == "UI Charms");
                        pm.InsertMethod("Black Charm? 2", 0, () =>
                        {
                            if (pm.FsmVariables.FindFsmInt("Current Item Number").Value == 28 && Test)
                            {
                                pm.SendEvent("CANCEL");
                            }
                        });
                        pm.InsertMethod("Black Charm?", 0, () =>
                        {
                            if (pm.FsmVariables.FindFsmInt("Current Item Number").Value == 28 && Test)
                            {
                                pm.SendEvent("CANCEL");
                            }
                        });
                        c = true;
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        GameObject unnAnim = null;
        public static string idle = "Slug Idle";
        public static string burst = "Slug Burst";
        public static string turn = "Slug Turn";
        public static string walk = "Slug Walk";

        public static bool Test = true;
        public override void Initialize()
        {
            ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
            ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;
            ModHooks.HeroUpdateHook += ModHooks_HeroUpdateHook;
            ModHooks.AfterSavegameLoadHook += ModHooks_AfterSavegameLoadHook;
        }

        private void ModHooks_AfterSavegameLoadHook(SaveGameData obj)
        {
            if (Test)
            {
                if (!obj.playerData.equippedCharms.Contains(28)) obj.playerData.equippedCharms.Add(28);
                obj.playerData.equippedCharm_28 = true;
            }
        }

        private void ModHooks_HeroUpdateHook()
        {
            HeroController.instance.gameObject.GetOrAddComponent<HeroScript>().UnnSnail = this;
        }

        private string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (key == "CHARM_DESC_28")
            {
                if (Test) {
                    return "包含了乌恩的力量\n\n这个护符是持有者的一部分，不能卸下";
                }
                return "包含了乌恩的力量";
            }
            return orig;
        }

        private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller)
        {
            if (unnAnim == null)
            {
                unnAnim = new GameObject();
                unnAnim.transform.parent = controller.transform;
                unnAnim.transform.localPosition = Vector3.zero;

                unnAnim.AddComponent<UnnScript>();
                unnAnim.AddComponent<MeshRenderer>();
                unnAnim.AddComponent<tk2dSprite>().SetSprite(controller.GetComponent<tk2dSprite>().Collection, 0);
                tk2dSpriteAnimator anim = unnAnim.AddComponent<tk2dSpriteAnimator>();
                anim.Library = controller.GetComponent<tk2dSpriteAnimator>().Library;
                anim.Play("Slug Idle");

                unnAnim.SetActive(false);
            }
            if (data.equippedCharm_5)
            {
                if (data.equippedCharm_17)
                {
                    idle = "Slug Idle BS";
                    burst = "Slug Burst BS";
                    walk = "Slug Walk BS";
                    turn = "Slug Turn BS";
                }
                else
                {
                    idle = "Slug Idle B";
                    burst = "Slug Burst B";
                    walk = "Slug Walk B";
                    turn = "Slug Turn B";
                }
            }else if (data.equippedCharm_17)
            {
                idle = "Slug Idle S";
                burst = "Slug Burst S";
                walk = "Slug Walk S";
                turn = "Slug Turn S";
            }
            else
            {
                idle = "Slug Idle";
                burst = "Slug Burst";
                walk = "Slug Walk";
                turn = "Slug Turn";
            }
            if (data.equippedCharm_28)
            {
                unnAnim.SetActive(true);
            }
            else
            {
                unnAnim.SetActive(false);
            }
        }
    }
}
