using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public GameObject target;
    public GameObject ragdoll;
    private Animator anim;
     NavMeshAgent agent;
     public float walkingSpeed;
     public float runningSpeed;

    enum STATE { IDLE,WANDER,CHASE,ATTACK,DEAD };

    private STATE state = STATE.IDLE;

    public float damageAmount = 20;
    public AudioSource[] damageZombie;
    private bool playDamage = false;
    public int shotsRewuired = 3;
    public	int shotsTaken  ;
    // Start is called before the first frame update
    void Start()
    { 
        
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
    } 
    void TurnOffTriggers()
             {
                 anim.SetBool("isWalking", false);
                 anim.SetBool("isAttack", false);
                 anim.SetBool("isRunning", false);
                 anim.SetBool("isDeath", false);
             }

    float DistanceToPlayer()
    {

        if (GameStats.gameOver)
        {
            return Mathf.Infinity;
        }
        return Vector3.Distance(target.transform.position, this.transform.position);
    }

    bool CanSeePlayer()
    {
        if (DistanceToPlayer() < 10)
            return true;
        return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
            return true;
        return false;
    }

    public void KillZombie()
    {
        TurnOffTriggers();
        anim.SetBool("isDeath", true);
        state = STATE.DEAD;
    }

    public void DamagePlayer()
    {
        if (target != null)
        {
            target.GetComponent<Controller>().TakeHit(damageAmount);
            PlayDamageAudio();
        }
        
    }

    void PlayDamageAudio()
    {
        AudioSource audioSource = new AudioSource();
                int n = Random.Range(1, damageZombie.Length);
                audioSource = damageZombie[n];
                audioSource.Play();
                damageZombie[n] = damageZombie[0];
                damageZombie[0] = audioSource;
                playDamage = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.T))
        // {
        //     if (Random.Range(0, 10) < 5)
        //     {
        //          GameObject rb = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
        //          rb.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 10000);
        //          Destroy(this.gameObject);
        //     }
        //     else
        //     {
        //         TurnOffTriggers();
        //         anim.SetBool("isDeath", true);
        //         state = STATE.DEAD;
        //     }
        //     return; 
        // }
        if (target == null && GameStats.gameOver == false)
        {
            target = GameObject.FindWithTag("Player");
            return;
        }
        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer()) state = STATE.CHASE;
                    else if (Random.Range(0,5000)<5)
                {
                    state = STATE.WANDER;
                }
                
                break;
            case STATE.WANDER:
                if (!agent.hasPath)
                {
                    float newX = this.transform.position.x + Random.Range(-5, 5);
                    float newZ = this.transform.position.z + Random.Range(-5, 5);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 dest = new Vector3(newX, newY, newZ);
                    agent.SetDestination(dest);
                    agent.stoppingDistance = 0;
                    TurnOffTriggers();
                    agent.speed = walkingSpeed;
                    anim.SetBool("isWalking", true);
                }

                if (CanSeePlayer()) state = STATE.CHASE;
                else if(Random.Range(0,5000)<5)
                {
                    state = STATE.IDLE;
                    TurnOffTriggers();
                    agent.ResetPath();
                }
                break;
            case STATE.CHASE:
                if (GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state = STATE.WANDER;
                    return;
                }
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 5;
                TurnOffTriggers();
                agent.speed = runningSpeed;
                anim.SetBool("isRunning", true);

                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending )
                {
                    state = STATE.ATTACK;
                }

                if ( ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }
                break;
            case STATE.ATTACK:
                if (GameStats.gameOver)
                {
                    TurnOffTriggers();
                    state = STATE.WANDER;
                    return;
                }
                TurnOffTriggers();
                anim.SetBool("isAttack", true);
                //this.transform.LookAt(target.transform.position);
                
                Vector3 targetPosition = new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z);
                this.transform.LookAt(targetPosition);
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                
                break;
            case STATE.DEAD:
                // TurnOffTriggers();
                // anim.SetBool("isDead", true);
                Destroy(agent);
				
				AudioSource[] sound = this.GetComponents<AudioSource>();
				foreach(AudioSource s in sound)
					s.volume = 0;
				
                this.GetComponent<Sink>().StartSink();
                break;
        }
    }
}
