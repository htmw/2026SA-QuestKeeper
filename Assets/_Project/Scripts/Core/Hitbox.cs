using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Hitbox Stats")]
    public int damage = 10;

    // Find the owner of the hitbox to prevent self-hits
    public CharacterBase owner;

    // Check if the spawned hitbox collides with another player's hurtbox (or just their collider)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if what we collide with has a CharacterBase script attached to it
        CharacterBase target = collision.GetComponent<CharacterBase>();

        // Deal Damage
        if (target != null && target != owner)
        {
            target.TakeDamage(damage);
            Debug.Log($"{owner.gameObject.name} hit {target.gameObject.name} for {damage} damage!");
        }
    }
}
