﻿using UnityEngine;

namespace CharacterSystem_V4
{
    public class Ork : ICharacterActionManager
    {
        public CharacterProperty Property;
        private CharacterRunTimeData RunTimeData;

        public Rigidbody2D MovementBody;
        public Animator CharacterAnimator;

        public AudioSource MoveSound, FallDownSound, LightAttackSound, HurtSound;
        public AttackColliders LightAttackColliders;

        void Start()
        {
            RunTimeData = new CharacterRunTimeData();
            RunTimeData.SetData(Property);

            nowAction = new OrkIdle();
            nowAction.SetManager(this);
        }

        public override void ActionUpdate()
        {
            if (RunTimeData.Health <= 0)
                SetAction(new OrkDead());
            else
            {
                RunTimeData.AttackTimer += Time.deltaTime;

                RunTimeData.RegenTimer += Time.deltaTime;
                if (RunTimeData.Health < Property.MaxHealth &&
                    RunTimeData.RegenTimer >= Property.CharacterRegenSpeed)
                {
                    RunTimeData.Health += Property.CharacterRegenHealth;
                    RunTimeData.RegenTimer = 0;
                }

                RunTimeData.VertigoConter -= Time.deltaTime / 10;
            }

            base.ActionUpdate();
        }

        private class IOrkAction : ICharacterAction
        {
            protected Ork ork;
            protected Vertical verticalBuffer;
            protected Horizontal horizontalBuffer;

            public override void SetManager(ICharacterActionManager actionManager)
            {
                ork = (Ork)actionManager;
                base.SetManager(actionManager);
            }

            public override void OnHit(Wound wound)
            {
                ork.RunTimeData.Health -= wound.Damage;
                ork.RunTimeData.VertigoConter += wound.Vertigo;

                if (wound.KnockBackDistance > 0)
                    ork.SetAction(new OrkHurt(wound));
            }
        }

        private class OrkIdle : IOrkAction
        {
            #region 動作更新
            public override void Start()
            {
                ork.CharacterAnimator.SetFloat("Vertical", (float)ork.RunTimeData.Vertical);
                ork.CharacterAnimator.SetFloat("Horizontal", (float)ork.RunTimeData.Horizontal);

                ork.CharacterAnimator.SetBool("IsFallDown", false);
                ork.CharacterAnimator.SetBool("IsMove", false);
            }
            #endregion

            #region 外部操作
            public override void LightAttack() =>
                actionManager.SetAction(new OrkLightAttack());

            public override void Move(Vertical direction) =>
                actionManager.SetAction(new OrkMove(direction, Horizontal.None));

            public override void Move(Horizontal direction) =>
                actionManager.SetAction(new OrkMove(Vertical.None, direction));
            #endregion
        }

        private class OrkMove : IOrkAction
        {
            public OrkMove(Vertical vertical, Horizontal horizontal)
            {
                verticalBuffer = vertical;
                horizontalBuffer = horizontal;
            }

            #region 動作更新
            public override void Start()
            {
                ork.MoveSound.Play();
                ork.CharacterAnimator.SetFloat("Vertical", (float)ork.RunTimeData.Vertical);
                ork.CharacterAnimator.SetFloat("Horizontal", (float)ork.RunTimeData.Horizontal);
                ork.CharacterAnimator.SetBool("IsMove", true);
            }

            public override void Update()
            {
                if (verticalBuffer == Vertical.None && horizontalBuffer == Horizontal.None)
                {
                    actionManager.SetAction(new OrkIdle());
                }
                else
                {
                    ork.RunTimeData.Vertical = verticalBuffer;
                    ork.RunTimeData.Horizontal = horizontalBuffer;

                    ork.CharacterAnimator.SetFloat("Vertical", (float)ork.RunTimeData.Vertical);
                    ork.CharacterAnimator.SetFloat("Horizontal", (float)ork.RunTimeData.Horizontal);

                    ork.MovementBody.MovePosition(
                        ork.MovementBody.position +
                        new Vector2((float)ork.RunTimeData.Horizontal, (float)ork.RunTimeData.Vertical * 0.6f).normalized
                         * ork.Property.MoveSpeed * Time.deltaTime);
                }
            }

            public override void End()
            {
                ork.MoveSound.Stop();
            }
            #endregion

            #region 外部操作
            public override void LightAttack() =>
               actionManager.SetAction(new OrkLightAttack());

            public override void Move(Vertical direction)
            {
                verticalBuffer = direction;
            }

            public override void Move(Horizontal direction)
            {
                horizontalBuffer = direction;
            }
            #endregion
        }

        private class OrkLightAttack : IOrkAction
        {
            #region 動作更新
            public override void Start()
            {
                ork.animationEnd = false;

                ork.LightAttackColliders.MyDamage
                    = new Wound { Damage = ork.Property.Attack, Vertigo = 0.4f };

                ork.CharacterAnimator.SetTrigger("LightAttack");
                ork.LightAttackSound.Play();
            }

            public override void Update()
            {
                if (ork.animationEnd)
                    actionManager.SetAction(new OrkIdle());
            }
            #endregion
        }

        private class OrkHurt : IOrkAction
        {
            float nowDistance;
            Vector2 knockBackDirection;
            private Wound wound;

            public OrkHurt(Wound wound)
            {
                this.wound = wound;
            }

            #region 動作更新
            public override void Start()
            {
                nowDistance = 0;
                knockBackDirection = (wound.KnockBackFrom - ork.MovementBody.position).normalized;
                ork.CharacterAnimator.SetBool("IsHurt", true);
                ork.HurtSound.Play();
            }

            public override void Update()
            {
                if (nowDistance < wound.KnockBackDistance)
                {
                    Vector2 temp = wound.KnockBackSpeed * knockBackDirection * Time.deltaTime;
                    nowDistance += temp.magnitude;

                    ork.MovementBody.MovePosition(ork.MovementBody.position
                        + temp);
                }
                else
                    ork.SetAction(new OrkIdle());
            }

            public override void End()
            {
                ork.CharacterAnimator.SetBool("IsHurt", false);
            }
            #endregion
        }

        private class OrkDead : IOrkAction
        {
            #region 動作更新
            public override void Start()
            {
                ork.CharacterAnimator.SetBool("IsFallDown", true);
                ork.FallDownSound.Play();
            }
            #endregion
        }
    }
}