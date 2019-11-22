using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {
    public Vector2 scrollPosition = Vector2.zero;
    void OnGUI() {        
        scrollPosition = GUI.BeginScrollView(new Rect(450, 300, 100, 60), scrollPosition, new Rect(0, 0, 70, 200));        
        if (GUI.Button(new Rect(0, 0, 80, 20), "robot1"))
            print("ok");        
//        GUI.Button(new Rect(90, 0, 80, 20), "obstacle1");
        GUI.Button(new Rect(0, 25, 80, 20), "Bottom-left");
        GUI.Button(new Rect(0, 180, 80, 20), "Bottom-left");        
//        GUI.Button(new Rect(90, 180, 80, 20), "Bottom-right");        
        GUI.EndScrollView();
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
