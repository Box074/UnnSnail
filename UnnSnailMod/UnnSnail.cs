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

namespace UnnSnailMod
{
    public class UnnSnail : Mod
    {
        GameObject unnAnim = null;
        public static string idle = "Slug Idle";
        public static string burst = "Slug Burst";
        public static string turn = "Slug Turn";
        public static string walk = "Slug Walk";
        public override void Initialize()
        {
            ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
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
