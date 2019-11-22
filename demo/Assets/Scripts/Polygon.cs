using UnityEngine;
using System.Collections;

public struct Polygon {
    public int numVertice;
    public Vector2[] Vertices; //fixed
    public Vector2[] plannerIVertices;// move + rotate
    public Vector2[] plannerGVertices;// move + rotate ( robot )

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {	
	}
}
