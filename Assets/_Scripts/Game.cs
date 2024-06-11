using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static IEventHub EventHub { get; private set; }

    public static AssetManager AssetManager { get; private set; }
    public static WindowsManager WindowsManager { get; private set; }
    public static CanvasManager CanvasManager { get; private set; } 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
