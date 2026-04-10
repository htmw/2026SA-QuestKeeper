using UnityEngine;

public class EasyController : MonoBehaviour
{
    public string difficultyLevel = "Easy";
    public Transform playerTransform;


    //variables for TC03_02
    public float actionFreeze = 0.8f; 
    private float timer;
    private Animator anim;
    private Rigidbody2D rb;

    // var TC03_03
    public float moveSpeed = 2f;
    public float stoppingDistance = 1f;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        //check that controller works
        Debug.Log("AI status: " + difficultyLevel + "Controller is active and manage opponent");

        if (playerTransform == null) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) {
                playerTransform = player.transform;
                Debug.Log("Easy level AI: Player found successfully");
            } else {
                Debug.LogWarning ("Easy level AI: Player not found!");
            }
        }
    }
    
    void Update()
    {
        //work with bugs, ai moving before play started
        if (GameManager.Instance == null || GameManager.Instance.currState != GameManager.MatchStates.Playing) {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("Moving", false);
            return;
        }
       
        //random actions TC03_02
        timer += Time.deltaTime;
        if (timer >= actionFreeze) {
            PerformRandomAction();
            timer = 0;
        }

        //move to player  TC03_03
        if (playerTransform != null) {
            //count distance 
            float distanceX = playerTransform.position.x - transform.position.x;

            //AI stop if too close to pplayer
            if (Mathf.Abs(distanceX) > stoppingDistance) {
                float direction = Mathf.Sign(distanceX);
                
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

                anim.SetBool("Moving", true);
                transform.localScale = new Vector3(direction, 1, 1);
            } else {
                //if AI came too close yo player thatn it idle/wait 
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("Moving", false);
            }
        }
    }

    void PerformRandomAction() {
        if (playerTransform == null)
            return;

        int randomChoice = Random.Range(0, 5);

        switch (randomChoice) {
        case 0: 
            //part for TC03_02/03
            anim.SetTrigger("Attacking");
            Debug.Log("Easy AI Level Action: Attack");
           
            //part for TC 03_04
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < 1.8f) {
                CharacterBase playerBase = playerTransform.GetComponent<CharacterBase>();
                if (playerBase != null) {
                    playerBase.TakeDamage(10);
                    Debug.Log("AI hit Player through CharacterBase system!");
                }
            }            
            break;

        case 1:
            anim.SetTrigger("Blocking");
            Debug.Log("Easy AI Level Action: Block");
            break;
        case 2: 
            rb.AddForce(Vector2.up * 12f, ForceMode2D.Impulse);
            anim.SetTrigger("Jumping");
            Debug.Log("Easy AI Level Action: Jump");
            break;
        case 3: 
            float dir = (playerTransform.position.x > transform.position.x) ? 1 : -1;
            rb.AddForce(new Vector2(dir * 5f, 0), ForceMode2D.Impulse); 
            Debug.Log("Easy AI Level Action: Move");
            break;
        case 4:
            Debug.Log("Easy AI Level Action: Idle/Waiting");
            //added here dropping anim in few sec  to prevent infinitely walk
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("Moving", false);
            break;
        }
    }
}
