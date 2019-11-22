﻿using UnityEngine;
using System.Collections;
using System;

public struct Tnode {
    public Vector2 pos;
    public float ang;
    public int i_pathT;
    public int iParent_pathT;

    public Tnode(int i,int iP,Vector2 v,float a) {
        i_pathT = i;
        iParent_pathT = iP;
        pos = v;
        ang = a;        
    }
    public Tnode(int i,int iP,Configuration x){
        i_pathT = i;
        iParent_pathT = iP;
        pos = x.Vertice;
        ang = x.Angle;
    }
    public Tnode(Tnode p) {
        i_pathT = p.i_pathT;
        iParent_pathT = p.iParent_pathT;
        pos = p.pos;
        ang = p.ang;        
    }
	// Use this for initialization
    void Start () {
       
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
