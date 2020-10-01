using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject cam;
    public GameObject stevePerfab;
    public GameObject bloodPrefab;
    public GameObject uiBloodPrefab;
    public GameObject canvas;
	public GameObject gameOverText;
    public GameObject[] checkPoints;
    public CompassController compassC;
    private float cWidth;
    private float cHeight;
    //-----------------
    
    private float bloodWidth;
    private float bloodHeight;
    //----------------
    public Slider helthBar;
    public Text textAmmoReserves;
    public Animator anim;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource medSound;
    public AudioSource ammoSound;
    public AudioSource[] footsteps;
    public AudioSource triggerSound;
    public AudioSource damage;
    public AudioSource reloadWeapon;
    private float speed = 0.1f;
    private float speed2 = 2f;
    //private float speed3 = 2f;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    //поворот камеры 
    private Quaternion cameraRot;
    //поворот персонажа
    private Quaternion characterRot;
    private float MinX = -90f;
    private float MaxX = 90;
    private bool cursorIsLocked = true;
    private bool lockCursor = true;
    private float x;
    private float z;
    //Inventory
    private int ammo = 20;
    private int maxAmmo = 100;
    private int med = 100;
    private int maxMed = 100;
    private int ammoClip = 10;
    private int ammoClipMax = 10;
    private bool playWalking = false;
    private bool previouslyGrounded = true;
   	public Transform shotDirection;

	public int lives = 3;
	private	int timesDied = 0;
    private GameObject steve;
    private Vector3 startPosition;
    private int currentCheckPoint = 0;
    public LayerMask checkPointLayer;

    public void TakeHit(float amount)
    {
        if(GameStats.gameOver) return;
        med = (int) Mathf.Clamp(med - amount, 0, maxMed);
        helthBar.value = med;
        Debug.Log("med" + med);

        GameObject bloodSplatter = Instantiate(uiBloodPrefab);
        bloodSplatter.transform.SetParent(canvas.transform);
        bloodSplatter.transform.position = new Vector3(Random.Range(0,cWidth),Random.Range(0,cHeight),0);
        int randomSize = Random.Range(1, 3);
        bloodSplatter.transform.localScale  = new Vector3(randomSize,randomSize,0);
        
        Destroy(bloodSplatter, 2.2f);
       
        if (med <= 0)
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                Terrain.activeTerrain.SampleHeight(this.transform.position),
                this.transform.position.z);
            steve = Instantiate(stevePerfab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Death");
            GameStats.gameOver = true;
            steve.GetComponent<AudioSource>().enabled = false;
            timesDied ++;

        } 
        if (timesDied == lives)
        {
                     Destroy(this.gameObject);
        }
        else
        {
            steve.GetComponent<AudioVictory>().enabled = false;
            cam.SetActive(false);
            Invoke("Respawn", 6);
        }
    }

    void Respawn()
    {
        Destroy(steve);
        cam.SetActive(true);
        GameStats.gameOver = false;
        med = maxMed;
        helthBar.value = med;
        this.transform.position = startPosition;
    }
void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Home")
        {
            Vector3 pos = new Vector3(this.transform.position.x,
                Terrain.activeTerrain.SampleHeight(this.transform.position),
                this.transform.position.z);
            GameObject steve = Instantiate(stevePerfab, pos, this.transform.rotation);
            steve.GetComponent<Animator>().SetTrigger("Dance");
			
			GameObject bloodSplatter = Instantiate(gameOverText);
        	bloodSplatter.transform.SetParent(canvas.transform);
 			bloodSplatter.transform.localPosition = new Vector3(0,0,0);

			

            GameStats.gameOver = true;
			Destroy(this.gameObject);
            
        }

        if (col.gameObject.tag == "SpawnPoint")
        {
            startPosition = this.transform.position;
            if (col.gameObject == checkPoints[currentCheckPoint])
            {
                currentCheckPoint++;
                compassC.target = checkPoints[currentCheckPoint];
            }
            
        }
    }

    //---------------------------------------------
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        capsule = this.GetComponent<CapsuleCollider>();

        cameraRot = cam.transform.localRotation;
        characterRot = this.transform.localRotation;
        
        med = maxMed;
        helthBar.value = med;

        textAmmoReserves.text = ammo + "/" + ammoClip;

        cWidth = canvas.GetComponent<RectTransform>().rect.width;
        cHeight = canvas.GetComponent<RectTransform>().rect.height;

        startPosition = this.transform.position;
        compassC.target = checkPoints[0];
    }

    void ProcessZombieHit()
    {
       RaycastHit hitInfo;
       if (Physics.Raycast(shotDirection.position, shotDirection.forward, out hitInfo, 200, ~checkPointLayer))
       {
           GameObject hitZombie = hitInfo.collider.gameObject;
           if (hitZombie.tag == "Zombie")
           {
               GameObject blood = Instantiate(bloodPrefab, hitInfo.point, Quaternion.identity);
               blood.transform.LookAt(this.transform.position);
               Destroy(blood, 0.5f);

               hitZombie.GetComponent<ZombieController>().shotsTaken++;
               if (hitZombie.GetComponent<ZombieController>().shotsTaken ==
                   hitZombie.GetComponent<ZombieController>().shotsRewuired)
               {
                   
               
               if (Random.Range(0, 10) < 5)
               {
                   GameObject rdPrefab = hitZombie.GetComponent<ZombieController>().ragdoll; 
                   GameObject newRD = Instantiate(rdPrefab, hitZombie.transform.position, hitZombie.transform.rotation);
                   newRD.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(shotDirection.forward * 1000);
                   Destroy(hitZombie);
               }
               else
               {
                   hitZombie.GetComponent<ZombieController>().KillZombie();
               }
               return; 
               }
           }
       }
    }

    // Update is called once per frame
    void Update()
    {

        UpdateCursorLock();
        
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool("Arm", !anim.GetBool("Arm"));

        Debug.Log(GameStats.canShoot);
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("Fire") && anim.GetBool("Arm") && GameStats.canShoot )
        {
            if (ammoClip > 0)
            {
                 anim.SetTrigger("Fire");
                 ProcessZombieHit();
                 ammoClip --;
                 textAmmoReserves.text = ammo + "/" + ammoClip;
                 GameStats.canShoot = false;
            }
            else 
            {
                triggerSound.Play();
            }
            
             Debug.Log(ammoClip);
             // shot.Play();
        }

        if (Input.GetKeyDown(KeyCode.R) && anim.GetBool("Arm"))
        {
            anim.SetTrigger("Reloading");
            reloadWeapon.Play();

            int amountNeeded = ammoClipMax - ammoClip;
            int ammoAvailable = amountNeeded < ammo ? amountNeeded : ammo;
            ammo -= ammoAvailable;
            ammoClip += ammoAvailable; 
            textAmmoReserves.text = ammo + "/" + ammoClip;
            Debug.Log("ammoClip"+ ammoClip);
            Debug.Log("ammo"+ ammo);
            
            
        }
            
        
       
        if (Mathf.Abs(x) >0 || Mathf.Abs(z)>0)
        {
            if (!anim.GetBool("Walking"))
            {
                anim.SetBool("Walking", true);
                InvokeRepeating("PlayFootStepAudio",0,0.4f);
               
            }
        }else if (anim.GetBool("Walking"))
        {
            anim.SetBool("Walking", false);
            CancelInvoke("PlayFootStepAudio");
            playWalking = false;
            
            
        }

        bool grounded = IsGrounded(); 
        if (Input.GetKeyDown(KeyCode.Space)&& grounded)
        {
            rb.AddForce(0, 300, 0); 
            jump.Play();
            if (anim.GetBool("Walking"))
            {
                CancelInvoke("PlayFootStepAudio");
                playWalking = false;
                
            }
                
        }else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }
        previouslyGrounded = grounded;
    }

    void PlayFootStepAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, footsteps.Length);

        audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;

        playWalking = true;
    }
    void FixedUpdate()
    {
        float yRot = Input.GetAxis("Mouse X")* speed2;
        float xRot = Input.GetAxis("Mouse Y")* speed2;

        cameraRot *= Quaternion.Euler(-xRot , 0, 0);
        characterRot *= Quaternion.Euler(0, yRot , 0);

        cameraRot = ClampRotationAroundXAxis(cameraRot);

        this.transform.localRotation = characterRot;
        cam.transform.localRotation = cameraRot;

         

         
         //Клавишы лево и право 
         x = Input.GetAxis("Horizontal")*speed;
        
        //Клавишы ввер и вниз
         z = Input.GetAxis("Vertical")*speed;
        transform.position += cam.transform.forward * z + cam.transform.right * x; //new Vector3(x * speed, 0, z * speed);

        UpdateCursorLock();
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;
        //преобразование кватернионa в угол эйлера
        float angleX =  2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinX, MaxX);
        //берет  угол Эйлера, который  создали (и зафиксировали), и превращает его обратно в кватернион.
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);
        return q;
    }
    bool IsGrounded()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out hitInfo,
            (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }

        
        return false;
    }
    public void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Med" && med < maxMed)
        {
            med = Mathf.Clamp(med + 50,0,maxMed);
            Debug.Log("MedBox" + med);
            helthBar.value = med;
            medSound.Play();
            Destroy(col.gameObject);
           
        }
        
        else if (col.gameObject.tag == "Lava")
        { 
            med = Mathf.Clamp(med - 10, 0,maxMed );
            Debug.Log(med);
            helthBar.value = med;
            if (med <= 0)
            {
                damage.Play();
            }
            
        }
        
        else if (col.gameObject.tag == "Ammo" && ammo < maxAmmo )
        {
            ammo = Mathf.Clamp(ammo + 10,0,maxAmmo);
            Debug.Log("Ammo" + ammo);
            textAmmoReserves.text = ammo + "/" + ammoClip;
            ammoSound.Play();
            Destroy(col.gameObject);
        }
        else if (IsGrounded())
        {
            if(anim.GetBool("Walking")&& !playWalking)
                 InvokeRepeating("PlayFootStepAudio",0,0.4f);
        }
           
    }
    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
            InternalLockUpdate();
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
            cursorIsLocked = true;

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if(!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}