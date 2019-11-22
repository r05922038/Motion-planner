using UnityEngine;
using System.Collections;

public struct Robot{

    //shape
    public int numPolygon;//fixed 
    public Polygon[] Polygons;//fixed / move + rotate

    //site
    public Configuration initSite;//move + rotate
    public Configuration goalSite;//move + rotate

    //potential value
    public int numControlpt;//fixed
    public Vector2[] Controlpts;//fixed

	// Use this for initialization
	void Start () {

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
