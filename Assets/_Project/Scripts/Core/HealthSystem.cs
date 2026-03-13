using UnityEngine;
using UnityEngine.Rendering;

public class HealthSystem : MonoBehaviour
{

    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currHealth;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        currHealth = maxHealth;
        Debug.Log(gameObject.name + " HP: " + currHealth);
    }

    // Reset Health
    public void ResetHealth()
    {
        currHealth = maxHealth;
        Debug.Log(gameObject.name + " HP Reset: " + currHealth);
    }

    public void TakeDamage(int damageTaken)
    {
        currHealth -= damageTaken;

        if (currHealth < 0)
        {
            currHealth = 0;
        }

        Debug.Log(gameObject.name + " took " + damageTaken + " damage. Current HP: " + currHealth);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
