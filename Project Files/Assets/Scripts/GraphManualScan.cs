using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphManualScan : MonoBehaviour
{
    //Start is called before the first frame update
    void Start()
    {
        AstarPath.active.Scan();
    }
    private void Awake()
    {
        AstarPath.active.Scan();
    }
}
