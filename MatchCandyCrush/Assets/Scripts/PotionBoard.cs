using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PotionBoard : MonoBehaviour
{
    public int width=6;
    public int height=8;
    public float xSpacing;
    public float ySpacing;

    //get a reference to our potion prefabs
    public GameObject[] potionPrefabs;
    //the collection node potionBoard + 60
    private Node[,] potionBoard;
    public GameObject potionBoardGo;

    //layoutArray
    public ArrayLayout arrayLayout;
    //public static of potionBoard
    public static PotionBoard Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        InitializeBoard();
    }
    void InitializeBoard()
    {
        potionBoard = new Node[width, height];

        xSpacing = (float)(width - 1) / 2;
        ySpacing = (float)((height - 1) / 2) + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - xSpacing, y - ySpacing);
                if (arrayLayout.rows[y].row[x])
                {
                    potionBoard[x, y] = new Node(false,null);
                }
                else
                {
                    int randomIndex = Random.Range(0, potionPrefabs.Length);

                    GameObject potion = Instantiate(potionPrefabs[randomIndex], position, Quaternion.identity);
                    potion.GetComponent<Potion>().SetIndicies(x, y);
                    potionBoard[x, y] = new Node(true, potion);
                }
            }
        }
        if(CheckBoard())
        {
            UnityEngine.Debug.Log("We have matches let's re-create the board");
            InitializeBoard();
        }
        else
        {
            UnityEngine.Debug.Log("There are no matches, it's time to start the game!");
        }
    }

    public bool CheckBoard()
    {
        UnityEngine.Debug.Log("Checking Board");
        bool hasMatched = false;

        List<Potion> potionsToRemove = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //checking if potion node is usable
                if (potionBoard[x,y].isUsable)
                {
                    //then proceed to get potion class in node.
                    Potion potion = potionBoard[x,y].potion.GetComponent<Potion>();

                    //ensure its not matched
                    if(!potion.isMatched)
                    {
                        //run some matching logic
                        MatchResult matchedPotions = IsConnected(potion);

                        if (matchedPotions.connectedPotion.Count >= 3)
                        {
                            //comlex matching..

                            potionsToRemove.AddRange(matchedPotions.connectedPotion);

                            foreach (Potion pot in matchedPotions.connectedPotion)
                            {
                                pot.isMatched = true;
                            }
                            hasMatched = true;
                        }
                    }
                }
            }

        }

        return hasMatched;
    }

    //IsConnected
    MatchResult IsConnected(Potion potion)
    {
        List<Potion> connectedPotion = new();
        PotionType potionType = potion.potionType;

        connectedPotion.Add(potion);

        //check right
        CheckDirection(potion, new Vector2Int(1, 0), connectedPotion);
        //check left
        CheckDirection(potion, new Vector2Int(-1, 0), connectedPotion);
        //have we made a 3 match? (Horizontal Match)
        if(connectedPotion.Count == 3)
        {
            UnityEngine.Debug.Log("I have a normal Horizontal match,the color of my match is:" + connectedPotion[0].potionType);

            return new MatchResult
            {
                connectedPotion = connectedPotion,
                direction = MatchDirection.Horizontal
            };
        }
        //checking for more than 3 (Long horizontal Match)
        else if(connectedPotion.Count > 3)
        {
            UnityEngine.Debug.Log("I have a LongHorizontal match,the color of my match is:" + connectedPotion[0].potionType);

            return new MatchResult
            {
                connectedPotion = connectedPotion,
                direction = MatchDirection.LongHorizontal
            };
        }

        //clear out the connectedpotions
        connectedPotion.Clear();
        //readd our initial potion
        connectedPotion.Add(potion);

        //check up
        CheckDirection(potion, new Vector2Int(0, 1), connectedPotion);
        //check down
        CheckDirection(potion, new Vector2Int(0, -1), connectedPotion);

        //have we made a 3 match? (Vertical Match)
        if (connectedPotion.Count == 3)
        {
            UnityEngine.Debug.Log("I have a normal Vertical match,the color of my match is:" + connectedPotion[0].potionType);

            return new MatchResult
            {
                connectedPotion = connectedPotion,
                direction = MatchDirection.Vertical
            };
        }
        //checking for more than 3 (Long Vertical Match)
        else if (connectedPotion.Count > 3)
        {
            UnityEngine.Debug.Log("I have a LongVertical match,the color of my match is:" + connectedPotion[0].potionType);

            return new MatchResult
            {
                connectedPotion = connectedPotion,
                direction = MatchDirection.LongVertical
            };
        }
        else
        {
            return new MatchResult
            {
                connectedPotion = connectedPotion,
                direction = MatchDirection.None
            };
        }
    }

    //CheckDirection
    void CheckDirection(Potion pot,Vector2Int direction,List<Potion> connectedPotion)
    {
        PotionType potionType = pot.potionType;
        int x = pot.xIndex + direction.x;
        int y = pot.yIndex + direction.y;

        //check that we're within the boundaries of the board
        while(x >= 0 && x < width && y >= 0 && y < height)
        {
            if (potionBoard[x,y].isUsable)
            {
                Potion neighbourPotion = potionBoard[x, y].potion.GetComponent<Potion>();

                //does our potionType Match? It must alsý not be matched
                if(!neighbourPotion.isMatched && neighbourPotion.potionType == potionType)
                {
                    connectedPotion.Add(neighbourPotion);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
}

public class MatchResult
{
   public List<Potion> connectedPotion;
   public MatchDirection direction;
}

public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical, 
    LongHorizontal,
    Super,
    None
}
