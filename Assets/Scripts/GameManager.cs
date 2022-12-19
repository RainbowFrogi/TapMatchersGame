using System.Collections;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] squareType[] squareTypes;

    [SerializeField] GameObject square;

    [SerializeField] ParticleSystem destroyEffect;

    [SerializeField] AudioSource blopSound;



    public int width, height;
    
    //I recommend to keep at least at 10, so the game doesn't feel very slow
    public int dropSpeedMultiplier;

    int highestDrop;
    int currentY;

    Square[,] squareArray;

    [HideInInspector] public bool canClick = true;

    [SerializeField] Camera cam;
    [SerializeField] float boundary;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //Calculate camera size based on the size of the grid, so that the cubes always look the same size
        RecalculateCameraSize();

        //Create the game
        InitGame();
    }

    void RecalculateCameraSize()
    {
        float aspectRatio = (float)cam.pixelHeight / (float)cam.pixelWidth;
        float size = Mathf.Max(width * aspectRatio, height) * 0.5f;

        cam.orthographicSize = size + boundary * 0.5f;
        cam.transform.position = new Vector3(width / 2f - 0.5f, height / 2f - 0.5f, -10f);
    }

    //Creates the game
    void InitGame()
    {
        squareArray = new Square[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject createdSquare = Instantiate(square, new Vector2(x, y), Quaternion.identity);
                int randomColorFromIndex = GetRandomColorIndex();
                createdSquare.GetComponent<SpriteRenderer>().color = squareTypes[randomColorFromIndex].squareColor;
                createdSquare.GetComponent<LocationHandler>().SetXY(x, y);

                squareArray[x, y] = new Square(randomColorFromIndex, createdSquare, true);
            }
        }
    }

    public void ClickMouse(int x, int y)
    {
        if (!canClick || ESCMenu.Instance.inMenu) return;

        blopSound.Play();

        int colorIndex = squareArray[x, y].colorIndex;
        Propagate(x, y, colorIndex);

        CheckIfDrop();

        StartCoroutine(EnableClicking(highestDrop));

        SpawnSquares();


        //If needed, propagate the function to matching blocks
        void Propagate(int x, int y, int colorIndex)
        {
            
            ParticleSystem PS = Instantiate(destroyEffect, squareArray[x, y].square.transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = PS.main;
            main.startColor = squareTypes[colorIndex].squareColor;
            squareArray[x, y].Clicked();            
            Destroy(PS.gameObject, 1.5f);
            
            if(TryClick(x - 1, y, colorIndex)) Propagate(x - 1, y, colorIndex); 
            if(TryClick(x + 1, y, colorIndex)) Propagate(x + 1, y, colorIndex);
            if(TryClick(x, y - 1, colorIndex)) Propagate(x, y - 1, colorIndex);
            if(TryClick(x, y + 1, colorIndex)) Propagate(x, y + 1, colorIndex);
        }
    }

    //Returns a bool, if all the conditions have been fulfilled, so we know that there's another square, where we have to propagate the click function to
    public bool TryClick(int x, int y, int colorIndex)
    {
        return ((x >= 0 && y >= 0 && x < width && y < height) && (squareArray[x, y].hasSquare) && squareArray[x, y].colorIndex == colorIndex);
    }

    //Check if a square doesn't have another square under it
    public void CheckIfDrop()
    {
        highestDrop = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 1; y < height; y++)
            {
                if (!squareArray[x, y].hasSquare) continue;

                if (y - 1 >= 0 && !squareArray[x, y - 1].hasSquare)
                {
                    currentY = y - 1;

                    for (int dropY = y - 1; dropY > 0; dropY--)
                    {
                        if (dropY - 1 >= 0 && !squareArray[x, dropY - 1].hasSquare) currentY--;
                        else break;
                    }

                    int drop = y - currentY;
                    if (drop > highestDrop) highestDrop = drop;

                    ChangeSquarePos(x, y);
                }
            }
        }
    }

    //Spawns the new squares above the map, on the x location of the destroyed ones
    public void SpawnSquares()
    {
        int spawned = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (InBounds(x, y) && !squareArray[x, y].hasSquare)
                {
                    SpawnSquare(x, y);
                    spawned++;
                }
            }
        }

        if (spawned > 0) 
        {
            StopAllCoroutines();
            StartCoroutine(EnableClicking(height + 1f));
        }
        
        //Spawn square in the specific c
        void SpawnSquare(int x, int y)
        {
            Square square = squareArray[x, y];

            square.hasSquare = true;
            square.square = Instantiate(this.square, new Vector2(x, y + height + 1f), Quaternion.identity);

            int colorIndex = GetRandomColorIndex();
            square.colorIndex = colorIndex;
            square.square.GetComponent<SpriteRenderer>().color = squareTypes[colorIndex].squareColor;

            LocationHandler squarehandler = square.square.GetComponent<LocationHandler>();
            squarehandler.SetXY(x, y);
            squarehandler.Respawned();
        }
    }

    
    //Returns true, if the given x and y coordinates are in bounds of the squareArray
    bool InBounds(int x, int y)
    {
        return x < width && y < height && x >= 0 && y >= 0;
    }

    int GetRandomColorIndex() => Random.Range(0, squareTypes.Length);

    //Change the position of square we dropped down
    public void ChangeSquarePos(int x, int y)
    {
        Square square = squareArray[x, y];
        square.hasSquare = false;

        squareArray[x, currentY] = new Square(square.colorIndex, square.square, true);
        square.square.GetComponent<LocationHandler>().WhereToDrop(currentY);
    }

    //Enable clicking after the all the blocks have landed
    IEnumerator EnableClicking(float delay)
    {
        canClick = false;
        yield return new WaitForSeconds(delay / dropSpeedMultiplier);
        canClick = true;
    }
}

public class Square
{
    public int colorIndex;

    public GameObject square;

    public bool hasSquare;


    public Square(int colorIndex, GameObject square, bool hasSquare)
    {
        this.colorIndex = colorIndex;
        this.square = square;
        this.hasSquare = hasSquare;
    }
    
    
    public void Clicked()
    {
        hasSquare = false;
        GameObject.Destroy(square);
        
    }
}

[System.Serializable]
public class squareType
{
    public Color squareColor;
}

