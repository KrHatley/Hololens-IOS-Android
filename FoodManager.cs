using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FoodManager : MonoBehaviour,ISubscriber {

    [SerializeField] public GameObject SnakeFoodPrefab;
    [SerializeField] private List<int[]> AvailablePatterns;
    [SerializeField] public GameObject[] Predators;
    [SerializeField] public List<SnakeKibbles> FoodInScene;
    [SerializeField] private List<FoodColoring> AvailableFoodTypes;
    [SerializeField] public Transform FoodTutStartPos;
    [SerializeField] public SnakeKibbles TestBait;
    private int[,] PossiblePatterns;
    private string[] PatternsTextFile;
    private int fileLength;
    private string path;
    public bool[] tempTarget;
    private int incrementTemp = 0;
    private int RedCount;
    private int BlueCount;
    private int GreenCount;
    private int UltraCount;
    private bool hasAvailablePatterns;
    public bool HasAvailablePatterns { get { return hasAvailablePatterns; } }
    public int[] DefaultPattern;
    private GameObject clone;

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    // Use this for initialization
    private void Awake()
    {
        DefaultPattern = new int[3];
        hasAvailablePatterns = false;
        tempTarget = new bool[3];
#if UNITY_EDITOR
    
        path = Path.Combine( Environment.CurrentDirectory, @"Assets\Scripts\Food Functions" );// do not fuck with this line
        PatternsTextFile = File.ReadAllLines(Path.Combine(path, "Patterns.txt"));
        fileLength = PatternsTextFile.Length / 3;
        PossiblePatterns = SortTextFile();
#endif
#if WINDOWS_UWP
        PossiblePatterns = GeneratePatterns();
        fileLength = 64;
#endif
    }

    private void Start ()
    {
        for (int i = 0; i<tempTarget.Length;i++)
        {
            tempTarget[i] = false;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        AvailableFoodTypes = ScavengeForType();
        AvailablePatterns = AvailablePatternsInScene();

        Predators = GameObject.FindGameObjectsWithTag("Predators");

        if (!hasAvailablePatterns && AvailablePatterns.Count > 0)
            hasAvailablePatterns = true;
        else
            hasAvailablePatterns = false;
    }

    #region Target and Cleanup
    /// <summary>
    /// Cleans the list of Active Food in scene
    /// </summary>
    private void CleanUp(GameObject snakeKibbleGameObject)
    {
        switch (snakeKibbleGameObject.GetComponent<SnakeKibbles>().PrevtypeofFood)
        {
            case FoodColoring.Red:
                RedCount--;
                break;
            case FoodColoring.Blue:
                BlueCount--;
                break;
            case FoodColoring.Green:
                GreenCount--;
                break;
            case FoodColoring.Ultra:
                UltraCount--;
                break;
            default:
                Debug.Log("Wrong FoodType found.");
                break;
        }
        FoodInScene.Remove(snakeKibbleGameObject.GetComponent<SnakeKibbles>());
        Destroy(snakeKibbleGameObject);
    }

 
    /// <summary>
    /// Public method used with snakes to determine what part of the pattern it should be on
    /// Will need to be changed once multiple snakes are added.
    /// </summary>
    public void TargetManagement()
    {
        tempTarget[incrementTemp] = true;
        incrementTemp++;
        if (incrementTemp>=4)
        {
            TemporaryTargetReset();
        }
    }

    /// <summary>TemporaryTargetReset:
    /// Resets the  TargetManagement method, will also need to be changed
    /// </summary>
    public void TemporaryTargetReset()
    {
        incrementTemp = 0;
        for (int i = 0; i < tempTarget.Length; i++)
        {
            tempTarget[i] = false;
        }

    }
    #endregion

    #region Pattern Initialization

    /// <summary> SortTextFile:
    /// Reads the Text file and creates and Array of all possible food patterns.
    /// </summary>
    /// <returns></returns>
    private int[,] SortTextFile()
    {
        int k = 0;
        int[,] SortArray = new int[fileLength, 3];
        for (int j = 0; j < fileLength; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                SortArray[j, i] = System.Int32.Parse(PatternsTextFile[k]);/// Does not convert correctly
                k++;
            }
        }
        return SortArray;
    }

    /// <summary>
    /// Generates patterns when running in hololens
    /// </summary>
    /// <returns></returns>
    int[,] GeneratePatterns()
    {
        int[,] tempArray = new int[64, 3];
        int i = 0;
        for (int j = 0; j < 64; j++)
        {
            if (j <= 16)
            {
                tempArray[j, 0] = 0;
                if (j < 4)
                {
                    tempArray[j, 1] = 0;
                    tempArray[j, 2] = i;
                }
                else if (j >= 4 && j < 8)
                {
                    tempArray[j, 1] = 1;
                    tempArray[j, 2] = i;
                }
                else if (j >= 8 && j < 12)
                {
                    tempArray[j, 1] = 2;
                    tempArray[j, 2] = i;
                }
                else if (j >= 12 && j < 17)
                {
                    tempArray[j, 1] = 3;
                    tempArray[j, 2] = i;
                }
                i++;
                if (i == 4)
                {
                    i = 0;
                }
            }
            else if (j >= 17 && j < 32)
            {
                tempArray[j, 0] = 1;
                if (j >= 17 && j < 20)
                {
                    tempArray[j, 1] = 0;
                    tempArray[j, 2] = i;
                }
                else if (j >= 20 && j < 24)
                {
                    tempArray[j, 1] = 1;
                    tempArray[j, 2] = i;
                }
                else if (j >= 24 && j < 28)
                {
                    tempArray[j, 1] = 2;
                    tempArray[j, 2] = i;
                }
                else if (j >= 28 && j < 32)
                {
                    tempArray[j, 1] = 3;
                    tempArray[j, 2] = i;
                }
                i++;
                if (i == 4)
                {
                    i = 0;
                }
            }
            else if (j >= 32 && j < 48)
            {
                tempArray[j, 0] = 2;
                if (j >= 32 && j < 36)
                {
                    tempArray[j, 1] = 0;
                    tempArray[j, 2] = i;
                }
                else if (j >= 36 && j < 40)
                {
                    tempArray[j, 1] = 1;
                    tempArray[j, 2] = i;
                }
                else if (j >= 40 && j < 44)
                {
                    tempArray[j, 1] = 2;
                    tempArray[j, 2] = i;
                }
                else if (j >= 44 && j < 48)
                {
                    tempArray[j, 1] = 3;
                    tempArray[j, 2] = i;
                }
                i++;
                if (i == 4)
                {
                    i = 0;
                }
            }
            else if (j >= 48 && j <= 64)
            {
                tempArray[j, 0] = 3;
                if (j >= 48 && j < 52)
                {
                    tempArray[j, 1] = 0;
                    tempArray[j, 2] = i;
                }
                else if (j >= 52 && j < 56)
                {
                    tempArray[j, 1] = 1;
                    tempArray[j, 2] = i;
                }
                else if (j >= 56 && j < 60)
                {
                    tempArray[j, 1] = 2;
                    tempArray[j, 2] = i;
                }
                else if (j >= 60 && j < 65)
                {
                    tempArray[j, 1] = 3;
                    tempArray[j, 2] = i;
                }
                i++;
                if (i == 4)
                {
                    i = 0;
                }
            }
        }
        return tempArray;
    }
    #endregion

    #region Pattern Verification
    /// <summary>GetFoodType:
    /// Determines which SnakeKibble object to look for based on the currentpattern;
    /// </summary>
    public FoodColoring GetFoodType(int i)
    {
        FoodColoring temp = FoodColoring.Red;
        switch (i)
        {
            case 0:
                temp = FoodColoring.Red;
                break;
            case 1:
                temp = FoodColoring.Blue;
                break;
            case 2:
                temp = FoodColoring.Green;
                break;
            case 3:
                temp = FoodColoring.Ultra;
                break;
        };
        return temp;
    }

    /// <summary>
    /// Makes list of food types active in scene
    /// </summary>
    /// <returns></returns>
    private List<FoodColoring> ScavengeForType()
    {
        List<FoodColoring> TempTypeList = new List<FoodColoring>();

        foreach (SnakeKibbles sk in FoodInScene)
        {
            if (!TempTypeList.Contains(sk.TypeofFood))
            {
                TempTypeList.Add(sk.TypeofFood);
            }
        }
        return TempTypeList;
    }

    

    /// <summary>
    /// Gets the number of each type of food in the scene
    /// </summary>
    /// <param name="fc"></param>
    private void GetFoodCount(FoodColoring fc)
    {
        switch (fc)
        {
            case FoodColoring.Red:
                RedCount++;
                break;
            case FoodColoring.Blue:
                BlueCount++;
                break;
            case FoodColoring.Green:
                GreenCount++;
                break;
            case FoodColoring.Ultra:
                UltraCount++;
                break;
            case FoodColoring.Digested:
                break;
            //case FoodColoring.Stale:
            //    break;
            case FoodColoring.Nibbled:
                break;
        }
    }

    /// <summary>AvailablePatternsInScene:
    ///  Returns a List of patterns that are available based on the food currently in scene
    /// </summary>
    /// <returns></returns>
    private List<int[]> AvailablePatternsInScene()
    {
        List<int[]> TempPatterns = new List<int[]>();
        for (int i = 0; i < fileLength; i++)
        {
            if (AvailableFoodTypes.Contains(GetFoodType(PossiblePatterns[i, 0])))
            {
                if (AvailableFoodTypes.Contains(GetFoodType(PossiblePatterns[i, 1])))
                {
                    if (AvailableFoodTypes.Contains(GetFoodType(PossiblePatterns[i, 2])))
                    {
                        int[] intTemp = new int[3];
                        for (int k = 0; k < 3; k++)//may need to change
                        {
                            intTemp[k] = PossiblePatterns[i, k];
                        }
                        if (TempPatterns.Contains(intTemp) == false)
                        {
                            if (CheckTrue(intTemp) == true)
                            {
                                TempPatterns.Add(intTemp);
                            }
                        }
                    }
                }
            }
        }
        return TempPatterns;
    }


    /// <summary>NewPattern:
    /// Called whenever a snake completes a pattern by eating the food in the correct order or is completely blocked by the player,
    /// A size 3 integer array used to identify food type via the Food Coloring Enum.
    /// </summary>
    /// <returns> A size 3 array used to identify food type. </returns>
    public int[] NewPattern()
    {
        int patternNumber;
        int[] newPattern = new int[3];
        if (AvailablePatterns.Count > 0)
        {
            patternNumber = UnityEngine.Random.Range(0, AvailablePatterns.Count);
            newPattern = AvailablePatterns[patternNumber];
        }
        else
        {
            hasAvailablePatterns = false;
        }
        return newPattern;
    }

    /// <summary>
    /// Checks if a pattern is possible
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public bool CheckTrue(int[] i)
    {
        bool temp;
        int tempRed = 0;
        int tempBlue = 0;
        int tempGreen = 0;
        int tempUltra = 0;
        bool temp1 = false;
        bool temp2 = false;
        bool temp3 = false;
        bool temp4 = false;

        foreach (int item in i)
        {
            switch (item)
            {
                case 0:
                    tempRed++;
                    break;
                case 1:
                    tempBlue++;
                    break;
                case 2:
                    tempGreen++;
                    break;
                case 3:
                    tempUltra++;
                    break;
            }
        }
        if (tempRed <= RedCount)
        {
            temp1 = true;
        }
        if (tempBlue <= BlueCount)
        {
            temp2 = true;
        }
        if (tempGreen <= GreenCount)
        {
            temp3 = true;
        }
        if (tempUltra <= UltraCount)
        {
            temp4 = true;
        }

        if (temp1 == true && temp2 == true && temp3 == true && temp4 == true)
        {
            temp = true;
        }
        else
        {
            temp = false;
            if (AvailablePatterns != null && AvailablePatterns.Contains(i) == true)
            {
                AvailablePatterns.Remove(i);
                if (temp1 == false)
                {
                    RemoveFood(FoodColoring.Red);
                }
                if (temp2 == false)
                {
                    RemoveFood(FoodColoring.Blue);
                }
                if (temp3 == false)
                {
                    RemoveFood(FoodColoring.Green);
                }
                if (temp4 == false)
                {
                    RemoveFood(FoodColoring.Ultra);
                }
            }
        }
        return temp;
    }

    /// <summary>
    /// EdgeCase control for when a food count is higher than it should be 
    /// called by CheckTrue() in the case that a pattern is available when it shouldn't be.
    /// Will need to be modified
    /// </summary>
    /// <param name="fc"></param>
    private void RemoveFood(FoodColoring fc)
    {
        switch (fc)
        {
            case FoodColoring.Red:
                if (RedCount>0)
                {
                    AvailableFoodTypes.Remove(FoodColoring.Red);
                    RedCount--;
                }

                break;
            case FoodColoring.Blue:
                if (BlueCount > 0)
                {
                    AvailableFoodTypes.Remove(FoodColoring.Blue);
                    BlueCount--;
                }

                break;
            case FoodColoring.Green:
                if(GreenCount>0)
                {
                    AvailableFoodTypes.Remove(FoodColoring.Green);
                    GreenCount--;
                }
                break;
            case FoodColoring.Ultra:
                if (UltraCount > 0)
                {
                    AvailableFoodTypes.Remove(FoodColoring.Ultra);
                    UltraCount--;
                }
                break;
        }
    }

    #endregion

    #region Event functions

    /// <summary>
    /// Not sure if we are using this RN
    /// </summary>
    /// <param name="NA"></param>
    void PopulateAvailablePatterns(Vector3 NA)
    {
        AvailablePatterns = AvailablePatternsInScene();
    }

    /// <summary>
    /// Spawn method for kibbles, called by OnRayCast and uses events
    /// </summary>
    /// <param name="v"></param>
    public void SpawnSnakeKibble(Vector3 v)
    {
        //if(v == FoodTutStartPos.position)
        //{
        //    clone = Instantiate(SnakeFoodPrefab, v, Quaternion.identity);
        //    FoodTutStartPos = clone.transform;
        //    TestBait = clone.GetComponent<SnakeKibbles>();
        //    //TestBait.transform.position = FoodTutStartPos.position;
        //}
        //else
        //{
        FoodInScene.Add(Instantiate(SnakeFoodPrefab, v, Quaternion.identity).GetComponent<SnakeKibbles>());
        //}
    }

    private void SpawnTutorialFood(Vector3 v)
    {
        clone = Instantiate(SnakeFoodPrefab, v, Quaternion.identity);
        FoodTutStartPos = clone.transform;
        TestBait = clone.GetComponent<SnakeKibbles>();
    }

    private void FoodCount(GameObject go)
    {
        GetFoodCount(go.GetComponent<SnakeKibbles>().TypeofFood);
    }

    private void SnakeSpawned(GameObject go)
    {
        Predators = new GameObject[1] { go };
    }

    public void ClearAllForRoundReset(bool b)
    {
        GameObject snakeKibbleHolder;

        for(int i = FoodInScene.Count; i > 0; i--)
        {
            snakeKibbleHolder = FoodInScene[FoodInScene.Count - 1].gameObject;
            FoodInScene.Remove(FoodInScene[FoodInScene.Count - 1]);
            Destroy(snakeKibbleHolder);
        }

        RedCount = 0;
        BlueCount = 0;
        GreenCount = 0;
        UltraCount = 0;

        Predators[0] = null;
    }

    public void Subscribe()
    {
        EventManager.OnRaycastAction += SpawnSnakeKibble;
        EventManager.OnFoodTypeChoice += FoodCount;
        EventManager.OnFoodDespawn += CleanUp;
        EventManager.OnSnakeSpawn += SnakeSpawned;
        EventManager.OnTutorialFoodSpawn += SpawnTutorialFood;
        EventManager.OnRoundOver += ClearAllForRoundReset;
    }

    public void Unsubscribe()
    {
        EventManager.OnRaycastAction -= SpawnSnakeKibble;
        EventManager.OnFoodTypeChoice -= FoodCount;
        EventManager.OnFoodDespawn -= CleanUp;
        EventManager.OnSnakeSpawn -= SnakeSpawned;
        EventManager.OnTutorialFoodSpawn -= SpawnTutorialFood;
        EventManager.OnRoundOver -= ClearAllForRoundReset;
    }

    #endregion
}
