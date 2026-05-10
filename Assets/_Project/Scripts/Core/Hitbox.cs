using UnityEngine;
using System.Collections.Generic;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Stats")]
    public int damage = 10;
    public CharacterBase owner;
    private bool hasHit = false;

    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        // Reset the hitbox state when it's enabled
        hasHit = false;
    }

    void Update()
    {
        if (hasHit) return; // If we've already hit something, don't check for collisions

        List<Collider2D> hitObjects = new List<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;

        int count = hitboxCollider.Overlap(filter, hitObjects);

        for (int i = 0; i < count; i++)
        {
            DeliverStrike(hitObjects[i]);
        }
    }

    private void DeliverStrike(Collider2D collision)
    {
        if (hasHit) return;

        CharacterBase target = collision.GetComponent<CharacterBase>();

        if (target != null && target != owner)
        {
            CharacterBase.AttackType currentAttackType = CharacterBase.AttackType.Punch;
            int deliveredDamage = damage;

            if (owner != null)
            {
                // Standing Kick: Low attack, slightly less damage
                if (owner.currentState == CharacterBase.CharacterState.Kick)
                {
                    currentAttackType = CharacterBase.AttackType.Kick;
                    deliveredDamage = damage + 2; // 12 damage
                }
                // Duck Punch: Flagged as low attack so it hits crouchers, reduced damage
                else if (owner.currentState == CharacterBase.CharacterState.DuckAttack)
                {
                    currentAttackType = CharacterBase.AttackType.Kick;
                    deliveredDamage = damage - 2; // 8 damage
                }
                // Duck Kick: Flagged as low attack, lowest damage fast-poke
                else if (owner.currentState == CharacterBase.CharacterState.DuckKick)
                {
                    currentAttackType = CharacterBase.AttackType.Kick;
                    deliveredDamage = damage - 4; // 6 damage
                }
            }

            target.TakeDamage(deliveredDamage, currentAttackType);
            hasHit = true;

            Debug.Log($"{owner.name} hit {target.name} with a {currentAttackType} (from state {owner.currentState}) for {deliveredDamage} damage!");
        }
    }
}
