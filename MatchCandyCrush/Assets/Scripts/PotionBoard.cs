using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
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

    public List<GameObject> potionToDestroy = new();

    [SerializeField]
    private Potion selectedPotion;
    [SerializeField]
    private bool isProcessingMove;

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
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray=Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit=Physics2D.Raycast(ray.origin,ray.direction);
            if(hit.collider != null && hit.collider.gameObject.GetComponent<Potion>())
            {
                if(isProcessingMove)
                {
                    return;
                }

                Potion potion=hit.collider.gameObject.GetComponent<Potion>();
                UnityEngine.Debug.Log("I have a clicked a potion it is: " + potion.gameObject);

                SelectPotion(potion);
            }
        }
    }

    void InitializeBoard()
    {
        DestroyPotions();

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
                    potionToDestroy.Add(potion);
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

    private void DestroyPotions()
    {
        if(potionToDestroy!=null)
        {
            foreach (GameObject potion in potionToDestroy)
            {
                Destroy(potion);
            }
            potionToDestroy.Clear();
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

    #region Swapping Potions
    //select potion
    public void SelectPotion(Potion _potion)
    {
        //if we don't have pation currently selected, then set the potion i just clicked my to selectedpotion
        if(selectedPotion==null)
        {
            UnityEngine.Debug.Log(_potion);
            selectedPotion = _potion;
        }
        //if we select the same potion twice,then let's make selectedpotion null
        else if (selectedPotion == _potion)
        {
            selectedPotion = null;
        }
        //if selectedpotion is not null and is not the current potion,attempt a swap
        //selectedpotion back to null
        else if (selectedPotion != _potion)
        {
            SwapPotion(selectedPotion,_potion);
            selectedPotion = null;
        }
    }
    //swap potion - logic
    private void SwapPotion(Potion _currentpotion,Potion _targetPotion)
    {
        //!IsAdjacent don't do anything
        if(!IsAdjacent(_currentpotion, _targetPotion))
        {
            return;
        }
        DoSwap(_currentpotion, _targetPotion);

        isProcessingMove = true;

        //startCoroutine ProcessMatches
        StartCoroutine(ProcessMatches(_currentpotion, _targetPotion));
    }
    //do swap 
    private void DoSwap(Potion _currentPotion,Potion _targetPotion)
    {
        GameObject temp = potionBoard[_currentPotion.xIndex,_currentPotion.yIndex].potion;
        potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion = potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion;
        potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion=temp;

        //update indicies
        int tempXIndex = _currentPotion.xIndex;
        int tempYIndex = _currentPotion.yIndex;
        _currentPotion.xIndex = _targetPotion.xIndex;
        _currentPotion.yIndex = _targetPotion.yIndex;
        _targetPotion.xIndex = tempXIndex;
        _targetPotion.yIndex=tempYIndex;

        _currentPotion.MoveToTarget(potionBoard[_targetPotion.xIndex, _targetPotion.yIndex].potion.transform.position);
        _targetPotion.MoveToTarget(potionBoard[_currentPotion.xIndex, _currentPotion.yIndex].potion.transform.position);
    }

    private IEnumerator ProcessMatches(Potion _currentPotion,Potion _targetPotion)
    {
        yield return new WaitForSeconds(0.2f);

        bool hasMatch = CheckBoard();

        if(!hasMatch)
        {
            DoSwap(_currentPotion, _targetPotion);
        }
        isProcessingMove = false;
    }

    //IsAdjacent
    private bool IsAdjacent(Potion _currentpotion,Potion _targetPotion)
    {
        return Mathf.Abs(_currentpotion.xIndex - _targetPotion.xIndex) + Mathf.Abs(_currentpotion.yIndex - _targetPotion.yIndex) == 1;
    }
    //ProcessMatches
    #endregion
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
