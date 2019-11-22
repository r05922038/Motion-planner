using UnityEngine;
using System.Collections;

public struct Obstacle {

    //shape
    public int numPolygon;//fixed
    public Polygon[] Polygons;//fixed / move + rotate
	public float height;
   

    //site
    public Configuration initSite;//fixed
    public Vector2 PosInPlanner;//move

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
