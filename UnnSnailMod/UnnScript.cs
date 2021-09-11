using System;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using GlobalEnums;

namespace UnnSnailMod
{
    public delegate bool Hero_CheckStillTouchingWall(CollisionSide side, bool checkTop = false);
    public class UnnScript : MonoBehaviour
    {
        MethodInfo MInfo_Hero_CheckStillTouchingWall = typeof(HeroController).GetMethod("CheckStillTouchingWall", BindingFlags.NonPublic |
                BindingFlags.Instance);
        Hero_CheckStillTouchingWall CheckStillTouchingWall = null;
        MeshRenderer renderer = null;
        tk2dSpriteAnimator anim = null;
        Rigidbody2D rig = null;
        void OnEnable()
        {
            CheckStillTouchingWall = (Hero_CheckStillTouchingWall)MInfo_Hero_CheckStillTouchingWall
                .CreateDelegate(typeof(Hero_CheckStillTouchingWall),HeroController.instance);

            renderer = GetComponent<MeshRenderer>();
            anim = GetComponent<tk2dSpriteAnimator>();
            rig = HeroController.instance.GetComponent<Rigidbody2D>();
            HeroController.instance.OnTakenDamage += Instance_OnTakenDamage;
            HeroController.instance.OnDeath += Instance_OnDeath;

            On.HeroController.Attack += HeroController_Attack;
            On.HeroController.CanDreamNail += HeroController_CanDreamNail;
            On.HeroController.CanDreamGate += HeroController_CanDreamGate;
            On.HeroController.CanCast += HeroController_CanCast;

            On.tk2dSpriteAnimator.LateUpdate += Tk2dSpriteAnimator_LateUpdate;

            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter += ActivateGameObject_OnEnter;
            On.HutongGames.PlayMaker.Actions.Tk2dWatchAnimationEvents.OnEnter += Tk2dWatchAnimationEvents_OnEnter;
        }

        private void Instance_OnDeath()
        {
            gameObject.SetActive(false);
        }

        MethodInfo H_OnAnimationCompleted = typeof(tk2dSpriteAnimator)
            .GetMethod("OnAnimationCompleted", BindingFlags.NonPublic | BindingFlags.Instance);

        private void Tk2dSpriteAnimator_LateUpdate(On.tk2dSpriteAnimator.orig_LateUpdate orig, tk2dSpriteAnimator self)
        {
            if(self.gameObject == HeroController.instance.gameObject)
            {
                if (!PlayerData.instance.atBench)
                {
                    if (self.CurrentClip.wrapMode == tk2dSpriteAnimationClip.WrapMode.Once && self.Playing)
                    {
                        H_OnAnimationCompleted.Invoke(self, null);
                        self.Stop();
                    }
                }
                else
                {
                    orig(self);
                }
            }
            else
            {
                orig(self);
            }
        }

        private void Tk2dWatchAnimationEvents_OnEnter(On.HutongGames.PlayMaker.Actions.Tk2dWatchAnimationEvents.orig_OnEnter orig,
            HutongGames.PlayMaker.Actions.Tk2dWatchAnimationEvents self)
        {
            if (self.gameObject.GameObject?.Value?.name == "Knight Dream Arrival")
            {
                self.Finish();
                return;
            }
            orig(self);
        }

        private void ActivateGameObject_OnEnter(On.HutongGames.PlayMaker.Actions.ActivateGameObject.orig_OnEnter orig,
            HutongGames.PlayMaker.Actions.ActivateGameObject self)
        {
            if (self.gameObject.GameObject?.Value?.name == "Knight Dream Arrival")
            {
                self.Finish();
                return;
            }
            orig(self);
        }

        private bool HeroController_CanCast(On.HeroController.orig_CanCast orig, HeroController self)
        {
            return false;
        }

        private bool HeroController_CanDreamGate(On.HeroController.orig_CanDreamGate orig, HeroController self)
        {
            return false;
        }

        private bool HeroController_CanDreamNail(On.HeroController.orig_CanDreamNail orig, HeroController self)
        {
            return false;
        }

        private void HeroController_Attack(On.HeroController.orig_Attack orig, HeroController self, AttackDirection attackDir)
        {
            orig(self, AttackDirection.normal);
        }
        private void Instance_OnTakenDamage()
        {
            anim.Play(UnnSnail.burst);
        }

        void Update()
        {
            if (PlayerData.instance.atBench)
            {
                HeroController.instance.EnableRenderer();
                renderer.enabled = false;
                return;
            }
            HeroController.instance.GetComponent<MeshRenderer>().enabled = false;
            HeroController.instance.GetComponent<tk2dSprite>().spriteId = GetComponent<tk2dSprite>().spriteId;
            renderer.enabled = true;

            HeroActions hc = InputHandler.Instance.inputActions;
            if (hc.left.IsPressed)
            {
                Turn(false);
                StartCoroutine(TryWalkOnWall());
            }
            else if (hc.right.IsPressed)
            {
                Turn(true);
                StartCoroutine(TryWalkOnWall());
            }
            else if (!HeroController.instance.touchingWallL && !HeroController.instance.touchingWallR)
            {
                Idle();
            }
            if (HeroController.instance.cState.onGround)
            {
                HeroController.instance.ResetAirMoves();
            }
        }
        IEnumerator TryWalkOnWall()
        {
            if (!TryInvoke("WALK_ON_WALL")) yield break;
            try
            {
                transform.SetRotationZ(-90);
                HeroActions hc = InputHandler.Instance.inputActions;
                HeroController hero = HeroController.instance;

                while (GameManager.instance.IsGameplayScene())
                {
                    if (! hero.touchingWallL && !hero.touchingWallR) break;
                    HeroController.instance.AffectedByGravity(false);
                    
                    transform.localPosition = new Vector3(1.1f, -0.7f, 0);

                    if (hc.up.IsPressed)
                    {
                        if (!anim.IsPlaying(UnnSnail.walk))
                        {
                            anim.Play(UnnSnail.walk);
                        }
                        rig.velocity = new Vector2(0, HeroController.instance.WALK_SPEED);
                        transform.SetRotationZ(-90);
                        transform.SetScaleY(1);
                    }
                    else if (hc.down.IsPressed)
                    {
                        if (!anim.IsPlaying(UnnSnail.walk))
                        {
                            anim.Play(UnnSnail.walk);
                        }
                        rig.velocity = new Vector2(0, -HeroController.instance.WALK_SPEED);
                        transform.SetRotationZ(90);
                        transform.SetScaleY(-1);
                    }
                    else
                    {
                        Idle();
                        rig.velocity = new Vector2(rig.velocity.x, 0);
                    }
                    yield return null;
                }
            }
            finally
            {
                HeroController.instance.AffectedByGravity(true);
                transform.rotation = new Quaternion(0, 0, 0, 0);
                transform.localPosition = Vector3.zero;
                transform.SetScaleY(1);
                EndInvoke("WALK_ON_WALL");
            }
        }
        void Idle()
        {
            if (!anim.IsPlaying(UnnSnail.idle))
            {
                anim.Play(UnnSnail.idle);
            }
        }
        void Turn(bool willTo)
        {
            if (HeroController.instance.cState.facingRight == willTo)
            {
                if (!anim.IsPlaying(UnnSnail.walk))
                {
                    anim.Play(UnnSnail.walk);
                }
            }
            else
            {
                anim.Play(UnnSnail.turn);
            }
            if (willTo)
            {
                HeroController.instance.FaceRight();
            }
            else
            {
                HeroController.instance.FaceLeft();
            }
        }
        void OnDisable()
        {
            invoking.Clear();
            HeroController.instance.GetComponent<tk2dSpriteAnimator>().enabled = true;
            HeroController.instance.EnableRenderer();
            HeroController.instance.OnTakenDamage -= Instance_OnTakenDamage;
            HeroController.instance.OnDeath -= Instance_OnDeath;

            On.HeroController.Attack -= HeroController_Attack;
            On.HeroController.CanDreamNail -= HeroController_CanDreamNail;
            On.HeroController.CanDreamGate -= HeroController_CanDreamGate;
            On.HeroController.CanCast -= HeroController_CanCast;

            On.tk2dSpriteAnimator.LateUpdate -= Tk2dSpriteAnimator_LateUpdate;

            On.HutongGames.PlayMaker.Actions.ActivateGameObject.OnEnter -= ActivateGameObject_OnEnter;
            On.HutongGames.PlayMaker.Actions.Tk2dWatchAnimationEvents.OnEnter -= Tk2dWatchAnimationEvents_OnEnter;
        }

        List<string> invoking = new List<string>();
        bool TryInvoke( string name)
        {
            if (invoking.Any(x => x == name)) return false;
            invoking.Add(name);
            return true;
        }

        void EndInvoke(string name)
        {
            invoking.Remove(name);
        }
    }
}