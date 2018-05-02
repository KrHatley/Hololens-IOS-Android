using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour, ISubscriber {

    [SerializeField] public int health;
    public int Health { get { return health; } set { health = value; } }
    [SerializeField] public float speed;
    public float Speed { get { return speed; } set { speed = value; } }
    [SerializeField] private int hunger;
    public int Hunger { get { return hunger; } set { hunger = value; } }
    [SerializeField] private int hungerMinimum;
    [SerializeField] private int ateBadFoodScoreAmount = 10;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float minSpeed;
    [SerializeField] protected GameObject acquiredTarget;
    public GameObject AcquiredTarget { get { return acquiredTarget; } }
    [SerializeField] private int[] CurrentPattern;
    [SerializeField] private GameObject tutorialFood;
    [SerializeField] private GameObject snakeBody;
    [SerializeField] private GameObject snakeTail;
    [SerializeField] private Transform backEnd;
    [SerializeField] private Transform TutStartPos;
    [SerializeField] private bool isTutActive;
    [SerializeField] private bool Starving;
    [SerializeField] private AudioClip eatGoodClip;
    [SerializeField] private AudioClip eatBadClip;
    private Vector3 Pos;
    private int[] PreviousPattern;
    private int[] NextPattern;
    private bool isUsingDefaultPattern;
    private AudioSource audioSource;
    private GameObject SnakeHead;
    private GameObject snakeTailClone;
    private List<GameObject> SnakeBodies;
    public int SnakeBodiesCount { get { return SnakeBodies.Count; } }
    private FoodManager FM;
    private FoodColoring TargetFoodType;
    private Rigidbody RB;
    private float speedDifferential;
    private float speedHolder;
    private bool ateBlockedFood;
    private int consecutiveBadFoodAmount = 1;
    public int ConsecutiveBadFoodAmount { get { return consecutiveBadFoodAmount; } set { consecutiveBadFoodAmount = value; } }
    private int hungerGoal;
    public int HungerGoal { get { return hungerGoal; } }
    private bool canSlither;
    public bool CanSlither { get { return canSlither; } set { canSlither = value; } }


    private void Awake()
    {
        RB = gameObject.GetComponent<Rigidbody>();
        SnakeHead = this.gameObject;
        SnakeBodies = new List<GameObject>();
        FM = GameObject.FindGameObjectWithTag("Manager").GetComponent<FoodManager>();
        CurrentPattern = new int[3];
        PreviousPattern = new int[3];
        NextPattern = new int[3];
        isUsingDefaultPattern = true;
        audioSource = transform.GetComponent<AudioSource>();
    }

    public void InitializeSnake(int hp, float startSpeed, bool isTutorialActive, float speedChangeAmount, Transform snakeSpawnPoint)
    {
        health = hp;
        speed = startSpeed;
        isTutActive = isTutorialActive;
        speedDifferential = speedChangeAmount;
        TutStartPos = snakeSpawnPoint;
        snakeTailClone = Instantiate(snakeTail);
        Pause();
    }

    public void InitializeSnake(int consecutiveBadFood, int hp, int snakesHunger, float startSpeed, bool isTutorialActive, float speedChangeAmount, Transform snakeSpawnPoint)
    {
        health = hp;
        hunger = 0;
        consecutiveBadFoodAmount = consecutiveBadFood;
        if (consecutiveBadFood > 1)
            ateBlockedFood = true;
        hungerGoal = snakesHunger;
        if (hungerGoal < hungerMinimum)
        {
            hungerGoal = hungerMinimum;
        }
        speed = startSpeed;
        isTutActive = isTutorialActive;
        speedDifferential = speedChangeAmount;
        TutStartPos = snakeSpawnPoint;
        snakeTailClone = Instantiate(snakeTail);
        Pause();
        canSlither = false;
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // Use this for initialization
    private void Start()
    {
        HandleSnakeBody();
    }

    // Update is called once per frame
    private void Update()
    {
        if(canSlither)
            Slither();

        Starving = FoodDesert();
        if (Starving)
        {
            EventManager.CallTimerAction(Vector3.zero);
            ChangePattern();
        }

        if(isUsingDefaultPattern)
        {
            if(!isTutActive)
            {
                if(FM.HasAvailablePatterns)
                {
                    ChangePattern();
                    isUsingDefaultPattern = false;
                }
            }
        }

        if (!isTutActive && CurrentPattern == FM.DefaultPattern)
        {
            ChangePattern();
        }

        if (isTutActive == true)
        {

            TutorialBehavior();
            Slither();
            if (FM.FoodTutStartPos != null)
            {
                Pos = TutStartPos.position - FM.FoodTutStartPos.position;
                float stoppoint = Pos.magnitude / 2;
                if ((transform.position - FM.FoodTutStartPos.position).magnitude < stoppoint)
                {
                    if(speed != 0)
                    {
                        Pause();
                    }
                }
                if (FM.TestBait.IsStale)
                {
                    Resume();
                }
            }
        }
        else if (health > 0)
        {
            if(acquiredTarget == null)
                Hunt();
        }
    }

    private void LateUpdate()
    {
        MoveSnakeBodies();
    }

    /// <summary>GetLastSnakeBodyPosition:
    /// 
    /// </summary>
    /// <returns></returns>
    public Vector3 GetLastSnakeBodyPosition()
    {
        return SnakeBodies[SnakeBodies.Count - 1].transform.position;
    }

    /// <summary>EatSnakeKibble:
    /// Collision based actions, health, speed and length increment or decrement accordingly
    /// </summary>
    /// <param name="snakeThatAte"></param>
    /// <param name="isStale"></param>
    private void EatSnakeKibble(Snake snakeThatAte, bool isStale)
    {
        if(!isTutActive)
        {
            if (!isStale)
            {
                if (snakeThatAte == this)
                {
                    audioSource.clip = eatGoodClip;
                    audioSource.Play();

                    health++;
                    hunger++;

                    if(ateBlockedFood)
                    {
                        ateBlockedFood = false;
                        consecutiveBadFoodAmount = 1;
                    }

                    if(hunger >= hungerGoal)
                    {
                        EventManager.CallGameOver(true);
                    }

                    HandleSnakeBody();

                    if (speed > minSpeed)
                    {
                        speed -= speedDifferential;
                    }
                }
            }
            else
            {
                if (snakeThatAte == this)
                {
                    audioSource.clip = eatBadClip;
                    audioSource.Play();

                    health--;

                    EventManager.CallPlayerScored(ateBadFoodScoreAmount * consecutiveBadFoodAmount);
                    EventManager.CallDisplayScore(acquiredTarget.transform.position);
                    EventManager.CallDisplayMultiplier(consecutiveBadFoodAmount, acquiredTarget.transform.position);
                    consecutiveBadFoodAmount++;
                    if (!ateBlockedFood)
                    { 
                        ateBlockedFood = true;
                    }

                    if (health == 0)
                    {
                        EventManager.CallSnakeScoreMultiplier(consecutiveBadFoodAmount);
                        GameObject snakeBodyHolder = SnakeBodies[SnakeBodies.Count - 1];
                        snakeBodyHolder.GetComponent<SnakeBody>().StopAllCoroutines();
                        StartCoroutine(snakeBodyHolder.GetComponent<SnakeBody>().HandleBodyDeath());
                        SnakeBodies.Remove(snakeBodyHolder);
                        EventManager.CallRoundOver(true);
                        StartCoroutine(SnakeDeathSpiral());
                    }
                    else
                    {
                        HandleSnakeBody();
                    }

                    if (speed < maxSpeed)
                    {
                        speed += speedDifferential;
                    }
                }
            }
            FM.TargetManagement();
        }
    }


    #region Food Detection Functions
    /// <summary> Hunt:
    /// This function is used to determine what food type to go after and whether or not it has eaten 
    /// that type of food and should move on to the next food type in the pattern.
    /// </summary>
    public virtual void Hunt()
    {
        if (FM.tempTarget[0] == false)
        {
            TargetFoodType = FM.GetFoodType(CurrentPattern[0]);
            FindNextFood();
        }
        else if (FM.tempTarget[0] == true && FM.tempTarget[1] == false)
        {
            TargetFoodType = FM.GetFoodType(CurrentPattern[1]);
            FindNextFood();
        }
        else if (FM.tempTarget[0] == true && FM.tempTarget[1] == true && FM.tempTarget[2] == false)
        {
            TargetFoodType = FM.GetFoodType(CurrentPattern[2]);
            FindNextFood();
        }
        else
        {
            ChangePattern();
            FM.TemporaryTargetReset();
        }
        
    }

    /// <summary> FindNextFood:
    /// Determines the next food object to target and changes movement direction
    /// The new food target is determined by the Pattern and closest object.
    /// </summary>
    private void FindNextFood()
    {
        List<GameObject> PotentialPrey = new List<GameObject>();
        Dictionary<float, GameObject> DistancesFromSnake = new Dictionary<float, GameObject>();
        float smallestDistance = 5000;
        foreach (SnakeKibbles sk in FM.FoodInScene)
        {
            if (sk.TypeofFood == TargetFoodType)
            {
                PotentialPrey.Add(sk.gameObject);
            }
        }
        foreach (GameObject go in PotentialPrey)
        {
            Vector3 tempVar = go.transform.position - this.transform.position;
            if (tempVar.magnitude < smallestDistance)
            {
                smallestDistance = tempVar.magnitude;
            }
            DistancesFromSnake.Add(tempVar.magnitude, go);
        }
        if (smallestDistance < 5000)
        {
            acquiredTarget = DistancesFromSnake[smallestDistance];
            DistancesFromSnake.Clear();
        }

        PotentialPrey.Clear();
        smallestDistance = 5000;
    }


    /// <summary> ChangePattern:
    /// Given a set of patterns of three, the snake changes what order of food would allow it to grow in size and speed
    /// This function is called once a pattern is complete or interupted.
    /// </summary>
    private void ChangePattern()
    {
        PreviousPattern = CurrentPattern;
        
        NextPattern = FM.NewPattern();
        if (NextPattern != CurrentPattern||CurrentPattern == null)
        {
            
            if (FM.CheckTrue(NextPattern))
            {
                CurrentPattern = NextPattern;
            }
            else
            {
                if (FM.HasAvailablePatterns && !isTutActive)
                {
                    do
                    {
                        NextPattern = FM.NewPattern();
                        Debug.Log(string.Format("Failed Test: {0}", NextPattern));
                    } while (FM.CheckTrue(NextPattern) == false);//|| NextPattern == DefaultPattern
                    CurrentPattern = NextPattern;
                }
               
            }
            FM.TemporaryTargetReset();
        }
    }

    /// <summary> FoodDesert:
    /// 
    /// </summary>
    /// <returns></returns>
    private bool FoodDesert()
    {
        bool temp = false;
        if (FM.FoodInScene.Count < 5)
        {
            Starving = true;
            Debug.Log(string.Format("Starving:{0}", Starving));
        }
        else
        {
            temp = false;
        }
        return temp;
    }
    #endregion

    #region SnakeMovement
    /// <summary> Slither:
    /// Movement of the snake and speed of movement
    /// </summary>
    protected void Slither()
    {
        if (acquiredTarget != null)
        {
            ChangeMovementDirection();
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, acquiredTarget.transform.position, step);
        }
    }

    /// <summary> ChangeMovementDirection:
    /// Rotate snake to turn towards the object it is after
    /// </summary>
    private void ChangeMovementDirection()
    {
        Vector3 DesiredDirection = acquiredTarget.transform.position - this.transform.position; // might come back to later
        this.transform.forward = DesiredDirection;
        //transform.rotation = new Quaternion(0, transform.rotation.y, transform.rotation.z, transform.rotation.w);
    }


    /// <summary>
    /// 
    /// </summary>
    private void HandleSnakeBody()
    {
        int HPDifferential = 0;
        HPDifferential = health - SnakeBodies.Count;

        if (HPDifferential < 0)
        {
            for (int i = 0; i > HPDifferential; i--)
            {
                GameObject snakeBodyHolder = SnakeBodies[SnakeBodies.Count - 1];
                SnakeBodies.Remove(snakeBodyHolder);
                StartCoroutine(snakeBodyHolder.GetComponent<SnakeBody>().HandleBodyDeath());
                //SnakeTail move to new last SnakeBody
            }
        }
        else if (HPDifferential == 0)
        {
            Debug.Log("No action required");
        }
        else if (HPDifferential > 0)
        {
            for (int i = 0; i < HPDifferential; i++)
            {
                SnakeBodies.Add(Instantiate(snakeBody));
                if (SnakeBodies.Count == 1)
                    SnakeBodies[SnakeBodies.Count - 1].transform.position = backEnd.position;
                else
                    SnakeBodies[SnakeBodies.Count - 1].transform.position = SnakeBodies[SnakeBodies.Count - 2].GetComponent<SnakeBody>().BackEnd.position;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void MoveSnakeBodies()
    {
        if (SnakeBodies.Count > 0)
        {
            for (int i = 0; i < SnakeBodies.Count; i++)
            {
                if (i == 0)
                    SnakeBodies[i].GetComponent<SnakeBody>().MoveBody(speed, transform, backEnd);
                else
                    SnakeBodies[i].GetComponent<SnakeBody>().MoveBody(speed, SnakeBodies[i - 1].transform, SnakeBodies[i - 1].GetComponent<SnakeBody>().BackEnd);
            }

            snakeTailClone.GetComponent<SnakeTail>().MoveBody(speed, SnakeBodies[SnakeBodies.Count - 1].transform, SnakeBodies[SnakeBodies.Count - 1].GetComponent<SnakeBody>().TailSpot);
        }
    }

    private IEnumerator SnakeDeathSpiral()
    {
        //RB.useGravity = true;
        //float speed = 5f;
        //float step;
        //Vector3 targetScale = new Vector3(.1f, .1f, .1f);
        //Vector3 targetVector = new Vector3();

        //while (transform.localScale.x > .1f)
        //{
        //    step = speed * Time.deltaTime;
        //    transform.localScale = Vector3.Lerp(transform.localScale, targetScale, step * Time.deltaTime);
        //    transform.rotation = new Quaternion(transform.rotation.x + 5f, transform.rotation.y - 5f, transform.rotation.z, transform.rotation.w);
        //    targetVector = new Vector3(transform.position.x * Mathf.Cos(step),transform.position.y*Mathf.Sin(step),transform.position.z);
        //    transform.position = Vector3.MoveTowards(transform.position, targetVector, step);//transform.forward * 2f
        //    yield return null;
        //}

        StartCoroutine(snakeTailClone.GetComponent<SnakeTail>().HandleBodyDeath());
        Destroy(gameObject);

        yield return null;
    }

    #endregion

    #region Tutorial Functions

    /// <summary>
    /// Snake behavior for the tutorial
    /// </summary>
    private void TutorialBehavior()
    {
      
        if (FM.TestBait != null)
        {
                acquiredTarget = FM.TestBait.gameObject;
                isTutActive = true;
        }
        else
        {
            EventManager.CallTutorialEnd(true);
            isTutActive = false;
        }
        

    }

    public void KillSnake()
    {

        foreach (GameObject go in SnakeBodies)
        {
            go.GetComponent<SnakeBody>().StopAllCoroutines();
        }

        snakeTail.GetComponent<SnakeTail>().StopAllCoroutines();

        for (int i = SnakeBodiesCount - 1; i >= 0; i--)
        {
            StartCoroutine(SnakeBodies[i].GetComponent<SnakeBody>().HandleBodyDeath());
            SnakeBodies.Remove(SnakeBodies[i]);
        }

        StartCoroutine(SnakeDeathSpiral());
    }

    /// <summary>
    /// Stops snake movement;
    /// </summary>
    public void Pause()
    {
        speedHolder = speed;
        speed = 0;
    }
    /// <summary>
    /// resumes snake movement
    /// </summary>
    public void Resume()
    {
        speed = speedHolder;
    }

    #endregion

    #region Event Functions
    public void Subscribe()
    {
        EventManager.OnFoodEaten += EatSnakeKibble;
    }

    public void Unsubscribe()
    {
        EventManager.OnFoodEaten -= EatSnakeKibble;
    }
    #endregion

}