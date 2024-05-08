using System.Collections;
using System.Collections.Generic;
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

        xSpacing = (float)(width-1) / 2;
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
    }
}
