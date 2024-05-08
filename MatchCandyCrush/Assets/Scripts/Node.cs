using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    /*to determine whether the space can be filled with positions or not
    alanın konumlarla doldurulup doldurulamayacağını belirlemek için*/
    public bool isUsable;

    public GameObject potion;

    public Node(bool _isUsable, GameObject _potion)
    {
        isUsable = _isUsable;
        potion = _potion;
    }   
}
