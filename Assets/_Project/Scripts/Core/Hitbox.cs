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
            DeliverPunch(hitObjects[i]);
        }
    }

    private void DeliverPunch(Collider2D collision)
    {
        if (hasHit) return; // Prevent multiple hits in one attack

        CharacterBase target = collision.GetComponent<CharacterBase>();

        if (target != null && target != owner)
        {
            target.TakeDamage(damage);
            hasHit = true; // Mark that we've hit something to prevent multiple hits
            Debug.Log($"{owner.name} hit {target.name} for {damage} damage! {target.name} has {target.currentHealth} health left.");
        }
    }
}
