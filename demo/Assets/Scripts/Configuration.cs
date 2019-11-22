using UnityEngine;
using System.Collections;

public struct Configuration  {
    public Vector2 Vertice;
    public float Angle;// need to turn to rad

    public Configuration(Vector2 v,float a){
        Vertice = v;
        Angle = a;
    }
    public Configuration(Configuration x){
        Vertice = x.Vertice;
        Angle = x.Angle;
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
