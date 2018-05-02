using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeKibbles : MonoBehaviour {

    [SerializeField] private FoodColoring typeofFood;
    [SerializeField] private FoodColoring prevtypeofFood;
    [SerializeField] private GameObject staleFoodVisual;
    [SerializeField] private GameObject[] snakeKibbleVisuals;
    [SerializeField] private AudioClip hitByProjectileClip;
    [SerializeField] private AudioClip targetFoodWarningClip;
    [SerializeField] private bool isStale;
    [SerializeField] private int scoreAmountForShootingFood = 5;
    [SerializeField] private TargetedFoodLayerChange targetedFoodLayerChange;
    public bool IsStale { get { return isStale; } }
    private FoodManager FM;
    private bool wasEaten;
    public AudioSource hitByProjectileAudioSource;

    public FoodColoring TypeofFood
    {
        get{ return typeofFood; }
        set{ typeofFood = value;}
    }

    public FoodColoring PrevtypeofFood
    {
        get{ return prevtypeofFood; }
        set{ prevtypeofFood = value; }
    }

    private void Awake()
    {
        FM = GameObject.FindGameObjectWithTag("Manager").GetComponent<FoodManager>();
        targetedFoodLayerChange.Initialize();
        RandomizeColor();
        hitByProjectileAudioSource = GetComponent<AudioSource>();
    }
    // Use this for initialization
    private void Start()
    {
        isStale = false;
    }

    private void Update()
    {
        if(gameObject.layer == 12 && !isStale)
        {
            hitByProjectileAudioSource.clip = targetFoodWarningClip;
            if(!hitByProjectileAudioSource.isPlaying)
            {
                hitByProjectileAudioSource.Play();
                hitByProjectileAudioSource.loop = true;
            }
        }
        else if (gameObject.layer == 12 && isStale)
        {
            if (hitByProjectileAudioSource.clip == targetFoodWarningClip)
                hitByProjectileAudioSource.Stop();
        }

        if(GameController.instance._GameState == GameController.GameStates.GamePause || 
            GameController.instance._GameState == GameController.GameStates.GameOver)
        {
            hitByProjectileAudioSource.Stop();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        GameObject collisionGO = other.gameObject;

        if (collisionGO.GetComponent<Snake>() != null)
        {
            if (collisionGO.GetComponent<Snake>().AcquiredTarget == this.gameObject)
            {
                EventManager.CallFoodEaten(collisionGO.GetComponent<Snake>(), isStale);
                typeofFood = FoodColoring.Digested;
                EventManager.CallFoodDespawn(this.gameObject);
                wasEaten = true;
            }
        }
        else if (collisionGO.GetComponent<Bullet>() != null)
        {
            if (!isStale)
            {
                isStale = true;
                EventManager.CallBulletHit(collisionGO, isStale);
                EventManager.CallBulletHitScore(scoreAmountForShootingFood);
                EventManager.CallDisplayScore(transform.position);
                EventManager.CallDisplayMultiplier(0, transform.position);
                staleFoodVisual.SetActive(true);
                hitByProjectileAudioSource.Stop();
                hitByProjectileAudioSource.clip = hitByProjectileClip;
                hitByProjectileAudioSource.loop = false;
                hitByProjectileAudioSource.Play();
            }
            else
            {
                collisionGO.GetComponent<Bullet>().HasHitFood = true;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!wasEaten)
        {
            GameObject collisionGO = other.gameObject;

            if (collisionGO.GetComponent<Snake>() != null)
            {
                if (collisionGO.GetComponent<Snake>().AcquiredTarget == this.gameObject)
                {
                    EventManager.CallFoodEaten(collisionGO.GetComponent<Snake>(), isStale);
                    typeofFood = FoodColoring.Digested;
                    EventManager.CallFoodDespawn(this.gameObject);
                }
            }
        }
    }
    /// <summary> RandomizeColor:
    /// Determines food type
    /// Called in awake method and will be used by snake to determine what food to eat next.
    /// </summary>
    private void RandomizeColor()
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.shader = Shader.Find("Specular");
        
        int i = UnityEngine.Random.Range(0, 4);
        switch (i)
        {
            
            default:
                typeofFood = FoodColoring.Red;
                prevtypeofFood = FoodColoring.Red;
                rend.material.SetColor("_Color", Color.red);
                break;
            case 0:
                typeofFood = FoodColoring.Red;
                prevtypeofFood = FoodColoring.Red;
                rend.material.SetColor("_Color", Color.red);
                break;
            case 1:
                typeofFood = FoodColoring.Blue;
                prevtypeofFood = FoodColoring.Blue;
                rend.material.SetColor("_Color", Color.blue);
                break;
            case 2:
                typeofFood = FoodColoring.Green;
                prevtypeofFood = FoodColoring.Green;
                rend.material.SetColor("_Color", Color.green);
                break;
            case 3:
                typeofFood = FoodColoring.Ultra;
                prevtypeofFood = FoodColoring.Ultra;
                rend.material.SetColor("_Color", Color.magenta);
                break;
        }
        snakeKibbleVisuals[i].SetActive(true);
        EventManager.CallTypeOfFoodChoosen(targetedFoodLayerChange, snakeKibbleVisuals[i]);
        EventManager.CallFoodTypeChoice(this.gameObject);
    }
}
