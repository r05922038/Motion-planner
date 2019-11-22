using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
public class World_screen2 : MonoBehaviour {
    /* data_structure */
	public int numRobot,numObstacle,numAddO,totalNumAddo,frequenceLowerO,threeTimes,maxAddO,scoreNPC,scoreUser,rNumPolygon;
    public Robot[] Robots;
    public Obstacle[] Obstacles;
    public Wall Walls;
	public bool inMouse,delayByO,fromBFS,testBFS,lockMoveO,fromMoveR,fromRotateR;
	public bool[] ifOnceObstacle;
	public float aniSpeed,sizeOfObstacle,maxFootHeight,timerPenalty;
	public Configuration[] goal;
    /* draw_polygon*/
    FutileParams fp;          
    WTPolygonSprite[][] sIRPolygons,sGRPolygons,sOPolygons,sGoal;
    WTPolygonSprite sWall;
    public static float x_PlannerToCanvas,y_PlannerToCanvas;
//	public static float canvasToPlanner;
	public static float x_canvasToPlanner, y_canvasToPlanner;
    /*bitmap*/
    public int[,,] Bitmap;
    /*collision*/
	public Vector2[][] Me,Parent,Me_O;
	public Vector2[][][] Others,Others_O,plannerGoal;
	public Vector2[] MyBounder,MyBounder_O,ParentsBounder;
	public Vector2[][] OthersBounder,OthersBounder_O;
	/*bfs*/
    bool ifShowPath,ifAniPath,ifAniCo;
//    WTPolygonSprite[][][] sPATH;
    Tnode[] pathT_arr;
    int[] robotPath;
    WTPolygonSprite[] aniRobot;
    /*temp*/
    public Vector3 curMouse,mouse_p;
    public int thRobot,thObstacle,IorG,RorO;
    public Vector2[] toIPrint,toGPrint,toOPrint,toGoal;
    /*GUI*/
    public Vector2 scrollPosition = Vector2.zero;
    public string robotFileName,obstacleFileName;
    // Use this for initialization

	void destroySprite (){
		/*robots*/
		for (int a=0; a<numRobot; a++){
			for (int b=0; b<Robots[a].numPolygon; b++){
				Futile.stage.RemoveChild(sIRPolygons [a][b]);
//				Futile.stage.RemoveChild(sGRPolygons [a][b]);
			}
		}
		for (int a=0; a<3; a++) {
						for (int b=0; b<Robots[0].numPolygon; b++) {
								Futile.stage.RemoveChild (sGoal [a] [b]);
						}
				}

		/*obstacles*/
		for (int a=0; a<numObstacle; a++){
			for (int b=0; b<Obstacles[a].numPolygon; b++){
				Futile.stage.RemoveChild(sOPolygons[a][b]);
			}
		}
		/*walls*/
//		Futile.stage.RemoveChild(sWall);
//		Futile.stage.RemoveChild(sWall);
	}


	void Start () {
        /*draw*/
		fp = new FutileParams(true, true, false, false);
		fp.AddResolutionLevel(1024f, 1.0f, 1.0f, "");//-res1
//		fp.backgroundColor = Color.white;
        fp.origin = Vector2.zero;
		Futile.instance.Init(fp);
//		x_PlannerToCanvas = (Futile.screen.halfWidth * 2) / Screen.width * 540 / 128;
//		y_PlannerToCanvas = (Futile.screen.halfHeight * 2) / Screen.height * 540 / 128;
//        canvasToPlanner = 128f / 540f;
		x_PlannerToCanvas = (Futile.screen.halfWidth * 2) / 128;
		y_PlannerToCanvas = (Futile.screen.halfHeight * 2) / 128;
		x_canvasToPlanner = 128f / Screen.width;
		y_canvasToPlanner = 128f / Screen.height;

        /*initialize*/
        robotFileName="/mouse1.txt";
        obstacleFileName = "/map1.txt";

/*		Vector2[] background = new Vector2[]{
			new Vector2 (0, 0),
			new Vector2 (0, 128),
			new Vector2 (128, 128),
			new Vector2 (128, 0),
		};
		for (int c=0; c<4; c++)
			background [c].Set(background[c].x *x_PlannerToCanvas,background[c].y*y_PlannerToCanvas);
		WTPolygonSprite back = new WTPolygonSprite (new WTPolygonData (background));
		back.color =Color.white;
		Futile.stage.AddChild(back);*/

		goal = new Configuration[3];
		goal [0] = new Configuration (new Vector2 (80,20),60f);
		goal [1] = new Configuration (new Vector2 (105, 80), 10f);
		goal[2]=new Configuration(new Vector2 (50, 60), 120f);

		Futile.atlasManager.LoadImage("b2");
		FSprite fSprite = new FSprite("b2");		
		fSprite.x = Futile.screen.halfWidth;
		fSprite.y = Futile.screen.halfHeight;
		Futile.stage.AddChild(fSprite);

		reset();

	}
	public void reset(){
        /*initialization*/
        ReadFile();
		rNumPolygon = Robots [0].numPolygon - 2;
        inMouse = false;
        if (Screen.width > Screen.height)
            Print();
        buildBitmap();
//        ifShowPath = false;
        ifAniPath= false;
		ifAniCo=false;
		delayByO=false;
		fromBFS = false;
		testBFS = true;
		lockMoveO = false;
		fromMoveR= false;
		fromRotateR= false;
		maxAddO=8;
		numAddO = 0;
		totalNumAddo = 0;
		frequenceLowerO = 3;
		threeTimes = 0;
		scoreNPC = 0;
		scoreUser = 0;
		aniSpeed = 55f;
		sizeOfObstacle = 3.2f;
		maxFootHeight = 30f;
		timerPenalty = 1f;
		obstacleInitialPartOthers ();
    }  
	void timer15s(){
		timerPenalty *= 0.85f;
		if (sizeOfObstacle <= 5.6 ) {
			sizeOfObstacle += 0.4f;
		} else if (sizeOfObstacle < 6) {
			sizeOfObstacle = 6f;
		}
	}
	GUIStyle myGUI=new GUIStyle();
	void OnGUI() {
		if (!ifAniPath) {
			if (GUI.Button (new Rect (0, Screen.height - (14.1f/128f*Screen.height), 14.1f/128f*Screen.width, 14.1f/128f*Screen.height), "GO")) {
				getBitmap ();
				if ((testBFS = BFS ())) {
					InvokeRepeating ("timer15s", 7f, 7f);
					StartCoroutine ("animation",(1f / aniSpeed));
//					StartCoroutine (animation (1f / aniSpeed));
				}
			}
			if (GUI.Button (new Rect (Screen.width*7f/8f+5, Screen.height - 50, Screen.width/8f-5, 50), "Reset")) {
				destroySprite ();
				reset ();
			}			
			aniSpeed = GUI.HorizontalSlider (new Rect (Screen.width-110, 10, 100, 20), aniSpeed, 30.0F, 120.0F);
			maxAddO = (int)GUI.HorizontalSlider (new Rect (Screen.width-110, 30, 100, 20), maxAddO, 1.0F, 20.0F);
			sizeOfObstacle = GUI.HorizontalSlider (new Rect (Screen.width-110, 50, 100, 20), sizeOfObstacle, 1.5F, 6.0F);
			maxFootHeight = GUI.HorizontalSlider (new Rect (Screen.width-110, 70, 100, 20), maxFootHeight, 10.0F, 50.0F);
			frequenceLowerO = (int)GUI.HorizontalSlider (new Rect (Screen.width-110, 90, 100, 20), frequenceLowerO, 1.0F, 10.0F);
			
			myGUI.normal.textColor=Color.white;
			myGUI.fontStyle=FontStyle.Normal;
			myGUI.fontSize = 15;			
			GUI.Label (new Rect (Screen.width-350, 5, 200, 20), "NPC speed (sec/step)：1/" + (int)aniSpeed, myGUI);  
			GUI.Label (new Rect (Screen.width-350, 25, 200, 20), "Maximum # of obstacle：" + (int)maxAddO, myGUI); 
			GUI.Label (new Rect (Screen.width-350, 45, 200, 20), "Size of obstacle：" + sizeOfObstacle.ToString ("0.0") + " X " + sizeOfObstacle.ToString ("0.0"), myGUI); 
			GUI.Label (new Rect (Screen.width-350, 65, 200, 20), "Step height of NPC (cm)：" + (int)maxFootHeight, myGUI);
			GUI.Label (new Rect (Screen.width-350, 85, 200, 20), "Frequency of lower obstacles：1/" + (int)frequenceLowerO, myGUI);

			myGUI.fontSize = 100;
			myGUI.normal.textColor=Color.yellow;
			myGUI.fontStyle=FontStyle.BoldAndItalic;
			if(scoreNPC>scoreUser)
				GUI.Label (new Rect (Screen.width/4, Screen.height/2-30, Screen.width/2, 60), "You lose",myGUI);
			if(scoreNPC<scoreUser)
				GUI.Label (new Rect (Screen.width/4, Screen.height/2-30, Screen.width/2, 60), "You win",myGUI);
		} else {
			if (GUI.Button (new Rect (Screen.width*7f/8f+5, Screen.height - 50, Screen.width/8f-5, 50),"Stop")){
				StopCoroutine("animation");
				ifAniCo=false;
				ifAniPath = false;
				CancelInvoke("timer15s");
			}

			myGUI.fontSize = 20;
			myGUI.normal.textColor=Color.white;
			myGUI.fontStyle=FontStyle.Bold;
			GUI.Label (new Rect (Screen.width-150, 10, 100, 20), "NPC score：" + scoreNPC,myGUI);
			GUI.Label (new Rect (Screen.width-150, 30, 100, 20), "your score：" +scoreUser,myGUI);
		}

		/*		if (GUI.Button (new Rect (450, Screen.height - 350, 100, 30), "Push Obstacle")) {
			addObstacle ();
		}*/
		/*        if (GUI.Button(new Rect(450, Screen.height-400, 100, 30), "Show Path")){
            if (!inMouse){
                if(ifShowPath)
                    clearPATH();
                inMouse = true;
                if(!lastBitmap)
                    getBitmap();
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
                sw.Reset();//碼表歸零
                sw.Start();//碼表開始計時
                BFS();
                sw.Stop();//碼錶停止                
                print("total time for BFS_and_ShowPath (sec) : "+sw.Elapsed.TotalMilliseconds/1000);
                lastBitmap= true;
                inMouse = false;
            }
        }        
        if (GUI.Button(new Rect(450, Screen.height-250, 100, 30), "Animation")){
            if(!inMouse && ifAniPath)
            StartCoroutine (animation (1f / 30f));
        }    */      
		
	}
/*	IEnumerator task(float timeinseconds){
		yield return new WaitForSeconds(timeinseconds);
		while (!ifTask) {
			yield return new WaitForSeconds(timeinseconds);
			getBitmap ();
			if (BFS ())
				StartCoroutine (animation (1f / 30f));
		}
	}*/
	void addObstacle (){
		totalNumAddo++;
		if (numAddO == maxAddO)
						cutObstacle ();
		numAddO++;
		Obstacle[] tempO = Obstacles;
		Obstacles = new Obstacle[numObstacle + 1];
		for (int i=0; i<numObstacle; i++)
			Obstacles [i] = tempO [i];
		Obstacles [numObstacle].numPolygon = 1;
		Obstacles [numObstacle].Polygons = new Polygon[1];
		Obstacles [numObstacle].Polygons [0] = new Polygon ();
		Obstacles [numObstacle].Polygons [0].numVertice = 4;
		Obstacles [numObstacle].Polygons [0].Vertices = new Vector2[]{
			new Vector2 (-sizeOfObstacle, sizeOfObstacle),
			new Vector2 (sizeOfObstacle, sizeOfObstacle),
			new Vector2 (sizeOfObstacle,-sizeOfObstacle),
			new Vector2 (-sizeOfObstacle, -sizeOfObstacle),
		};
		Obstacles [numObstacle].initSite = new Configuration (new Vector2 (7, 7), 0f);
		Obstacles [numObstacle].PosInPlanner = new Vector2 (7, 7);
		Obstacles [numObstacle].Polygons [0].plannerIVertices = new Vector2[4];
		Obstacles [numObstacle].height = ((totalNumAddo%frequenceLowerO)==0)?20f:40f;
		for (int i=0; i<4; i++)
			Obstacles [numObstacle].Polygons [0].plannerIVertices [i] = RotateBy (Obstacles [numObstacle].Polygons [0].Vertices [i], Obstacles [numObstacle].initSite.Angle) + Obstacles [numObstacle].initSite.Vertice;
		WTPolygonSprite[][] tempSO = sOPolygons;
		sOPolygons = new WTPolygonSprite[numObstacle + 1][];
		for (int i=0; i<numObstacle; i++)
			sOPolygons [i] = tempSO [i];
		sOPolygons [numObstacle] = new WTPolygonSprite[1];
		toOPrint = new Vector2[4];
		for (int c=0; c<4; c++)
			toOPrint [c].Set (Obstacles [numObstacle].Polygons [0].plannerIVertices [c].x * x_PlannerToCanvas, Obstacles [numObstacle].Polygons [0].plannerIVertices [c].y * y_PlannerToCanvas);
		sOPolygons [numObstacle] [0] = new WTPolygonSprite (new WTPolygonData (toOPrint));
		sOPolygons [numObstacle] [0].color = ((totalNumAddo%frequenceLowerO)==0)?Color.magenta:Color.red;
		Futile.stage.AddChild (sOPolygons [numObstacle] [0]);
		bool[] tempIf = ifOnceObstacle;
		ifOnceObstacle = new bool[numObstacle + 1];
		for (int i=0; i<numObstacle; i++)
						ifOnceObstacle [i] = tempIf [i];
		ifOnceObstacle [numObstacle] = false;
		numObstacle++;
	}
	void cutObstacle (){
		Obstacle[] tempO = Obstacles;
		Obstacles = new Obstacle[numObstacle - 1];
		for (int i=0; i<numObstacle-numAddO; i++)
			Obstacles [i] = tempO [i];
		for (int i=numObstacle-numAddO+1; i<numObstacle; i++)
			Obstacles [i-1] = tempO [i];
		WTPolygonSprite[][] tempSO = sOPolygons;
		sOPolygons = new WTPolygonSprite[numObstacle - 1][];
		for (int i=0; i<numObstacle-numAddO; i++)
			sOPolygons [i] = tempSO [i];
		for (int i=numObstacle-numAddO+1; i<numObstacle; i++)
			sOPolygons [i-1] = tempSO [i];
		Futile.stage.RemoveChild (tempSO [numObstacle - numAddO] [0]);
		bool[] tempIf = ifOnceObstacle;
		ifOnceObstacle = new bool[numObstacle - 1];
		for (int i=0; i<numObstacle-numAddO; i++)
			ifOnceObstacle[i] = tempIf [i];
		for (int i=numObstacle-numAddO+1; i<numObstacle; i++)
			ifOnceObstacle[i-1] = tempIf [i];
		numObstacle--;
		numAddO--;
	}

    public void ReadFile(){
        /*robot*/
        string path = Application.dataPath + robotFileName;
        if (!File.Exists(path))
            return;
        StreamReader sr = File.OpenText (path);
        string input = ""; 
        string[] nums;
        input = sr.ReadLine ();// # number of robots
        input = sr.ReadLine ();
        numRobot = int.Parse (input);
        Robots = new Robot[numRobot];
        for(int i=0;i<numRobot;i++)
            Robots[i]=new Robot();
        for (int a=0; a<numRobot; a++) {
            input = sr.ReadLine ();// # robots # (number)
            input = sr.ReadLine ();// # number of polygons
            input = sr.ReadLine ();
            Robots [a].numPolygon = int.Parse(input);
            Robots [a].Polygons = new Polygon[ Robots [a].numPolygon ];
            for(int i=0;i<Robots [a].numPolygon;i++)
                Robots [a].Polygons[i]=new Polygon();
            for (int b=0; b<Robots[a].numPolygon; b++) {
                input = sr.ReadLine ();// # polygons # (number)
                input = sr.ReadLine ();// # number of vertices
                input = sr.ReadLine ();
                Robots [a].Polygons [b].numVertice = int.Parse (input);
                Robots [a].Polygons [b].Vertices = new Vector2[ Robots [a].Polygons [b].numVertice ];
                Robots [a].Polygons [b].plannerIVertices= new Vector2[ Robots [a].Polygons [b].numVertice ];
                Robots [a].Polygons [b].plannerGVertices= new Vector2[ Robots [a].Polygons [b].numVertice ];
                input = sr.ReadLine ();// # vertices
                for (int c=0; c<Robots[a].Polygons[b].numVertice; c++) {
                    input = sr.ReadLine ();
                    nums = input.Split (' '); 
                    Robots [a].Polygons [b].Vertices [c] = new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1]));
                }
            }
            input = sr.ReadLine ();// # initial configuration
            input = sr.ReadLine ();
            nums = input.Split (' ');
            Robots [a].initSite= new Configuration(new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1])),Normalize(Single.Parse (nums [2])));
            input = sr.ReadLine ();// # goal configuration
            input = sr.ReadLine ();
            nums = input.Split (' ');
            Robots [a].goalSite= new Configuration(new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1])),Normalize(Single.Parse (nums [2])));
            input = sr.ReadLine ();// # number of control points
            input = sr.ReadLine ();
            Robots [a].numControlpt = int.Parse (input);
            Robots [a].Controlpts = new Vector2 [Robots [a].numControlpt];
            for (int b=0; b<Robots[a].numControlpt; b++) {
                input = sr.ReadLine ();// # control point # (number)
                input = sr.ReadLine ();
                nums = input.Split (' ');
                Robots [a].Controlpts [b] = new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1]));
            }
        }
        sr.Close(); 
        /*obstacle*/
        path = Application.dataPath + obstacleFileName;
        if (!File.Exists(path))
            return;
        sr = File.OpenText(path);
        input = sr.ReadLine();// # number of obstacles
        input = sr.ReadLine();
        numObstacle = int.Parse(input);
		ifOnceObstacle=new bool[numObstacle];
		for (int i=0; i<numObstacle; i++)
						ifOnceObstacle [i] = true;
        Obstacles = new Obstacle[numObstacle];
        for(int i=0;i<numObstacle;i++)
            Obstacles[i]=new Obstacle();
        for (int a=0; a<numObstacle; a++){
			Obstacles [a].height=100f;
            input = sr.ReadLine();// # obstacles # (number)
            input = sr.ReadLine();// # number of polygons
            input = sr.ReadLine();
            Obstacles [a].numPolygon = int.Parse(input);
            Obstacles [a].Polygons = new Polygon[ Obstacles [a].numPolygon ];
            for(int i=0;i<Obstacles [a].numPolygon;i++)
                Obstacles [a].Polygons[i]=new Polygon();
            for (int b=0; b<Obstacles[a].numPolygon; b++){
                input = sr.ReadLine();// # polygons # (number)
                input = sr.ReadLine();// # number of vertices
                input = sr.ReadLine();
                Obstacles [a].Polygons [b].numVertice = int.Parse(input);
                Obstacles [a].Polygons [b].Vertices = new Vector2[ Obstacles [a].Polygons [b].numVertice ];
                Obstacles [a].Polygons [b].plannerIVertices= new Vector2[ Obstacles [a].Polygons [b].numVertice ];
                input = sr.ReadLine();// # vertices
                for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){
                    input = sr.ReadLine();
                    nums = input.Split(' '); 
                    Obstacles [a].Polygons [b].Vertices [c].Set(Single.Parse (nums [0]),Single.Parse (nums [1]));
                }
            }
            input = sr.ReadLine();// # initial configuration
            input = sr.ReadLine();
            nums = input.Split(' ');
            Obstacles [a].initSite= new Configuration(new Vector2(Single.Parse (nums [0]),Single.Parse (nums [1])),Single.Parse(nums [2]));
            Obstacles [a].PosInPlanner= new Vector2(Obstacles [a].initSite.Vertice.x,Obstacles [a].initSite.Vertice.y);
        }
        sr.Close();
    }   
    public void setPlanner(){
        /*robots*/
        for (int a=0; a<numRobot; a++) {
            for (int b=0; b<Robots[a].numPolygon; b++){
                for (int c=0; c<Robots[a].Polygons[b].numVertice; c++){
                    Robots [a].Polygons [b].plannerIVertices [c] = RotateBy(Robots [a].Polygons [b].Vertices [c], Robots [a].initSite.Angle)+Robots [a].initSite.Vertice;
                    Robots [a].Polygons [b].plannerGVertices[c]= RotateBy(Robots [a].Polygons [b].Vertices [c], Robots [a].goalSite.Angle)+Robots [a].goalSite.Vertice;
                }
            }
        }
        /*obstacles*/
        for (int a=0; a<numObstacle; a++){
            for (int b=0; b<Obstacles[a].numPolygon; b++){
                for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++)
                    Obstacles [a].Polygons [b].plannerIVertices [c] = RotateBy(Obstacles [a].Polygons [b].Vertices [c], Obstacles [a].initSite.Angle)+Obstacles [a].initSite.Vertice;
            }
        }
    }
    public void setSprite(){
        /*robots*/
        sIRPolygons = new WTPolygonSprite[numRobot][];
        sGRPolygons = new WTPolygonSprite[numRobot][];
		sGoal=new WTPolygonSprite[3][];
        for (int a=0; a<numRobot; a++){
            sIRPolygons [a] = new WTPolygonSprite[Robots [a].numPolygon];
            sGRPolygons [a] = new WTPolygonSprite[Robots [a].numPolygon];
        }
		for (int a=0; a<3; a++)
			sGoal [a] = new WTPolygonSprite[Robots [0].numPolygon];
        /*obstacles*/
        sOPolygons = new WTPolygonSprite[numObstacle][];
        for (int a=0; a<numObstacle; a++)
            sOPolygons [a] = new WTPolygonSprite[Obstacles [a].numPolygon];
    }
    public void buildBitmap(){
        Bitmap = new int[Robots [0].numControlpt, 128, 128];    
    }
    public void Print (){
		setPlanner();
		setSprite();
        Walls = new Wall();
        /*robots*/
        for (int a=0; a<numRobot; a++){
            for (int b=0; b<Robots[0].numPolygon; b++){
                toIPrint = new Vector2[Robots [a].Polygons [b].numVertice];
//                toGPrint= new Vector2[Robots [a].Polygons [b].numVertice];
                for (int c=0; c<Robots[a].Polygons[b].numVertice; c++){
                    toIPrint [c].Set(Robots [a].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas,Robots [a].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas);
//                    toGPrint [c].Set(Robots [a].Polygons [b].plannerGVertices [c].x * x_PlannerToCanvas,Robots [a].Polygons [b].plannerGVertices [c].y * y_PlannerToCanvas);
                }
                sIRPolygons [a][b] = new WTPolygonSprite(new WTPolygonData(toIPrint));

				if(b==0){
					sIRPolygons [a][b].color =Color.blue;
				}
				else if(b>=rNumPolygon){
					sIRPolygons [a][b].color =Color.clear;
				}
				else{
					sIRPolygons [a][b].color = Color.cyan;
				}
                Futile.stage.AddChild(sIRPolygons [a][b]);
//                sGRPolygons [a][b] = new WTPolygonSprite(new WTPolygonData(toGPrint));
//                sGRPolygons [a][b].color = Color.white;
//                Futile.stage.AddChild(sGRPolygons [a][b]);
            }
        }
		plannerGoal=new Vector2[3][][];

		for (int a=0; a<3; a++) {
			plannerGoal[a]=new Vector2[Robots[0].numPolygon][];
			for (int b=0; b<Robots[0].numPolygon; b++) {
				plannerGoal[a][b]=new Vector2[Robots [0].Polygons [b].numVertice];
				toGoal = new Vector2[Robots [0].Polygons [b].numVertice];
				for (int c=0; c<Robots[0].Polygons[b].numVertice; c++) {
					plannerGoal[a][b][c] = RotateBy (Robots [0].Polygons [b].Vertices [c], goal [a].Angle) + goal [a].Vertice;
					toGoal [c].x = plannerGoal[a][b][c].x*x_PlannerToCanvas;
					toGoal [c].y = plannerGoal[a][b][c].y*y_PlannerToCanvas;
				}
				sGoal [a] [b] = new WTPolygonSprite (new WTPolygonData (toGoal));

				if(a==0 && b==rNumPolygon){
					sGoal [a] [b].color = Color.green;
				}
				else if(a==0){
					sGoal [a] [b].color =Color.white;//yellow
				}
				else{
					sGoal [a] [b].color =Color.gray;
				}

				Futile.stage.AddChild (sGoal [a] [b]);
			}
		}

        /*obstacles*/
        for (int a=0; a<numObstacle; a++){
            for (int b=0; b<Obstacles[a].numPolygon; b++){
                toOPrint = new Vector2[Obstacles [a].Polygons [b].numVertice];
                for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++)
                    toOPrint [c].Set(Obstacles [a].Polygons [b].plannerIVertices [c].x *x_PlannerToCanvas,Obstacles [a].Polygons [b].plannerIVertices [c].y*y_PlannerToCanvas);
                sOPolygons[a][b] = new WTPolygonSprite(new WTPolygonData(toOPrint));
				sOPolygons[a][b].color = Color.black;//black,white
                Futile.stage.AddChild(sOPolygons[a][b]);
            }
        }
        /*walls*/
/*        toIPrint = new Vector2[4];
        Walls.right = new Vector2[]{
            new Vector2(128, -1),
            new Vector2(128, (128 + 1)),
            new Vector2((128+1), (128 + 1) ),
            new Vector2((128+1), -1),
        };
        for (int i =0; i<4; i++)
            toIPrint [i].Set(Walls.right[i].x * x_PlannerToCanvas,Walls.right[i].y * y_PlannerToCanvas);
        sWall = new WTPolygonSprite(new WTPolygonData(toIPrint));
		sWall .color = Color.black;
		Futile.stage.AddChild(sWall);
        toIPrint = new Vector2[4];
        Walls.top = new Vector2[] {
            new Vector2(0 ,(128 + 1)),
            new Vector2(128, (128 + 1)),
            new Vector2(128, 127.9f ),
            new Vector2(0 , 127.9f ),
        };
        for (int i =0; i<4; i++)
            toIPrint [i].Set(Walls.top [i].x * x_PlannerToCanvas, Walls.top [i].y * y_PlannerToCanvas);
        sWall = new WTPolygonSprite(new WTPolygonData(toIPrint));
        sWall.color = Color.black;
        Futile.stage.AddChild(sWall);*/
    }
	public Vector2 RotateBy (Vector2 v,float angle){
        angle*= Mathf.Deg2Rad;
        var ca = Mathf.Cos(angle);// var -> float
        var sa = Mathf.Sin(angle);
        return new Vector2( (float)(v.x * ca - v.y * sa), (float)(v.x * sa + v.y * ca));
    }              
    // Update is called once per frame
    void Update () { 
        GUItest();
    }
    public void GUItest(){
		if ((!fromBFS)&&(testBFS&&((!inMouse)&&(ifAniPath&&Input.GetKeyDown ("space"))))) {
//		if ((!fromBFS)&&(((!inMouse)&&(Input.GetKeyDown ("space"))))) {
						addObstacle ();
				}
		if ((!fromBFS)&&(!inMouse&&Input.GetMouseButtonDown (0))) {
            curMouse = Input.mousePosition;
            RorO=inRegion ();
            if ((RorO==1&&!ifAniPath)&&(IorG==1)) {                 
                StartCoroutine (moveRobot (1f / 10000000000000f));//by IorG
            }
            else if(RorO==2&&!ifOnceObstacle[thObstacle]){
				StartCoroutine ("moveObstacle", (1f / 10000000000000f));
			}
		}
		else if((!fromBFS)&&(!inMouse&&Input.GetMouseButtonDown (1))) {
            curMouse = Input.mousePosition;
            RorO=inRegion ();
			if ((RorO==1&&!ifAniPath)&&(IorG==1)){
                ifAniPath= false;
                StartCoroutine (rotateRobot (1f /10000000000000f));
            }
/*            else if(RorO==2){
                ifAniPath= false;
                StartCoroutine (rotateObstacle (1f / 10000000000000f));
            }*/
        }
    }
    public int inRegion(){
        Vector2 basis=new Vector2();
        float angle;
        curMouse.x = curMouse.x*x_canvasToPlanner;
        curMouse.y =curMouse.y*y_canvasToPlanner;

		/*search obstacles*/
		for (int a=numObstacle-1; a>=numObstacle-numAddO ; a--){
			if(ifOnceObstacle[a])
				continue;
			for (int b=0; b<Obstacles[a].numPolygon; b++){
				basis.Set(Obstacles [a].Polygons [b].plannerIVertices [Obstacles [a].Polygons [b].numVertice - 1].x - curMouse.x, Obstacles [a].Polygons [b].plannerIVertices [Obstacles [a].Polygons [b].numVertice - 1].y - curMouse.y);
				angle = 0;
				for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){
					angle += Vector2.Angle(new Vector2(Obstacles [a].Polygons [b].plannerIVertices [c].x - curMouse.x, Obstacles [a].Polygons [b].plannerIVertices [c].y - curMouse.y), basis);
					basis.Set(Obstacles [a].Polygons [b].plannerIVertices [c].x - curMouse.x, Obstacles [a].Polygons [b].plannerIVertices [c].y - curMouse.y);
				}
				if (Mathf.Abs(angle - 360) < 1f){
					thObstacle = a;
					return 2;
				}
			}
		}
        /*search robots*/
        for(int a=0;a<numRobot;a++){
            for(int b=0;b<rNumPolygon;b++){

                /*init robot*/
                basis.Set(Robots[a].Polygons[b].plannerIVertices[Robots[a].Polygons[b].numVertice-1].x - curMouse.x, Robots[a].Polygons[b].plannerIVertices[Robots[a].Polygons[b].numVertice-1].y - curMouse.y);
                angle = 0;
                for(int c=0;c<Robots[a].Polygons[b].numVertice;c++){
                    angle += Vector2.Angle (new Vector2(Robots[a].Polygons[b].plannerIVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerIVertices[c].y-curMouse.y), basis);
                    basis.Set (Robots[a].Polygons[b].plannerIVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerIVertices[c].y-curMouse.y);
                }
                if (Mathf.Abs(angle - 360)<1f) {
                    IorG=1;
                    thRobot=a;
                    return 1;
                }
                /*goal robot*/
/*                basis.Set(Robots[a].Polygons[b].plannerGVertices[Robots[a].Polygons[b].numVertice-1].x - curMouse.x, Robots[a].Polygons[b].plannerGVertices[Robots[a].Polygons[b].numVertice-1].y - curMouse.y);
                angle = 0;
                for(int c=0;c<Robots[a].Polygons[b].numVertice;c++){
                    angle += Vector2.Angle (new Vector2(Robots[a].Polygons[b].plannerGVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerGVertices[c].y-curMouse.y), basis);
                    basis.Set (Robots[a].Polygons[b].plannerGVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerGVertices[c].y-curMouse.y);
                }
                if (Mathf.Abs(angle - 360)<1f) {
                    IorG=2;
                    thRobot=a;
                    return 1;
                }*/
            }
        }        
        return 0;//do nothing
    }
    public void robotBuildOthers(){
		OthersBounder = new Vector2[(fromBFS&&inMouse)?thObstacle:numObstacle][];
		Others = new Vector2[(fromBFS&&inMouse)?thObstacle:numObstacle][][];
        int i_obstacle,i_polygon,i_vertice;
		for (i_obstacle=0; i_obstacle<OthersBounder.Length; i_obstacle++){
            OthersBounder [i_obstacle] = new Vector2[]{
                new Vector2(129, 129),
                new Vector2(-1, -1),
            };
            Others[i_obstacle]=new Vector2[Obstacles[i_obstacle].numPolygon][];
            for (i_polygon=0; i_polygon < Obstacles[i_obstacle].numPolygon ; i_polygon++){
                Others [i_obstacle][i_polygon] = Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices;
                for (i_vertice=0; i_vertice<Obstacles [i_obstacle].Polygons [i_polygon].numVertice; i_vertice++){
                    if (OthersBounder [i_obstacle] [0].x > Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].x)
                        OthersBounder [i_obstacle] [0].x = Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].x;
                    if (OthersBounder [i_obstacle] [0].y > Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].y)
                        OthersBounder [i_obstacle] [0].y = Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].y;
                    if (OthersBounder [i_obstacle] [1].x < Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].x)
                        OthersBounder [i_obstacle] [1].x = Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].x;
                    if (OthersBounder [i_obstacle] [1].y < Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].y)
                        OthersBounder [i_obstacle] [1].y = Obstacles [i_obstacle].Polygons [i_polygon].plannerIVertices [i_vertice].y;
                }
            }
        }
    }
    public void robotBuildMe(Configuration t){
            MyBounder [0].Set(129, 129);
            MyBounder [1].Set(-1, -1);
            for (int a=0; a<Me.Length; a++){
                for (int b=0; b<Me[a].Length; b++){
                    Me [a] [b] = RotateBy(Robots [0].Polygons [a].Vertices [b], t.Angle) + t.Vertice;
                    if (MyBounder [0].x > Me [a] [b].x)
                        MyBounder [0].x = Me [a] [b].x;
                    if (MyBounder [0].y > Me [a] [b].y)
                        MyBounder [0].y = Me [a] [b].y;
                    if (MyBounder [1].x < Me [a] [b].x)
                        MyBounder [1].x = Me [a] [b].x;
                    if (MyBounder [1].y < Me [a] [b].y)
                        MyBounder [1].y = Me [a] [b].y;
                }
            }
    }
    public void robotBuildMe(Vector2 v,float angle){
        MyBounder [0].Set(129, 129);
        MyBounder [1].Set(-1, -1);
        for (int a=0; a<Me.Length; a++){
            for (int b=0; b<Me[a].Length; b++){
                Me [a] [b] = RotateBy(Robots [0].Polygons [a].Vertices [b], angle) + v;
                if (MyBounder [0].x > Me [a] [b].x)
                    MyBounder [0].x = Me [a] [b].x;
                if (MyBounder [0].y > Me [a] [b].y)
                    MyBounder [0].y = Me [a] [b].y;
                if (MyBounder [1].x < Me [a] [b].x)
                    MyBounder [1].x = Me [a] [b].x;
                if (MyBounder [1].y < Me [a] [b].y)
                    MyBounder [1].y = Me [a] [b].y;
            }
        }
    }
    public void robotBuildMe(int x,int y){    
        for (int a=0; a<Parent.Length; a++){
            for (int b=0; b<Parent[a].Length; b++){
                Me [a] [b].x = x+Parent [a] [b].x;
                Me [a] [b].y = y+Parent [a] [b].y;
            }
        }
        MyBounder [0].x = x+ParentsBounder [0].x;
        MyBounder [0].y = y+ParentsBounder [0].y;
        MyBounder [1].x = x+ParentsBounder [1].x;
        MyBounder [1].y = y+ParentsBounder [1].y;
    }
    public void robotBuildMe_x(int x){    
        for (int a=0; a<Parent.Length; a++){
            for (int b=0; b<Parent[a].Length; b++)
                Me [a] [b].x = x+Parent [a] [b].x;
        }
        MyBounder [0].x = x+ParentsBounder [0].x;
        MyBounder [1].x = x+ParentsBounder [1].x;
    }       
    public void robotBuildParent(Vector2 v,float an){
        ParentsBounder [0].Set(129, 129);
        ParentsBounder [1].Set(-1, -1);
        for (int a=0; a<Parent.Length; a++){
            for (int b=0; b<Parent[a].Length; b++){
                Parent [a] [b] = RotateBy(Robots [0].Polygons [a].Vertices [b], an) + v;
                if (ParentsBounder [0].x > Parent [a] [b].x)
                    ParentsBounder [0].x = Parent [a] [b].x;
                if (ParentsBounder [0].y > Parent [a] [b].y)
                    ParentsBounder [0].y = Parent [a] [b].y;
                if (ParentsBounder [1].x < Parent [a] [b].x)
                    ParentsBounder [1].x = Parent [a] [b].x;
                if (ParentsBounder [1].y < Parent [a] [b].y)
                    ParentsBounder [1].y = Parent [a] [b].y;
            }
        }
    }
	public bool collision_O (){
		//wall
		if(MyBounder_O [0].x<0 || MyBounder_O [0].y<0)
			return true;
		if(MyBounder_O [1].x>=128 || MyBounder_O [1].y>=128)
			return true;
		//others
		int i_others,i_polygon,i_vertice,i1, i2;
		
		Vector2 v1, v3, v122, v344;
		bool detection;
		for(i_others=0; i_others < OthersBounder_O.Length; i_others++){
			detection=false;
			//Me,Others
			if (MyBounder_O [0].x > OthersBounder_O [i_others] [0].x && MyBounder_O [0].x < OthersBounder_O [i_others] [1].x){
				if (MyBounder_O [0].y > OthersBounder_O [i_others] [0].y && MyBounder_O [0].y < OthersBounder_O [i_others] [1].y){
					detection = true;
				}
				else if(MyBounder_O [1].y > OthersBounder_O [i_others] [0].y && MyBounder_O [1].y < OthersBounder_O [i_others] [1].y){
					detection = true;
				}            
			}
			if (!detection){
				if (MyBounder_O [1].x > OthersBounder_O [i_others] [0].x && MyBounder_O [1].x < OthersBounder_O [i_others] [1].x){
					if (MyBounder_O [0].y > OthersBounder_O [i_others] [0].y && MyBounder_O [0].y < OthersBounder_O [i_others] [1].y){
						detection = true;
					}
					else if (MyBounder_O [1].y > OthersBounder_O [i_others] [0].y && MyBounder_O [1].y < OthersBounder_O [i_others] [1].y){
						detection = true;
					}
				}
			}
			//Others<Me
			if (!detection){
				if(OthersBounder_O [i_others] [0].x > MyBounder_O [0].x && OthersBounder_O [i_others] [0].x < MyBounder_O [1].x){
					if(OthersBounder_O [i_others] [0].y > MyBounder_O [0].y && OthersBounder_O [i_others] [0].y < MyBounder_O [1].y){
						detection = true;
					}
					else if(OthersBounder_O [i_others] [1].y > MyBounder_O [0].y && OthersBounder_O [i_others] [1].y < MyBounder_O [1].y){
						detection = true;
					}
				}
			}
			if (!detection){
				if(OthersBounder_O [i_others] [1].x > MyBounder_O [0].x && OthersBounder_O [i_others] [1].x < MyBounder_O [1].x){
					if(OthersBounder_O [i_others] [0].y > MyBounder_O [0].y && OthersBounder_O [i_others] [0].y < MyBounder_O [1].y){
						detection = true;
					}
					else if(OthersBounder_O [i_others] [1].y > MyBounder_O [0].y && OthersBounder_O [i_others] [1].y < MyBounder_O [1].y){
						detection = true;
					}
				}
			}
			
			if (detection){
				for (i1=0; i1<Me_O.Length; i1++){
					for (i2=0; i2<Me_O[i1].Length; i2++){
						v1 = ((i2 == 0) ? Me_O [i1] [Me_O [i1].Length - 1] : Me_O [i1] [i2 - 1]);
						for (i_polygon=0; i_polygon< ((i_others==0)?rNumPolygon:Others_O[i_others].Length); i_polygon++){
							for (i_vertice=0; i_vertice<Others_O[i_others][i_polygon].Length; i_vertice++){
								v3 = ((i_vertice == 0) ? Others_O[i_others][i_polygon][Others_O[i_others][i_polygon].Length - 1] : Others_O[i_others][i_polygon][i_vertice - 1]);
								v122 = RotateBy(Me_O [i1] [i2] - v1, 90.0f);
								v344 = RotateBy(Others_O[i_others][i_polygon][i_vertice] - v3, 90.0f);
								if ((Vector2.Dot(v122, v3 - v1) * Vector2.Dot(v122, Others_O[i_others][i_polygon][i_vertice] - v1) < 0 && Vector2.Dot(v344, v1 - v3) * Vector2.Dot(v344, Me_O [i1] [i2] - v3) < 0))
									return true;
							}
						}
					}
				}
			}
		}
		return false;
	}
	public int i_others;
	public bool collision (){
		//wall
		if (MyBounder [0].x < 0 || MyBounder [0].y < 0)
						return true;
		if (MyBounder [1].x >= 128 || MyBounder [1].y >= 128)
						return true;
		//others
		int i_polygon,i_vertice,i1, i2;
		
		Vector2 v1, v3, v122, v344;
		bool detection;
		int lowerBound = ((fromBFS||fromMoveR||fromRotateR) ? 0 : numObstacle - numAddO - 1);
//		for(i_others=0, detection=false ; i_others < OthersBounder.Length; i_others++, detection=false){
		for(i_others=OthersBounder.Length-1; i_others >=lowerBound; i_others--){
			detection=false;
			if(fromBFS&&(Obstacles[i_others].height<=maxFootHeight))
				continue;
			//Me,Others
			if (MyBounder [0].x >= OthersBounder [i_others] [0].x && MyBounder [0].x <= OthersBounder [i_others] [1].x){
				if (MyBounder [0].y >= OthersBounder [i_others] [0].y && MyBounder [0].y <= OthersBounder [i_others] [1].y){
					detection = true;
				}
				else if(MyBounder [1].y >= OthersBounder [i_others] [0].y && MyBounder [1].y <= OthersBounder [i_others] [1].y){
					detection = true;
				}            
			}
			if (!detection){
				if (MyBounder [1].x >= OthersBounder [i_others] [0].x && MyBounder [1].x <= OthersBounder [i_others] [1].x){
                    if (MyBounder [0].y >= OthersBounder [i_others] [0].y && MyBounder [0].y <= OthersBounder [i_others] [1].y){
                        detection = true;
                    }
                    else if (MyBounder [1].y >= OthersBounder [i_others] [0].y && MyBounder [1].y <= OthersBounder [i_others] [1].y){
                        detection = true;
                    }
                }
            }
            //Others<Me
            if (!detection){
                if(OthersBounder [i_others] [0].x >= MyBounder [0].x && OthersBounder [i_others] [0].x <= MyBounder [1].x){
                    if(OthersBounder [i_others] [0].y >= MyBounder [0].y && OthersBounder [i_others] [0].y <= MyBounder [1].y){
                        detection = true;
                    }
                    else if(OthersBounder [i_others] [1].y >= MyBounder [0].y && OthersBounder [i_others] [1].y <= MyBounder [1].y){
                        detection = true;
                    }
                }
            }
            if (!detection){
                if(OthersBounder [i_others] [1].x >= MyBounder [0].x && OthersBounder [i_others] [1].x <= MyBounder [1].x){
                    if(OthersBounder [i_others] [0].y >= MyBounder [0].y && OthersBounder [i_others] [0].y <= MyBounder [1].y){
                        detection = true;
                    }
                    else if(OthersBounder [i_others] [1].y >= MyBounder [0].y && OthersBounder [i_others] [1].y <= MyBounder [1].y){
                        detection = true;
                    }
                }
            }

            if (detection){
				for (i1=0;i1<Me.Length; i1++){
					for (i2=0;i2<Me[i1].Length; i2++){
                        v1 = ((i2 == 0) ? Me [i1] [Me [i1].Length - 1] : Me [i1] [i2 - 1]);
						for (i_polygon=0;i_polygon<Others[i_others].Length; i_polygon++){
							for (i_vertice=0;i_vertice<Others[i_others][i_polygon].Length; i_vertice++){
                                v3 = ((i_vertice == 0) ? Others[i_others][i_polygon][Others[i_others][i_polygon].Length - 1] : Others[i_others][i_polygon][i_vertice - 1]);
                                v122 = RotateBy(Me [i1] [i2] - v1, 90.0f);
                                v344 = RotateBy(Others[i_others][i_polygon][i_vertice] - v3, 90.0f);
                                if ((Vector2.Dot(v122, v3 - v1) * Vector2.Dot(v122, Others[i_others][i_polygon][i_vertice] - v1) < 0 && Vector2.Dot(v344, v1 - v3) * Vector2.Dot(v344, Me [i1] [i2] - v3) < 0)){
									if(!fromBFS&&(Obstacles[i_others].height<=maxFootHeight)){
										delayByO=true;
									}
									else{
                                    	return true;
									}
								}
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
    IEnumerator moveRobot(float timeinseconds){ 
        inMouse = true;
		fromMoveR = true;
		Color temp;
		Vector2 origCenter = new Vector2 ();
        //Me,MyBounder,Others
        Me=new Vector2[rNumPolygon][];
		for(int i=0;i<rNumPolygon;i++)
            Me[i]=new Vector2[Robots[thRobot].Polygons[i].numVertice];
        MyBounder = new Vector2[2];
        robotBuildOthers();
        yield return new WaitForSeconds(timeinseconds);
        if (IorG == 1){
            Vector2 def_c = new Vector2 (curMouse.x-Robots[thRobot].initSite.Vertice.x,curMouse.y-Robots[thRobot].initSite.Vertice.y);
            mouse_p = Input.mousePosition; 
            mouse_p.x = mouse_p.x*x_canvasToPlanner;
            mouse_p.y= mouse_p.y*y_canvasToPlanner;
            origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
            //Me,MyBounder
            robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
            if(collision()){
                while (true){
                    mouse_p = Input.mousePosition; 
                    mouse_p.x = mouse_p.x*x_canvasToPlanner;
                    mouse_p.y= mouse_p.y*y_canvasToPlanner;
                    origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
                    //Me,MyBounder
                    robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
                    if(!collision())
                        break;
                    Robots[thRobot].initSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
					for(int b=0;b<rNumPolygon;b++){
						temp=sIRPolygons[thRobot][b].color;
                        Futile.stage.RemoveChild(sIRPolygons[thRobot][b]);
                        toIPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                        for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                            Robots[thRobot].Polygons[b].plannerIVertices[c]=Robots[thRobot].Polygons[b].plannerIVertices[c]-origCenter+Robots[thRobot].initSite.Vertice;
                            toIPrint [c].x=Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                            toIPrint [c].y=Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                        }
                        sIRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toIPrint));
                        sIRPolygons[thRobot][b].color= temp;
                        Futile.stage.AddChild(sIRPolygons[thRobot][b]);
                    } 
                    yield return new WaitForSeconds(timeinseconds);
                }
            }
            while (true){
                mouse_p = Input.mousePosition; 
                mouse_p.x = mouse_p.x*x_canvasToPlanner;
                mouse_p.y= mouse_p.y*y_canvasToPlanner;
                origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
                //Me,MyBounder
                robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
                if(collision())
                    break;
                Robots[thRobot].initSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
				for(int b=0;b<rNumPolygon;b++){
					temp=sIRPolygons[thRobot][b].color;
                    Futile.stage.RemoveChild(sIRPolygons[thRobot][b]);
                    toIPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                    for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                        Robots[thRobot].Polygons[b].plannerIVertices[c]=Robots[thRobot].Polygons[b].plannerIVertices[c]-origCenter+Robots[thRobot].initSite.Vertice;
                        toIPrint [c].x=Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y=Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sIRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sIRPolygons[thRobot][b].color= temp;
                    Futile.stage.AddChild(sIRPolygons[thRobot][b]);
                } 
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(0))
                    break;
            }
        } 
/*        else{     
            Vector2 def_c = new Vector2 (curMouse.x-Robots[thRobot].goalSite.Vertice.x,curMouse.y-Robots[thRobot].goalSite.Vertice.y);
            mouse_p = Input.mousePosition; 
            mouse_p.x = mouse_p.x*x_canvasToPlanner;
            mouse_p.y= mouse_p.y*y_canvasToPlanner;
            origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
            robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
            if(collision()){
                while (true){
                    mouse_p = Input.mousePosition; 
                    mouse_p.x = mouse_p.x*x_canvasToPlanner;
                    mouse_p.y= mouse_p.y*y_canvasToPlanner;
                    origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
                    robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
                    if(!collision())
                        break;
                    Robots[thRobot].goalSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
					for(int b=0;b<rNumPolygon;b++){
                        Futile.stage.RemoveChild(sGRPolygons[thRobot][b]);
                        toGPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                        for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                            Robots[thRobot].Polygons[b].plannerGVertices[c]=Robots[thRobot].Polygons[b].plannerGVertices[c]-origCenter+Robots[thRobot].goalSite.Vertice;
                            toGPrint [c].x=Robots [thRobot].Polygons [b].plannerGVertices [c].x *x_PlannerToCanvas;
                            toGPrint [c].y=Robots [thRobot].Polygons [b].plannerGVertices [c].y *y_PlannerToCanvas;
                        }
                        sGRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toGPrint));
                        sGRPolygons[thRobot][b].color= Color.blue;
                        Futile.stage.AddChild(sGRPolygons[thRobot][b]);
                    }                
                    yield return new WaitForSeconds(timeinseconds);
                }
            }
            while (true){
                mouse_p = Input.mousePosition; 
                mouse_p.x = mouse_p.x*x_canvasToPlanner;
                mouse_p.y= mouse_p.y*y_canvasToPlanner;
                origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
                robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
                if(collision())
                    break;
                Robots[thRobot].goalSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
				for(int b=0;b<rNumPolygon;b++){
					temp=sGRPolygons[thRobot][b].color;
                    Futile.stage.RemoveChild(sGRPolygons[thRobot][b]);
                    toGPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                    for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                        Robots[thRobot].Polygons[b].plannerGVertices[c]=Robots[thRobot].Polygons[b].plannerGVertices[c]-origCenter+Robots[thRobot].goalSite.Vertice;
                        toGPrint [c].x=Robots [thRobot].Polygons [b].plannerGVertices [c].x *x_PlannerToCanvas;
                        toGPrint [c].y=Robots [thRobot].Polygons [b].plannerGVertices [c].y *y_PlannerToCanvas;
                    }
                    sGRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toGPrint));
                    sGRPolygons[thRobot][b].color= temp;
                    Futile.stage.AddChild(sGRPolygons[thRobot][b]);
                }                
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(0))
                    break;
            }
        }*/
		fromMoveR = false;
        inMouse = false;
    }
	public void obstacleInitialPartOthers(){
		Me_O = new Vector2[1][];
		Me_O [0] = new Vector2[4];
		MyBounder_O = new Vector2[2];
		Others_O = new Vector2[4][][];
		OthersBounder_O = new Vector2[4][];
		for (int i=0; i<4; i++) {
			Others_O [i] = new Vector2[Robots [0].numPolygon][];
			OthersBounder_O[i] = new Vector2[]{
				new Vector2(129, 129),
				new Vector2(-1, -1),
			};
			for (int a=0; a<Robots[0].numPolygon; a++){
				Others_O [i][a] =(i==0)?Robots [0].Polygons [a].plannerIVertices:plannerGoal[i-1][a];
				for(int b=0;b<Robots[0].Polygons[a].numVertice;b++){
					if(OthersBounder_O[i][0].x > ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x))
						OthersBounder_O[i][0].x = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x);
					if(OthersBounder_O[i][0].y > ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y))
						OthersBounder_O[i][0].y = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y);
					if(OthersBounder_O[i][1].x < ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x))
						OthersBounder_O[i][1].x = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x);
					if(OthersBounder_O[i][1].y < ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y))
						OthersBounder_O[i][1].y = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y);
				}
			}
		}
		
	}
	IEnumerator moveObstacle(float timeinseconds){
		inMouse = true;
		ifOnceObstacle [thObstacle] = true;
		Vector2 origCenter=new Vector2();
		Vector2 def_c = new Vector2 (curMouse.x-Obstacles[thObstacle].PosInPlanner.x,curMouse.y-Obstacles[thObstacle].PosInPlanner.y);
		Color color=sOPolygons [thObstacle] [0].color;
		//Me,MyBounder
//		Me_O = new Vector2[Obstacles[thObstacle].numPolygon][];
//		for (int i=0; i<Obstacles[thObstacle].numPolygon; i++)
//			Me_O [i] = new Vector2[Obstacles [thObstacle].Polygons [i].numVertice];
//		MyBounder_O = new Vector2[2];
		//obstacleBuildOthers
//		Others_O = new Vector2[4][][];
//		OthersBounder_O = new Vector2[4][];
//		for (int i=0; i<4; i++) {
//				Others_O [i] = new Vector2[Robots [0].numPolygon][];
//				OthersBounder_O [i] = new Vector2[2];
//		}
		yield return new WaitForSeconds(timeinseconds);
		while (inMouse) {
			mouse_p = Input.mousePosition; 
			mouse_p.x = mouse_p.x * x_canvasToPlanner;
			mouse_p.y = mouse_p.y * y_canvasToPlanner;
			origCenter.Set (Obstacles [thObstacle].PosInPlanner.x, Obstacles [thObstacle].PosInPlanner.y);
			//obstacleBuildMe
			MyBounder_O [0].Set (129, 129);
			MyBounder_O [1].Set (-5, -5);
			for (int b=0; b<Obstacles[thObstacle].numPolygon; b++) {
				for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++) {
					Me_O [b] [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + new Vector2 (mouse_p.x - def_c.x, mouse_p.y - def_c.y);
					if (MyBounder_O [0].x > Me_O [b] [c].x)
						MyBounder_O [0].x = Me_O [b] [c].x;
					if (MyBounder_O[0].y > Me_O [b] [c].y)
						MyBounder_O [0].y = Me_O [b] [c].y;
					if (MyBounder_O [1].x < Me_O [b] [c].x)
						MyBounder_O [1].x = Me_O [b] [c].x;
					if (MyBounder_O [1].y < Me_O [b] [c].y)
						MyBounder_O [1].y = Me_O [b] [c].y;
				}
			}
/*			for(int i=0; i<4; i++) {
				OthersBounder_O[i] = new Vector2[]{
					new Vector2(129, 129),
					new Vector2(-1, -1),
				};
				for (int a=0; a<Robots[0].numPolygon; a++){
					Others_O [i][a] =(i==0)?Robots [0].Polygons [a].plannerIVertices:plannerGoal[i-1][a];
					for(int b=0;b<Robots[0].Polygons[a].numVertice;b++){
						if(OthersBounder_O[i][0].x > ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x))
							OthersBounder_O[i][0].x = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x);
						if(OthersBounder_O[i][0].y > ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y))
							OthersBounder_O[i][0].y = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y);
						if(OthersBounder_O[i][1].x < ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x))
							OthersBounder_O[i][1].x = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].x:plannerGoal[i-1][a][b].x);
						if(OthersBounder_O[i][1].y < ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y))
							OthersBounder_O[i][1].y = ((i==0)?Robots [0].Polygons [a].plannerIVertices[b].y:plannerGoal[i-1][a][b].y);
					}
				}
			}*/
			OthersBounder_O[0] = new Vector2[]{
				new Vector2(129, 129),
				new Vector2(-1, -1),
			};
			for (int a=0; a<rNumPolygon; a++){
				Others_O [0][a] =Robots [0].Polygons [a].plannerIVertices;
				for(int b=0;b<Robots[0].Polygons[a].numVertice;b++){
					if(OthersBounder_O[0][0].x > Robots [0].Polygons [a].plannerIVertices[b].x)
						OthersBounder_O[0][0].x = Robots [0].Polygons [a].plannerIVertices[b].x;
					if(OthersBounder_O[0][0].y > Robots [0].Polygons [a].plannerIVertices[b].y)
						OthersBounder_O[0][0].y = Robots [0].Polygons [a].plannerIVertices[b].y;
					if(OthersBounder_O[0][1].x < Robots [0].Polygons [a].plannerIVertices[b].x)
						OthersBounder_O[0][1].x = Robots [0].Polygons [a].plannerIVertices[b].x;
					if(OthersBounder_O[0][1].y < Robots [0].Polygons [a].plannerIVertices[b].y)
						OthersBounder_O[0][1].y = Robots [0].Polygons [a].plannerIVertices[b].y;
				}
			}
			
			/*			for (int a=0; a<Robots[0].numPolygon; a++){
				Others_O [0][a] = Robots [0].Polygons [a].plannerGVertices;
				Others_O [1][a] = Robots [0].Polygons [a].plannerIVertices;
				for(int b=0;b<Robots[0].Polygons[a].numVertice;b++){
					if(OthersBounder_O[0][0].x > Robots [0].Polygons [a].plannerGVertices[b].x)
						OthersBounder_O[0][0].x = Robots [0].Polygons [a].plannerGVertices[b].x;
					if(OthersBounder_O[0][0].y > Robots [0].Polygons [a].plannerGVertices[b].y)
						OthersBounder_O[0][0].y = Robots [0].Polygons [a].plannerGVertices[b].y;
					if(OthersBounder_O[0][1].x < Robots [0].Polygons [a].plannerGVertices[b].x)
						OthersBounder_O[0][1].x = Robots [0].Polygons [a].plannerGVertices[b].x;
					if(OthersBounder_O[0][1].y < Robots [0].Polygons [a].plannerGVertices[b].y)
						OthersBounder_O[0][1].y = Robots [0].Polygons [a].plannerGVertices[b].y;
						
					if(OthersBounder_O[1][0].x > Robots [0].Polygons [a].plannerIVertices[b].x)
						OthersBounder_O[1][0].x = Robots [0].Polygons [a].plannerIVertices[b].x;
					if(OthersBounder_O[1][0].y > Robots [0].Polygons [a].plannerIVertices[b].y)
						OthersBounder_O[1][0].y = Robots [0].Polygons [a].plannerIVertices[b].y;
					if(OthersBounder_O[1][1].x < Robots [0].Polygons [a].plannerIVertices[b].x)
						OthersBounder_O[1][1].x = Robots [0].Polygons [a].plannerIVertices[b].x;
					if(OthersBounder_O[1][1].y < Robots [0].Polygons [a].plannerIVertices[b].y)
						OthersBounder_O[1][1].y = Robots [0].Polygons [a].plannerIVertices[b].y;
				}
			}*/
			if (collision_O ())
				break;
			lockMoveO = true;
			Obstacles [thObstacle].PosInPlanner.Set (mouse_p.x - def_c.x, mouse_p.y - def_c.y); 
			for (int b=0; b<Obstacles[thObstacle].numPolygon; b++) {
				Futile.stage.RemoveChild (sOPolygons [thObstacle] [b]);
				toOPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
				for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++) {
					Obstacles [thObstacle].Polygons [b].plannerIVertices [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + Obstacles [thObstacle].PosInPlanner;
					toOPrint [c].x = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
					toOPrint [c].y = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
				}
				sOPolygons [thObstacle] [b] = new WTPolygonSprite (new WTPolygonData (toOPrint));
				sOPolygons [thObstacle] [b].color = color;
				Futile.stage.AddChild (sOPolygons [thObstacle] [b]);
			}	
			lockMoveO = false;
//			robotBuildOthers();
			yield return new WaitForSeconds (timeinseconds);
			if (Input.GetMouseButtonDown (0))
				break;
		}
//		robotBuildOthers();
		inMouse = false;
	}    
	public float Normalize(float angle){//output : 0.0 ~ 359.x
		while (angle<0)
			angle += 360;
		while (angle>=360)
			angle -= 360;
		return angle;
	}
	IEnumerator rotateRobot(float timeinseconds){  
		inMouse = true;
		fromRotateR=true;
		Color temp;
		float angle,totalAngle;
		Vector2 to=new Vector2();
		Vector2 from=new Vector2();
		Vector3 bef =Input.mousePosition;
		bef.x = bef.x *x_canvasToPlanner;
		bef.y = bef.y *y_canvasToPlanner;
		totalAngle = 0;
		Me=new Vector2[rNumPolygon][];
		for(int i=0;i<rNumPolygon;i++)
			Me[i]=new Vector2[Robots[thRobot].Polygons[i].numVertice];
		MyBounder = new Vector2[2];
		robotBuildOthers();
		if (IorG == 1){
            yield return new WaitForSeconds(timeinseconds);            
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * x_canvasToPlanner;
                mouse_p.y = mouse_p.y * y_canvasToPlanner;              
                to.Set(mouse_p.x - Robots [thRobot].initSite.Vertice.x, mouse_p.y - Robots [thRobot].initSite.Vertice.y);
                from.Set(bef.x - Robots [thRobot].initSite.Vertice.x, bef.y - Robots [thRobot].initSite.Vertice.y);
                angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg;
                robotBuildMe(new Configuration(Robots [0].initSite.Vertice, Robots [0].initSite.Angle + totalAngle + angle));
                if (collision())
                {
                    Robots [thRobot].initSite.Angle = Normalize(Robots [thRobot].initSite.Angle + totalAngle);
                    break;
                }
                totalAngle += angle;
				for (int b=0; b<rNumPolygon; b++)
                {
                    toIPrint = new Vector2[Robots [thRobot].Polygons [b].numVertice];
					temp=sIRPolygons [thRobot] [b].color;
                    Futile.stage.RemoveChild(sIRPolygons [thRobot] [b]);
                    for (int c=0; c<Robots[thRobot].Polygons[b].numVertice; c++)
                    {
                        Robots [thRobot].Polygons [b].plannerIVertices [c] = RotateBy(Robots [thRobot].Polygons [b].plannerIVertices [c] - Robots [thRobot].initSite.Vertice, angle) + Robots [thRobot].initSite.Vertice;
                        toIPrint [c].x = Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sIRPolygons [thRobot] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sIRPolygons [thRobot] [b].color = temp;
                    Futile.stage.AddChild(sIRPolygons [thRobot] [b]);
                }
                bef.x = mouse_p.x;
                bef.y = mouse_p.y;
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(1))
                {
                    Robots [thRobot].initSite.Angle = Normalize(Robots [thRobot].initSite.Angle + totalAngle);
                    break;
                }           
            }
        } 
/*		else{
            yield return new WaitForSeconds(timeinseconds);            
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * x_canvasToPlanner;
                mouse_p.y = mouse_p.y * y_canvasToPlanner;
                to.Set(mouse_p.x - Robots [thRobot].goalSite.Vertice.x, mouse_p.y - Robots [thRobot].goalSite.Vertice.y);
                from.Set(bef.x - Robots [thRobot].goalSite.Vertice.x, bef.y - Robots [thRobot].goalSite.Vertice.y);
                angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg; 
                robotBuildMe(new Configuration(Robots [0].goalSite.Vertice, Robots [0].goalSite.Angle + totalAngle + angle));
                if (collision())
                {
                    Robots [thRobot].goalSite.Angle = Normalize(Robots [thRobot].goalSite.Angle + totalAngle);
                    break;
                }
                totalAngle += angle;
				for (int b=0; b<rNumPolygon; b++)
                {
                    toGPrint = new Vector2[Robots [thRobot].Polygons [b].numVertice];
					temp=sGRPolygons [thRobot] [b].color;
                    Futile.stage.RemoveChild(sGRPolygons [thRobot] [b]);
                    for (int c=0; c<Robots[thRobot].Polygons[b].numVertice; c++)
                    {
                        Robots [thRobot].Polygons [b].plannerGVertices [c] = RotateBy(Robots [thRobot].Polygons [b].plannerGVertices [c] - Robots [thRobot].goalSite.Vertice, angle) + Robots [thRobot].goalSite.Vertice;
                        toGPrint [c].x = Robots [thRobot].Polygons [b].plannerGVertices [c].x * x_PlannerToCanvas;
                        toGPrint [c].y = Robots [thRobot].Polygons [b].plannerGVertices [c].y * y_PlannerToCanvas;
                    }
                    sGRPolygons [thRobot] [b] = new WTPolygonSprite(new WTPolygonData(toGPrint));
                    sGRPolygons [thRobot] [b].color = temp;
                    Futile.stage.AddChild(sGRPolygons [thRobot] [b]);
                }
                bef.x = mouse_p.x;
                bef.y = mouse_p.y;
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(1))
                {
                    Robots [thRobot].goalSite.Angle = Normalize(Robots [thRobot].goalSite.Angle + totalAngle);
                    break;
                }           
            }
        }*/
		fromRotateR=false;
        inMouse = false;        
    }
/*    IEnumerator rotateObstacle(float timeinseconds){ 
        inMouse = true;
        float angle;
        Vector2 to=new Vector2();
        Vector2 from=new Vector2();        
        Vector3 bef =Input.mousePosition;
        bef.x = bef.x *canvasToPlanner;
        bef.y = bef.y*canvasToPlanner;
        //Me,MyBounder
        Me = new Vector2[Obstacles[thObstacle].numPolygon][];
        for (int i=0; i<Obstacles[thObstacle].numPolygon; i++)
            Me [i] = new Vector2[Obstacles [thObstacle].Polygons [i].numVertice];
        MyBounder = new Vector2[]{
            new Vector2(129, 129),
            new Vector2(-1, -1),
        };
        //obstacleBuildOthers
        Others = new Vector2[2][][];
        Others[0]=new Vector2[Robots[0].numPolygon][];
        Others[1]=new Vector2[Robots[0].numPolygon][];
        OthersBounder = new Vector2[2][];
        OthersBounder[0] = new Vector2[]{
            new Vector2(129, 129),
            new Vector2(-1, -1),
        };
        OthersBounder[1] = new Vector2[]{
            new Vector2(129, 129),
            new Vector2(-1, -1),
        };
        for (int a=0; a<Robots[0].numPolygon; a++){
            Others [0][a] = Robots [0].Polygons [a].plannerGVertices;
            Others [1][a] = Robots [0].Polygons [a].plannerIVertices;
            for(int b=0;b<Robots[0].Polygons[a].numVertice;b++){
                if(OthersBounder[0][0].x > Robots [0].Polygons [a].plannerGVertices[b].x)
                    OthersBounder[0][0].x = Robots [0].Polygons [a].plannerGVertices[b].x;
                if(OthersBounder[0][0].y > Robots [0].Polygons [a].plannerGVertices[b].y)
                    OthersBounder[0][0].y = Robots [0].Polygons [a].plannerGVertices[b].y;
                if(OthersBounder[0][1].x < Robots [0].Polygons [a].plannerGVertices[b].x)
                    OthersBounder[0][1].x = Robots [0].Polygons [a].plannerGVertices[b].x;
                if(OthersBounder[0][1].y < Robots [0].Polygons [a].plannerGVertices[b].y)
                    OthersBounder[0][1].y = Robots [0].Polygons [a].plannerGVertices[b].y;
                if(OthersBounder[1][0].x > Robots [0].Polygons [a].plannerIVertices[b].x)
                    OthersBounder[1][0].x = Robots [0].Polygons [a].plannerIVertices[b].x;
                if(OthersBounder[1][0].y > Robots [0].Polygons [a].plannerIVertices[b].y)
                    OthersBounder[1][0].y = Robots [0].Polygons [a].plannerIVertices[b].y;
                if(OthersBounder[1][1].x < Robots [0].Polygons [a].plannerIVertices[b].x)
                    OthersBounder[1][1].x = Robots [0].Polygons [a].plannerIVertices[b].x;
                if(OthersBounder[1][1].y < Robots [0].Polygons [a].plannerIVertices[b].y)
                    OthersBounder[1][1].y = Robots [0].Polygons [a].plannerIVertices[b].y;
            }
        }
        yield return new WaitForSeconds(timeinseconds);  

		Color color=sOPolygons [thObstacle] [0].color;
		mouse_p = Input.mousePosition;
        mouse_p.x = mouse_p.x*canvasToPlanner;
        mouse_p.y= mouse_p.y*canvasToPlanner;
        to.Set(mouse_p.x - Obstacles [thObstacle].PosInPlanner.x, mouse_p.y - Obstacles [thObstacle].PosInPlanner.y);
        from.Set(bef.x - Obstacles [thObstacle].PosInPlanner.x, bef.y - Obstacles [thObstacle].PosInPlanner.y);
        angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg; 
        MyBounder[0].Set(129, 129);
        MyBounder[1].Set(-1, -1);
        for (int b=0; b<Obstacles[thObstacle].numPolygon; b++){
            for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++){
                Me[b][c]=RotateBy(Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - Obstacles [thObstacle].PosInPlanner, angle) + Obstacles [thObstacle].PosInPlanner;
                if(MyBounder[0].x>Me[b][c].x)
                    MyBounder[0].x=Me[b][c].x;
                if(MyBounder[0].y>Me[b][c].y)
                    MyBounder[0].y=Me[b][c].y;
                if(MyBounder[1].x<Me[b][c].x)
                    MyBounder[1].x=Me[b][c].x;
                if(MyBounder[1].y<Me[b][c].y)
                    MyBounder[1].y=Me[b][c].y;
            }
        }
        bool ifCo = false;
        if (collision())
        {            
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * canvasToPlanner;
                mouse_p.y = mouse_p.y * canvasToPlanner;
                to.Set(mouse_p.x - Obstacles [thObstacle].PosInPlanner.x, mouse_p.y - Obstacles [thObstacle].PosInPlanner.y);
                from.Set(bef.x - Obstacles [thObstacle].PosInPlanner.x, bef.y - Obstacles [thObstacle].PosInPlanner.y);
                angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg; 
                MyBounder [0].Set(129, 129);
                MyBounder [1].Set(-1, -1);
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Me [b] [c] = RotateBy(Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - Obstacles [thObstacle].PosInPlanner, angle) + Obstacles [thObstacle].PosInPlanner;
                        if (MyBounder [0].x > Me [b] [c].x)
                            MyBounder [0].x = Me [b] [c].x;
                        if (MyBounder [0].y > Me [b] [c].y)
                            MyBounder [0].y = Me [b] [c].y;
                        if (MyBounder [1].x < Me [b] [c].x)
                            MyBounder [1].x = Me [b] [c].x;
                        if (MyBounder [1].y < Me [b] [c].y)
                            MyBounder [1].y = Me [b] [c].y;
                    }
                }
                if (!collision())
                    break;

                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    toIPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
                    Futile.stage.RemoveChild(sOPolygons [thObstacle] [b]);
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Obstacles [thObstacle].Polygons [b].plannerIVertices [c] = RotateBy(Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - Obstacles [thObstacle].PosInPlanner, angle) + Obstacles [thObstacle].PosInPlanner;
                        toIPrint [c].x = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sOPolygons [thObstacle] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sOPolygons [thObstacle] [b].color = color;
                    Futile.stage.AddChild(sOPolygons [thObstacle] [b]);
                }
                bef.x = mouse_p.x;
                bef.y = mouse_p.y;
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(1)){
                    ifCo=true;
                    break;
                }
            }
        }
        if (!ifCo)
        {
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * canvasToPlanner;
                mouse_p.y = mouse_p.y * canvasToPlanner;
                to.Set(mouse_p.x - Obstacles [thObstacle].PosInPlanner.x, mouse_p.y - Obstacles [thObstacle].PosInPlanner.y);
                from.Set(bef.x - Obstacles [thObstacle].PosInPlanner.x, bef.y - Obstacles [thObstacle].PosInPlanner.y);
                angle = Mathf.Atan2(to.y, to.x) * Mathf.Rad2Deg - Mathf.Atan2(from.y, from.x) * Mathf.Rad2Deg; 
                MyBounder [0].Set(129, 129);
                MyBounder [1].Set(-1, -1);
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Me [b] [c] = RotateBy(Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - Obstacles [thObstacle].PosInPlanner, angle) + Obstacles [thObstacle].PosInPlanner;
                        if (MyBounder [0].x > Me [b] [c].x)
                            MyBounder [0].x = Me [b] [c].x;
                        if (MyBounder [0].y > Me [b] [c].y)
                            MyBounder [0].y = Me [b] [c].y;
                        if (MyBounder [1].x < Me [b] [c].x)
                            MyBounder [1].x = Me [b] [c].x;
                        if (MyBounder [1].y < Me [b] [c].y)
                            MyBounder [1].y = Me [b] [c].y;
                    }
                }
                if (collision())
                    break;
            
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    toIPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
                    Futile.stage.RemoveChild(sOPolygons [thObstacle] [b]);
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Obstacles [thObstacle].Polygons [b].plannerIVertices [c] = RotateBy(Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - Obstacles [thObstacle].PosInPlanner, angle) + Obstacles [thObstacle].PosInPlanner;
                        toIPrint [c].x = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sOPolygons [thObstacle] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sOPolygons [thObstacle] [b].color = color;
                    Futile.stage.AddChild(sOPolygons [thObstacle] [b]);
                }
                bef.x = mouse_p.x;
                bef.y = mouse_p.y;
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(1))
                    break;
            }
        }
        inMouse = false;
    }*/
    public void getBitmap(){
        Vector2 Cpt;
        for (int cpt=0; cpt<Robots[0].numControlpt; cpt++){
            /*initialize Bitmap*/
            for (int x=0; x<128; x++){
                for (int y=0; y<128; y++)
                    Bitmap [cpt,x, y] = 254;
            }
            /*draw obstacles on Bitmap*/
            int d, basis, x_index, y_index;//edges of the polygon
            float dx, dy;
            int x_max, x_min;//a polygon
            int[,] frame = new int[128, 2];//a polygon
            for (int a=0; a < numObstacle; a++){
                for (int b=0; b<Obstacles[a].numPolygon; b++){
                    x_min = 128;
                    x_max = -1;
                    for (int i=0; i<128; i++){
                        frame [i, 0] = 128;//store y_min
                        frame [i, 1] = -1;//store y_max
                    }
                    for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++){//a polygon
                        basis = (c == 0) ? (Obstacles [a].Polygons [b].numVertice - 1) : (c - 1);
                        d = Mathf.CeilToInt(Mathf.Max(Mathf.Abs(Obstacles [a].Polygons [b].plannerIVertices [c].x - Obstacles [a].Polygons [b].plannerIVertices [basis].x), Mathf.Abs(Obstacles [a].Polygons [b].plannerIVertices [c].y - Obstacles [a].Polygons [b].plannerIVertices [basis].y)));
                        dx = ((Obstacles [a].Polygons [b].plannerIVertices [c].x - Obstacles [a].Polygons [b].plannerIVertices [basis].x) / d);
                        dy = ((Obstacles [a].Polygons [b].plannerIVertices [c].y - Obstacles [a].Polygons [b].plannerIVertices [basis].y) / d);
                        for (int i=0; i<=d; i++){//edges of the polygon

                            x_index = (int)(Obstacles [a].Polygons [b].plannerIVertices [basis].x + i * dx);
                            y_index = (int)(Obstacles [a].Polygons [b].plannerIVertices [basis].y + i * dy);
                            if (frame [x_index, 0] > y_index)
                                frame [x_index, 0] = y_index;
                            if (frame [x_index, 1] < y_index)
                                frame [x_index, 1] = y_index;
                            if (x_min > x_index)
                                x_min = x_index;
                            if (x_max < x_index)
                                x_max = x_index;
                        }
                    }
                    for (int x=x_min; x<=x_max; x++){
                        for (int y=frame[x,0]; y<=frame[x,1]; y++)
                            Bitmap [cpt,y, x] = 255;
                    }
                }
            }
            /*potential wave from goal*/
            int[,,] wave = new int[256, 128 * 4, 2];//Li,sucessors,(x,y)
            int nextNum, curNum;
            nextNum = 0;
            Cpt=RotateBy(Robots[0].Controlpts[cpt],Robots[0].goalSite.Angle)+Robots[0].goalSite.Vertice;
            Bitmap [cpt,Mathf.FloorToInt(Cpt.y), Mathf.FloorToInt(Cpt.x)] = 0;
            wave [0, nextNum, 0] = Mathf.FloorToInt(Cpt.x);
            wave [0, nextNum, 1] = Mathf.FloorToInt(Cpt.y);
            nextNum++;
            curNum = nextNum;
            for (int i=0; (curNum>0)&&(i<255); i++){
                nextNum = 0;
                for (int j=0; j<curNum; j++){
                    //up
                    y_index = wave [i, j, 1] + 1;
                    if (y_index <= 127){
                        x_index = wave [i, j, 0];
                        if (Bitmap [cpt,y_index, x_index] == 254){
                            Bitmap [cpt,y_index, x_index] = i + 1;
                            wave [i + 1, nextNum, 0] = x_index;
                            wave [i + 1, nextNum, 1] = y_index;
                            nextNum++;
                        }
                    }
                    //down
                    y_index = wave [i, j, 1] - 1;
                    if (y_index >= 0){
                        x_index = wave [i, j, 0];
                        if (Bitmap [cpt,y_index, x_index] == 254){
                            Bitmap [cpt,y_index, x_index] = i + 1;
                            wave [i + 1, nextNum, 0] = x_index;
                            wave [i + 1, nextNum, 1] = y_index;
                            nextNum++;
                        }
                    }
                    //right
                    x_index = wave [i, j, 0] + 1;
                    if (x_index <= 127){
                        y_index = wave [i, j, 1];
                        if (Bitmap [cpt,y_index, x_index] == 254){
                            Bitmap [cpt,y_index, x_index] = i + 1;
                            wave [i + 1, nextNum, 0] = x_index;
                            wave [i + 1, nextNum, 1] = y_index;
                            nextNum++;
                        }
                    }
                    //left
                    x_index = wave [i, j, 0] - 1;
                    if (x_index >= 0){
                        y_index = wave [i, j, 1];
                        if (Bitmap [cpt,y_index, x_index] == 254){
                            Bitmap [cpt,y_index, x_index] = i + 1;
                            wave [i + 1, nextNum, 0] = x_index;
                            wave [i + 1, nextNum, 1] = y_index;
                            nextNum++;
                        }
                    }
                }
                curNum = nextNum;
            }
        }
    }
/*    public void showBitmap(bool showCoordinate){
        try{
            Vector2 Cpt;
            for(int cpt=0;cpt<Robots[0].numControlpt;cpt++){
                string fName="/bitmap"+cpt.ToString()+".txt";
                string path = Application.dataPath + fName;
                FileStream aFile = new FileStream(path, FileMode.Create); //FileMode.OpenOrCreate       
                StreamWriter sw = new StreamWriter(aFile);           
                for (int y=127; y>=0; y--){
                    if(showCoordinate){
                        for (int x=0; x<128; x++){
                            if(Bitmap [cpt,y,x]!=255){
                                sw.Write("("+x.ToString("D3")+","+y.ToString("D3")+")"+":"+Bitmap [cpt,y,x].ToString("D3")+" ");
                            }else{
                                sw.Write("              ");                
                            }
                        }
                        sw.Write("\n");
                    }else{
                        for (int x=0; x<128; x++){
                            if(Bitmap [cpt,y,x]!=255){
                                sw.Write(Bitmap [cpt,y,x].ToString("D3")+" ");                      
                            }else{
                                sw.Write("    ");
                            }
                        }
                        sw.Write("\n");
                    }
                }
                Cpt=RotateBy(Robots[0].Controlpts[cpt],Robots[0].goalSite.Angle)+Robots[0].goalSite.Vertice;
                sw.Write("Control Point "+cpt+" : ( "+((int)Cpt.x)+", "+((int)Cpt.y)+" )");
                sw.Close();
            }
        }
        catch (IOException ex){            
            Console.WriteLine(ex.Message);            
            Console.ReadLine();            
            return ;            
        }
    }*/
    public int getU(Vector2 v,float a){
        int u = 0;
        Vector2 temp ;
        for (int i =0; i<Robots[0].numControlpt; i++){
            temp=RotateBy(Robots[0].Controlpts[i],a)+v;
            u += Bitmap [i,Mathf.FloorToInt(temp.y),Mathf.FloorToInt(temp.x)];

        }
        return u;
    }
    public float equalsGoal(Vector2 v,float a){
        if (Mathf.Abs(v.x-Robots[0].goalSite.Vertice.x)< 1f && Mathf.Abs(v.y-Robots[0].goalSite.Vertice.y)< 1f)
            return Robots[0].goalSite.Angle-a;
        return 1000f;
    }
/*    public void clearPATH(){       
        for (int a=0; a<sPATH.Length; a++){
            for (int b=0; b<sPATH[a].Length; b++){
                for (int c=0; c<sPATH [a] [b].Length; c++){
                    Futile.stage.RemoveChild(sPATH [a] [b] [c]);
                }
            }
        }
        ifShowPath=false;
    }*/
    public bool BFS(){
		fromBFS = true;
        Vector2 childVertice = new Vector2();
        float childAngle = 0f;
		Me = new Vector2[rNumPolygon][];
		Parent = new Vector2[rNumPolygon][];
		for (int i=0; i<rNumPolygon; i++){
            Me [i] = new Vector2[Robots [0].Polygons [i].numVertice];
            Parent [i] = new Vector2[Robots [0].Polygons [i].numVertice];
        }
        MyBounder = new Vector2[2];
        ParentsBounder = new Vector2[2];
        robotBuildOthers();
        /*test init_robot collision*/
        robotBuildMe(Robots [0].initSite);
        if(collision())
           return false;
        /*test init_robot collision*/
        robotBuildMe(Robots [0].goalSite);
        if(collision())
            return false;

        LinkedList<Tnode> pathT = new LinkedList<Tnode>();
        LinkedList<Tnode>[] Open = new LinkedList<Tnode>[256 * Robots [0].numControlpt + 1];
        for (int i=0; i<Open.Length; i++)
            Open [i] = new LinkedList<Tnode>();
        bool[,,] Visited = new bool[128, 128, 360];
        for (int a=0; a<128; a++){
            for (int b=0; b<128; b++){
                for (int c=0; c<360; c++)
                    Visited [a, b, c] = false;
            }
        }
        int tempU, i_child, i_open;
        float testAngle;
        Tnode child ;
        Tnode cur = new Tnode(0, -1, Robots [0].initSite.Vertice, Robots [0].initSite.Angle);
		int i_minOpen = getU(cur.pos,cur.ang);
        Open [i_minOpen].AddFirst(cur);
        pathT.AddFirst(cur);
        Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(cur.ang)] = true;
        bool i_valid=false;
        bool Success = false;
        if ((testAngle = equalsGoal(cur.pos,cur.ang)) < 360){
            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                Success = true;
                cur.ang = Robots [0].goalSite.Angle;
                cur.pos = Robots [0].goalSite.Vertice;
            }
        }
        while (i_minOpen>=0 && !Success){//i_minOpen<0 : Open is empty
            /* get parent ( cur from Open[] ) */
            cur=Open [i_minOpen].First();
            Open [i_minOpen].RemoveFirst();
            if (Open [i_minOpen].Count() == 0){
                for (i_open=i_minOpen+1; i_open<Open.Length && Open [i_open].Count()==0; i_open++);
                i_minOpen =(i_open == Open.Length)?-1:i_open;
            }
            robotBuildParent(cur.pos,cur.ang);
            /* generate 10 childs ( childO to Open[], childT to pathT ) */
            for (i_child=1; i_child<=6; i_child++,i_valid=false){
                switch (i_child){
                    case 1:/* x+1 */
                        if (Mathf.FloorToInt(cur.pos.x) < 127){
                            childVertice.x = cur.pos.x + 1;
                            if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(cur.ang)]){
                                childVertice.y = cur.pos.y;
                                childAngle = cur.ang;
                                robotBuildMe(1,0);
                                i_valid = true;
                            }
                        }
                        break;
                    case 2:/* x-1 */
                        if (Mathf.FloorToInt(cur.pos.x) > 0){
                            childVertice.x = cur.pos.x - 1;
                            if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(cur.ang)]){
                                childVertice.y = cur.pos.y;
                                childAngle = cur.ang;
                                robotBuildMe(-1, 0);
                                i_valid = true;
                            }
                        }
                        break;
                    case 3:/* y+1 */
                        if (Mathf.FloorToInt(cur.pos.y) < 127){
                            childVertice.y = cur.pos.y + 1;
                            if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.ang)]){
                                childVertice.x = cur.pos.x;
                                childAngle = cur.ang;
                                robotBuildMe(0, 1);
                                i_valid = true;
                            }
                        }
                        break;
                    case 4:/* y-1 */
                        if (Mathf.FloorToInt(cur.pos.y) > 0){
                            childVertice.y = cur.pos.y - 1;
                            if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.ang)]){
                                childVertice.x = cur.pos.x;
                                childAngle = cur.ang;
                                robotBuildMe(0, -1);
                                i_valid = true;
                            }
                        }
                        break;
                    case 5:/* Angle+4 */                        
                        childAngle = Normalize(cur.ang + 4);
                        if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(childAngle)]){
                            childVertice = cur.pos;
                            robotBuildMe(childVertice, childAngle);
                            i_valid = true;
                        }
                        break;
                    case 6:/* Angle-4 */
                        childAngle = Normalize(cur.ang - 4);
                        if (!Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(childAngle)]){
                            childVertice = cur.pos;
                            robotBuildMe(childVertice, childAngle);
                            i_valid = true;
                        }
                        break;
                }
                if (i_valid){   
                    Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(childAngle)] = true;
                    if (!collision()){
                        child = new Tnode(pathT.Count, cur.i_pathT, childVertice, childAngle);
                        tempU = getU(child.pos,child.ang);
                        Open [tempU].AddFirst(child);
                        pathT.AddLast(child);
                        if (tempU < i_minOpen || i_minOpen == -1)
                            i_minOpen = tempU;
                        if ((testAngle = equalsGoal(child.pos,child.ang)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.ang = Robots [0].goalSite.Angle;
                                child.pos = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }
                    }
                }
            }
        }
		fromBFS = false;
        if (Success){
//            ifShowPath = true;
            pathT_arr = new Tnode[pathT.Count];
            pathT.CopyTo(pathT_arr, 0);
            int index = pathT_arr.Length - 1;
            int numNode = 0;
            while (true){
                numNode++;
                index = pathT_arr [index].iParent_pathT;
                if (index == -1)
                    break;
            }
//            ifAniPath= true;
            robotPath=new int[numNode];
            robotPath[0]=0;
            int i_path=numNode-1;
//            int num = Mathf.FloorToInt(((float)numNode) / 20f);//for 5
//            int ii;//for 5
//            sPATH = new WTPolygonSprite[num][][];//for 5
//            sPATH = new WTPolygonSprite[numNode][][];//NOT FOR 5
            index = pathT_arr.Length - 1;
//            numNode = 0;
            Tnode p;
//            Vector2 from, to;
//            Vector2[] screen=new Vector2[4];
//            float vh;
            while (true){
//                for (ii=0; ii<19&&index!=0; ii++){//for 5
//                    robotPath[i_path--]=index;
//                    index = pathT_arr [index].iParent_pathT;
//                }
//                if (index == 0&&ii<19)//for 5
//                    break;
                robotPath[i_path--]=index;//NOT FOR 5
                p = pathT_arr [index];
                index = p.iParent_pathT;
 /*               sPATH [numNode] = new WTPolygonSprite[Robots [0].numPolygon][];
                for (int a=0; a<Robots[0].numPolygon; a++){
                    sPATH [numNode] [a] = new WTPolygonSprite[Robots [0].Polygons [a].numVertice];
                    for (int b=0; b<Robots[0].Polygons[a].numVertice; b++){
                        if (b == 0){
                            from = RotateBy(Robots [0].Polygons [a].Vertices [Robots [0].Polygons [a].numVertice - 1], p.ang) + p.pos;
                        } else{
                            from = RotateBy(Robots [0].Polygons [a].Vertices [b - 1], p.ang) + p.pos;
                        }
                        to = RotateBy(Robots [0].Polygons [a].Vertices [b], p.ang) + p.pos;
                        vh = Vector2.Angle(from - to,Vector2.right);
                        if (vh > 30 && vh < 150){
                            screen = new Vector2[]{
                                new Vector2((from.x - 0.2f) * x_PlannerToCanvas, from.y * y_PlannerToCanvas),
                                new Vector2((from.x + 0.2f) * x_PlannerToCanvas, from.y * y_PlannerToCanvas),
                                new Vector2((to.x + 0.2f) * x_PlannerToCanvas, to.y * y_PlannerToCanvas),
                                new Vector2((to.x - 0.2f) * x_PlannerToCanvas, to.y * y_PlannerToCanvas),
                            };
                        } else{
                            screen = new Vector2[]{
                                new Vector2(from.x * x_PlannerToCanvas, (from.y - 0.2f) * y_PlannerToCanvas),
                                new Vector2(from.x * x_PlannerToCanvas, (from.y + 0.2f) * y_PlannerToCanvas),
                                new Vector2(to.x * x_PlannerToCanvas, (to.y + 0.2f) * y_PlannerToCanvas),
                                new Vector2(to.x * x_PlannerToCanvas, (to.y - 0.2f) * y_PlannerToCanvas),
                            };
                        }
                        sPATH [numNode] [a] [b] = new WTPolygonSprite(new WTPolygonData(screen));
                        sPATH [numNode] [a] [b].color = Color.black;
                        Futile.stage.AddChild(sPATH [numNode] [a] [b]);
                    }
                }*/
                if (index == -1)
                    break;
//                numNode++;
            }
//            print("the number of nodes in pathT : "+pathT.Count());
            return true;
        }
        return false;
    }
    IEnumerator animation(float timeinseconds){
		Color temp;
		bool[] ifThreeTimes=new bool[]{false,false,false};
		while(threeTimes<3){
			ifAniPath = true;
			//Me,MyBounder,Others
			Me = new Vector2[rNumPolygon][];
			for (int j=0; j<rNumPolygon; j++)
				Me [j] = new Vector2[Robots [0].Polygons [j].numVertice];
			MyBounder = new Vector2[2];				  
//			robotBuildOthers ();
			int i;
			if(testBFS){
				for (i=1; i<robotPath.Length; i++) {
					delayByO=false;
					//Me,MyBounder
					robotBuildMe (new Configuration (new Vector2 (pathT_arr [robotPath [i]].pos.x, pathT_arr [robotPath [i]].pos.y), pathT_arr [robotPath [i]].ang));
					robotBuildOthers();
					if (collision ()) {//RETURN false, then BFS, then animation repead
						if( inMouse && i_others==thObstacle ){
							while(lockMoveO);
							StopCoroutine("moveObstacle");
							inMouse = false;
						}
//						if(!delayByO){
							ifAniCo=true;
							break;
//						}
					}
					Robots [0].initSite.Vertice = pathT_arr [robotPath [i]].pos;
					Robots [0].initSite.Angle = pathT_arr [robotPath [i]].ang;
					for (int b=0; b<rNumPolygon; b++) { 
						temp=sIRPolygons [0] [b].color;
						Futile.stage.RemoveChild (sIRPolygons [0] [b]);
						toIPrint = new Vector2[Robots [0].Polygons [b].numVertice];
						for (int c=0; c<Robots[0].Polygons[b].numVertice; c++) {
							Robots [0].Polygons [b].plannerIVertices [c] = RotateBy (Robots [0].Polygons [b].Vertices [c], pathT_arr [robotPath [i]].ang) + pathT_arr [robotPath [i]].pos;
							toIPrint [c].x = Robots [0].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
							toIPrint [c].y = Robots [0].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
						}
						sIRPolygons [0] [b] = new WTPolygonSprite (new WTPolygonData (toIPrint));
						sIRPolygons [0] [b].color = temp;
						Futile.stage.AddChild (sIRPolygons [0] [b]);
					}
					yield return new WaitForSeconds (delayByO?timeinseconds*timerPenalty*2:timeinseconds*timerPenalty);
				}
			}
			if(ifAniCo||(!testBFS)){
				if(ifAniCo){
					getBitmap ();
					testBFS=BFS();
				}
				if(!testBFS){
					scoreUser++;
					if(threeTimes<2){
						threeTimes++;
						Robots[0].goalSite=goal[threeTimes];
						getBitmap ();
						while (!(testBFS=BFS())&&numAddO>0){
							if(inMouse){								
								while(lockMoveO);
								StopCoroutine("moveObstacle");
								inMouse = false;
							}
							cutObstacle();
							getBitmap ();
						}

						for (int b=0; b<Robots[0].numPolygon; b++) {
							sGoal[threeTimes][b].color=sGoal[threeTimes-1][b].color;
							sGoal[threeTimes-1][b].color=Color.gray;
						}

						if(threeTimes==1){
							sGoal[threeTimes][rNumPolygon].color=Color.white;
							sGoal[threeTimes][rNumPolygon+1].color=Color.yellow;
						}
						else if(threeTimes==2){
							sGoal[threeTimes][Robots[0].numPolygon-1].color=Color.white;
						}

					}
					else{//part_3:NPC lost
						for (int b=0; b<Robots[0].numPolygon; b++)
							sGoal[threeTimes][b].color=Color.gray;
						break;
					}
				}
			}
			else{//success
				scoreNPC++;

				if(threeTimes<2){

					for (int b=0; b<Robots[0].numPolygon; b++) {
						sGoal[threeTimes+1][b].color=sGoal[threeTimes][b].color;
						sGoal[threeTimes][b].color=Color.gray;
					}

					if(threeTimes==0){//A
						ifThreeTimes[0]=true;
						sIRPolygons [0] [rNumPolygon].color = Color.green;
						sGoal[threeTimes+1][rNumPolygon].color=Color.white;
						rNumPolygon++;
						sGoal[threeTimes+1][rNumPolygon].color=Color.yellow;
					}
					else if(threeTimes==1 && !ifThreeTimes[0]){//B
						ifThreeTimes[1]=true;
						sGoal[threeTimes+1][rNumPolygon+1].color=Color.white;

//						sIRPolygons [0] [rNumPolygon]=sIRPolygons [0] [rNumPolygon+1];
//						Futile.stage.RemoveChild(sIRPolygons [0] [rNumPolygon+1]);
//						sIRPolygons [0] [rNumPolygon].color = Color.yellow;

						Robots[0].Polygons[rNumPolygon]=Robots[0].Polygons[rNumPolygon+1];
						for(int j=0;j<Robots[0].Polygons[rNumPolygon].numVertice;j++)
							Robots[0].Polygons[rNumPolygon].plannerIVertices[j]=plannerGoal[1][rNumPolygon+1][j];
						Robots[0].Polygons[rNumPolygon+1]=new Polygon();

						toGoal=new Vector2[Robots[0].Polygons[rNumPolygon].numVertice];
						for (int c=0; c<Robots[0].Polygons[rNumPolygon].numVertice; c++) {
							toGoal [c].x = plannerGoal[1][rNumPolygon+1][c].x*x_PlannerToCanvas;
							toGoal [c].y = plannerGoal[1][rNumPolygon+1][c].y*y_PlannerToCanvas;
						}
						Futile.stage.RemoveChild(sIRPolygons [0] [rNumPolygon]);
						Futile.stage.RemoveChild(sIRPolygons [0] [rNumPolygon+1]);
						sIRPolygons [0] [rNumPolygon] = new WTPolygonSprite (new WTPolygonData (toGoal));
						sIRPolygons [0] [rNumPolygon].color = Color.yellow;
						Futile.stage.AddChild(sIRPolygons [0] [rNumPolygon]);

						rNumPolygon++;
					}
					else if(threeTimes==1 && ifThreeTimes[0]){//A,B
						ifThreeTimes[1]=true;

						toGoal=new Vector2[Robots[0].Polygons[rNumPolygon].numVertice];
						for (int c=0; c<Robots[0].Polygons[rNumPolygon].numVertice; c++) {
							toGoal [c].x = plannerGoal[1][rNumPolygon][c].x*x_PlannerToCanvas;
							toGoal [c].y = plannerGoal[1][rNumPolygon][c].y*y_PlannerToCanvas;
						}
						Futile.stage.RemoveChild(sIRPolygons [0] [rNumPolygon]);
						sIRPolygons [0] [rNumPolygon] = new WTPolygonSprite (new WTPolygonData (toGoal));
						sIRPolygons [0] [rNumPolygon].color = Color.yellow;
						Futile.stage.AddChild(sIRPolygons [0] [rNumPolygon]);

						for(int j=0;j<plannerGoal[1][rNumPolygon].Length;j++)
							Robots[0].Polygons[rNumPolygon].plannerIVertices[j]=plannerGoal[1][rNumPolygon][j];

						sGoal[threeTimes+1][rNumPolygon].color=Color.white;
						rNumPolygon++;
					}

					Robots[0].goalSite=goal[threeTimes+1];
					getBitmap ();
					testBFS=BFS();

				}
				else{//part_3:NPC win
					for (int b=0; b<Robots[0].numPolygon; b++)
						sGoal[threeTimes][b].color=Color.gray;
					break;
				}

				threeTimes++;
			}
			ifAniCo=false;
		}

		/*obstacles*/
/*		for (int a=0; a<numObstacle-numAddO; a++){
			for (int b=0; b<Obstacles[a].numPolygon; b++){
				sOPolygons[a][b].color = Color.black;//black,white
			}
		}*/

		ifAniCo=false;
		ifAniPath = false;
		CancelInvoke("timer15s");
		yield return new WaitForSeconds(timeinseconds/100000);
	}     
}