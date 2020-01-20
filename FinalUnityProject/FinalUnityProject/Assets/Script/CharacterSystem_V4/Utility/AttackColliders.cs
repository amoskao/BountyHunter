﻿using UnityEngine;

namespace CharacterSystem_V4
{
    public class AttackColliders : MonoBehaviour
    {
        public DamageData MyDamage;
        public string TargetTag;
        public bool HitAll;

        private bool hasHitTarget;

        private void Start()
        {
            hasHitTarget = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if ((HitAll || !hasHitTarget) && collision.gameObject.tag == TargetTag)
            {
                //Debug.Log($"Target Enter : {TargetTag}");
                hasHitTarget = true;
                var hitPoint = Physics2D.Raycast(gameObject.gameObject.transform.position,
                    collision.gameObject.transform.position - gameObject.gameObject.transform.position);
                MyDamage.HitAt = hitPoint.point;
                MyDamage.HitFrom = gameObject.gameObject.transform.position;
                collision.gameObject.GetComponentInParent<ICharacterActionManager>().OnHit(MyDamage);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if(collision.gameObject.tag == TargetTag)
            {
                //Debug.Log($"Target Exit: {TargetTag}");
                hasHitTarget = false;
            }
        }
    }
}
