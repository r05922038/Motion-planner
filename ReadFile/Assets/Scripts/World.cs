using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
public class World : MonoBehaviour {
    /* data_structure */
    public int numRobot,numObstacle;
    public Robot[] Robots;
    public Obstacle[] Obstacles;
    public Wall Walls;
    public bool inMouse;
    /* draw_polygon*/
    FutileParams fp;          
    WTPolygonSprite[][] sIRPolygons,sGRPolygons,sOPolygons;
    WTPolygonSprite sWall;
    public static float x_PlannerToCanvas,y_PlannerToCanvas,canvasToPlanner;
    /*bitmap*/
    public int[,,] Bitmap;
    /*collision*/
    public Vector2[][] Me,Parent;
    public Vector2[][][] Others;
    public Vector2[] MyBounder,ParentsBounder;
    public Vector2[][] OthersBounder;
    /*bfs*/
    bool ifShowPath,ifAniPath,lastBitmap;
    WTPolygonSprite[][][] sPATH;
    Tnode[] pathT_arr;
    int[] robotPath;
    WTPolygonSprite[] aniRobot;
    Tnode[] robotPathT;
    /*temp*/
    public Vector3 curMouse,mouse_p;
    public int thRobot,thObstacle,IorG,RorO;
    public Vector2[] toIPrint,toGPrint;
    /*GUI*/
    public Vector2 scrollPosition = Vector2.zero;
    public string robotFileName,obstacleFileName;
    // Use this for initialization
	void Start () {
        /*draw*/
        fp = new FutileParams(true, true, false, false);
        fp.AddResolutionLevel(480f, 1.0f, 1.0f, "-res1");
        fp.backgroundColor = Color.white;
        fp.origin = Vector2.zero;
        Futile.instance.Init(fp);
        x_PlannerToCanvas = (Futile.screen.halfWidth * 2) / Screen.width * 400 / 128;
        y_PlannerToCanvas = (Futile.screen.halfHeight * 2) / Screen.height * 400 / 128;
        canvasToPlanner = 128f / 400f;
        /*initialize*/
        robotFileName="/robot0.txt";
        obstacleFileName = "/obstacles0.txt";
        reset();
        print(Screen.height);
    }
    public void removeObject(){
        //sIRPolygons,sGRPolygons,sOPolygons
        for (int a=0; a<sIRPolygons.Length; a++)
        {
            for (int b=0; b<sIRPolygons[a].Length; b++){
                Futile.stage.RemoveChild(sIRPolygons [a] [b]);
                Futile.stage.RemoveChild(sGRPolygons [a] [b]);
            }
        }
        for (int a=0; a<sOPolygons.Length; a++)
        {
            for (int b=0; b<sOPolygons[a].Length; b++)
                Futile.stage.RemoveChild(sOPolygons [a] [b]);
        }
    }
    public void reset(){
        /*initialization*/
        ReadFile();
        MakeOneRobot(true);
        inMouse = false;
        if (Screen.width > Screen.height)
            Print();
        buildBitmap();
        ifShowPath = false;
        ifAniPath= false;
        lastBitmap = false;
    }   
    void OnGUI() {

        scrollPosition = GUI.BeginScrollView(new Rect(450, Screen.height-135, 100, 100), scrollPosition, new Rect(0, 0, 70, 220));        
        if (GUI.Button(new Rect(0, 0, 80, 20), "input-0"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot0.txt";        
            obstacleFileName = "/obstacles0.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 25, 80, 20), "input-1"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot1.txt";        
            obstacleFileName = "/obstacles1.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 50, 80, 20), "input-2"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot2.txt";        
            obstacleFileName = "/obstacles2.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 75, 80, 20), "input-3"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot3.txt";        
            obstacleFileName = "/obstacles3.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 100, 80, 20), "input-4"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot4.txt";        
            obstacleFileName = "/obstacles4.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 125, 80, 20), "input-5"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot5.txt";        
            obstacleFileName = "/obstacles5.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 150, 80, 20), "input-6"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot6.txt";        
            obstacleFileName = "/obstacles6.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 175, 80, 20), "input-7"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot7.txt";        
            obstacleFileName = "/obstacles7.txt";
            reset();
        }
        if (GUI.Button(new Rect(0, 200, 80, 20), "input-8"))
        {
            if(ifShowPath)
                clearPATH();
            removeObject();
            robotFileName = "/robot8.txt";        
            obstacleFileName = "/obstacles8.txt";
            reset();
        }               
        GUI.EndScrollView();

        if (GUI.Button(new Rect(450, Screen.height-400, 100, 30), "Show Path")){
            if (!inMouse&&test_Obstacle_outer()){
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
        if (GUI.Button(new Rect(450, Screen.height-350, 100, 30), "Clear Path")){
            if (!inMouse && ifShowPath)
                clearPATH();
        }
        if (GUI.Button(new Rect(450, Screen.height-300, 100, 30), "Smooth Path")){
            if (!inMouse && ifShowPath){
                clearPATH();
                if(ifAniPath)
                    smoothing();
            }
        }
        if (GUI.Button(new Rect(450, Screen.height-250, 100, 30), "Animation")){
            if(!inMouse && ifAniPath)
                StartCoroutine (animation (1f / 30f));
        }
        if (GUI.Button(new Rect(450, Screen.height-200, 100, 45), "Show"+'\n'+"Potential Field")){
            getBitmap();
            lastBitmap=true;
            showBitmap(false);
        }      

    }
    bool test_Obstacle_outer(){
        /*test obstacle collision_outer*/
        for (int a=0; a<numObstacle; a++)
        {
            for (int b=0; b<Obstacles[a].numPolygon; b++)
            {
                for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++)
                {
                    if (Obstacles [a].Polygons [b].plannerIVertices [c].x < 0 || Obstacles [a].Polygons [b].plannerIVertices [c].x > 128)
                        return false;
                    if (Obstacles [a].Polygons [b].plannerIVertices [c].y < 0 || Obstacles [a].Polygons [b].plannerIVertices [c].y > 128)
                        return false;
                }
            }
        }
        return true;
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
        Obstacles = new Obstacle[numObstacle];
        for(int i=0;i<numObstacle;i++)
            Obstacles[i]=new Obstacle();
        for (int a=0; a<numObstacle; a++){
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
    public void MakeOneRobot(bool oneRobot){
        if(oneRobot) numRobot = 1;
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
        for (int a=0; a<numRobot; a++){
            sIRPolygons [a] = new WTPolygonSprite[Robots [a].numPolygon];
            sGRPolygons [a] = new WTPolygonSprite[Robots [a].numPolygon];
        }
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
            for (int b=0; b<Robots[a].numPolygon; b++){
                toIPrint = new Vector2[Robots [a].Polygons [b].numVertice];
                toGPrint= new Vector2[Robots [a].Polygons [b].numVertice];
                for (int c=0; c<Robots[a].Polygons[b].numVertice; c++){
                    toIPrint [c].Set(Robots [a].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas,Robots [a].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas);
                    toGPrint [c].Set(Robots [a].Polygons [b].plannerGVertices [c].x * x_PlannerToCanvas,Robots [a].Polygons [b].plannerGVertices [c].y * y_PlannerToCanvas);
                }
                sIRPolygons [a][b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                sIRPolygons [a][b].color = Color.cyan;
                Futile.stage.AddChild(sIRPolygons [a][b]);
                sGRPolygons [a][b] = new WTPolygonSprite(new WTPolygonData(toGPrint));
                sGRPolygons [a][b].color = Color.blue;
                Futile.stage.AddChild(sGRPolygons [a][b]);
            }
        }
        /*obstacles*/
        for (int a=0; a<numObstacle; a++){
            for (int b=0; b<Obstacles[a].numPolygon; b++){
                toIPrint = new Vector2[Obstacles [a].Polygons [b].numVertice];
                for (int c=0; c<Obstacles[a].Polygons[b].numVertice; c++)
                    toIPrint [c].Set(Obstacles [a].Polygons [b].plannerIVertices [c].x *x_PlannerToCanvas,Obstacles [a].Polygons [b].plannerIVertices [c].y*y_PlannerToCanvas);
                sOPolygons[a][b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                sOPolygons[a][b].color = Color.gray;
                Futile.stage.AddChild(sOPolygons[a][b]);
            }
        }
        /*walls*/
        toIPrint = new Vector2[4];
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
            new Vector2(128, 128 ),
            new Vector2(0 , 128 ),
        };
        for (int i =0; i<4; i++)
            toIPrint [i].Set(Walls.top [i].x * x_PlannerToCanvas, Walls.top [i].y * y_PlannerToCanvas);
        sWall = new WTPolygonSprite(new WTPolygonData(toIPrint));
        sWall.color = Color.black;
        Futile.stage.AddChild(sWall);
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
        if (!inMouse&&Input.GetMouseButtonDown (0)) {
            curMouse = Input.mousePosition;
            RorO=inRegion ();
            if (RorO==1) { 
                ifAniPath= false;
                lastBitmap= false;
                StartCoroutine (moveRobot (1f / 10000000000000f));//by IorG
            }
            else if(RorO==2){
                ifAniPath= false;
                lastBitmap= false;
                StartCoroutine (moveObstacle (1f / 10000000000000f));
            }
        }
        else if(!inMouse&&Input.GetMouseButtonDown (1)) {
            curMouse = Input.mousePosition;
            RorO=inRegion ();
            if (RorO==1){
                ifAniPath= false;
                lastBitmap= false;
                StartCoroutine (rotateRobot (1f /10000000000000f));
            }
            else if(RorO==2){
                ifAniPath= false;
                lastBitmap= false;
                StartCoroutine (rotateObstacle (1f / 10000000000000f));
            }
        }
    }
    public int inRegion(){
        Vector2 basis=new Vector2();
        float angle;
        curMouse.x = curMouse.x*canvasToPlanner;
        curMouse.y =curMouse.y*canvasToPlanner;
        /*search robots*/
        for(int a=0;a<numRobot;a++){
            for(int b=0;b<Robots[a].numPolygon;b++){
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
                basis.Set(Robots[a].Polygons[b].plannerGVertices[Robots[a].Polygons[b].numVertice-1].x - curMouse.x, Robots[a].Polygons[b].plannerGVertices[Robots[a].Polygons[b].numVertice-1].y - curMouse.y);
                angle = 0;
                for(int c=0;c<Robots[a].Polygons[b].numVertice;c++){
                    angle += Vector2.Angle (new Vector2(Robots[a].Polygons[b].plannerGVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerGVertices[c].y-curMouse.y), basis);
                    basis.Set (Robots[a].Polygons[b].plannerGVertices[c].x-curMouse.x,Robots[a].Polygons[b].plannerGVertices[c].y-curMouse.y);
                }
                if (Mathf.Abs(angle - 360)<1f) {
                    IorG=2;
                    thRobot=a;
                    return 1;
                }
            }
        }
        /*search obstacles*/
        for (int a=0; a<numObstacle; a++){
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
        return 0;//do nothing
    }
    public void robotBuildOthers(){
        OthersBounder = new Vector2[numObstacle][];
        Others = new Vector2[numObstacle][][];
        int i_obstacle,i_polygon,i_vertice;
        for (i_obstacle=0; i_obstacle<numObstacle; i_obstacle++){
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
    public bool collision (){
        //wall
        if(MyBounder [0].x<0 || MyBounder [0].y<0)
            return true;
        if(MyBounder [1].x>=128 || MyBounder [1].y>=128)
            return true;
        //others
        int i_others,i_polygon,i_vertice,i1, i2;
  
        Vector2 v1, v3, v122, v344;
        bool detection;
        for(i_others=0, detection=false ; i_others < OthersBounder.Length; i_others++, detection=false){
            //Me,Others
            if (MyBounder [0].x > OthersBounder [i_others] [0].x && MyBounder [0].x < OthersBounder [i_others] [1].x){
                if (MyBounder [0].y > OthersBounder [i_others] [0].y && MyBounder [0].y < OthersBounder [i_others] [1].y){
                    detection = true;
                }
                else if(MyBounder [1].y > OthersBounder [i_others] [0].y && MyBounder [1].y < OthersBounder [i_others] [1].y){
                    detection = true;
                }            
            }
            if (!detection){
                if (MyBounder [1].x > OthersBounder [i_others] [0].x && MyBounder [1].x < OthersBounder [i_others] [1].x){
                    if (MyBounder [0].y > OthersBounder [i_others] [0].y && MyBounder [0].y < OthersBounder [i_others] [1].y){
                        detection = true;
                    }
                    else if (MyBounder [1].y > OthersBounder [i_others] [0].y && MyBounder [1].y < OthersBounder [i_others] [1].y){
                        detection = true;
                    }
                }
            }
            //Others<Me
            if (!detection){
                if(OthersBounder [i_others] [0].x > MyBounder [0].x && OthersBounder [i_others] [0].x < MyBounder [1].x){
                    if(OthersBounder [i_others] [0].y > MyBounder [0].y && OthersBounder [i_others] [0].y < MyBounder [1].y){
                        detection = true;
                    }
                    else if(OthersBounder [i_others] [1].y > MyBounder [0].y && OthersBounder [i_others] [1].y < MyBounder [1].y){
                        detection = true;
                    }
                }
            }
            if (!detection){
                if(OthersBounder [i_others] [1].x > MyBounder [0].x && OthersBounder [i_others] [1].x < MyBounder [1].x){
                    if(OthersBounder [i_others] [0].y > MyBounder [0].y && OthersBounder [i_others] [0].y < MyBounder [1].y){
                        detection = true;
                    }
                    else if(OthersBounder [i_others] [1].y > MyBounder [0].y && OthersBounder [i_others] [1].y < MyBounder [1].y){
                        detection = true;
                    }
                }
            }

            if (detection){
                for (i1=0; i1<Me.Length; i1++){
                    for (i2=0; i2<Me[i1].Length; i2++){
                        v1 = ((i2 == 0) ? Me [i1] [Me [i1].Length - 1] : Me [i1] [i2 - 1]);
                        for (i_polygon=0; i_polygon<Others[i_others].Length; i_polygon++){
                            for (i_vertice=0; i_vertice<Others[i_others][i_polygon].Length; i_vertice++){
                                v3 = ((i_vertice == 0) ? Others[i_others][i_polygon][Others[i_others][i_polygon].Length - 1] : Others[i_others][i_polygon][i_vertice - 1]);
                                v122 = RotateBy(Me [i1] [i2] - v1, 90.0f);
                                v344 = RotateBy(Others[i_others][i_polygon][i_vertice] - v3, 90.0f);
                                if ((Vector2.Dot(v122, v3 - v1) * Vector2.Dot(v122, Others[i_others][i_polygon][i_vertice] - v1) < 0 && Vector2.Dot(v344, v1 - v3) * Vector2.Dot(v344, Me [i1] [i2] - v3) < 0))
                                    return true;
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
		Vector2 origCenter = new Vector2 ();
        //Me,MyBounder,Others
        Me=new Vector2[Robots[0].numPolygon][];
        for(int i=0;i<Robots[0].numPolygon;i++)
            Me[i]=new Vector2[Robots[thRobot].Polygons[i].numVertice];
        MyBounder = new Vector2[2];
        robotBuildOthers();
        yield return new WaitForSeconds(timeinseconds);
        if (IorG == 1){
            Vector2 def_c = new Vector2 (curMouse.x-Robots[thRobot].initSite.Vertice.x,curMouse.y-Robots[thRobot].initSite.Vertice.y);
            mouse_p = Input.mousePosition; 
            mouse_p.x = mouse_p.x*canvasToPlanner;
            mouse_p.y= mouse_p.y*canvasToPlanner;
            origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
            //Me,MyBounder
            robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
            if(collision()){
                while (true){
                    mouse_p = Input.mousePosition; 
                    mouse_p.x = mouse_p.x*canvasToPlanner;
                    mouse_p.y= mouse_p.y*canvasToPlanner;
                    origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
                    //Me,MyBounder
                    robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
                    if(!collision())
                        break;
                    Robots[thRobot].initSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
                    for(int b=0;b<Robots[thRobot].numPolygon;b++){
                        Futile.stage.RemoveChild(sIRPolygons[thRobot][b]);
                        toIPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                        for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                            Robots[thRobot].Polygons[b].plannerIVertices[c]=Robots[thRobot].Polygons[b].plannerIVertices[c]-origCenter+Robots[thRobot].initSite.Vertice;
                            toIPrint [c].x=Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                            toIPrint [c].y=Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                        }
                        sIRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toIPrint));
                        sIRPolygons[thRobot][b].color= Color.cyan;
                        Futile.stage.AddChild(sIRPolygons[thRobot][b]);
                    } 
                    yield return new WaitForSeconds(timeinseconds);
                }
            }
            while (true){
                mouse_p = Input.mousePosition; 
                mouse_p.x = mouse_p.x*canvasToPlanner;
                mouse_p.y= mouse_p.y*canvasToPlanner;
                origCenter.Set(Robots[thRobot].initSite.Vertice.x,Robots[thRobot].initSite.Vertice.y);
                //Me,MyBounder
                robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].initSite.Angle));
                if(collision())
                    break;
                Robots[thRobot].initSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
                for(int b=0;b<Robots[thRobot].numPolygon;b++){
                    Futile.stage.RemoveChild(sIRPolygons[thRobot][b]);
                    toIPrint=new Vector2[Robots[thRobot].Polygons[b].numVertice];
                    for(int c=0;c<Robots[thRobot].Polygons[b].numVertice;c++){
                        Robots[thRobot].Polygons[b].plannerIVertices[c]=Robots[thRobot].Polygons[b].plannerIVertices[c]-origCenter+Robots[thRobot].initSite.Vertice;
                        toIPrint [c].x=Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y=Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sIRPolygons[thRobot][b]=new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sIRPolygons[thRobot][b].color= Color.cyan;
                    Futile.stage.AddChild(sIRPolygons[thRobot][b]);
                } 
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(0))
                    break;
            }
        } 
        else{     
            Vector2 def_c = new Vector2 (curMouse.x-Robots[thRobot].goalSite.Vertice.x,curMouse.y-Robots[thRobot].goalSite.Vertice.y);
            mouse_p = Input.mousePosition; 
            mouse_p.x = mouse_p.x*canvasToPlanner;
            mouse_p.y= mouse_p.y*canvasToPlanner;
            origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
            robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
            if(collision()){
                while (true){
                    mouse_p = Input.mousePosition; 
                    mouse_p.x = mouse_p.x*canvasToPlanner;
                    mouse_p.y= mouse_p.y*canvasToPlanner;
                    origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
                    robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
                    if(!collision())
                        break;
                    Robots[thRobot].goalSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
                    for(int b=0;b<Robots[thRobot].numPolygon;b++){
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
                mouse_p.x = mouse_p.x*canvasToPlanner;
                mouse_p.y= mouse_p.y*canvasToPlanner;
                origCenter.Set(Robots[thRobot].goalSite.Vertice.x,Robots[thRobot].goalSite.Vertice.y);
                robotBuildMe(new Configuration(new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y),Robots[0].goalSite.Angle));
                if(collision())
                    break;
                Robots[thRobot].goalSite.Vertice.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
                for(int b=0;b<Robots[thRobot].numPolygon;b++){
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
                if (Input.GetMouseButtonDown(0))
                    break;
            }
        }
        inMouse = false;
    }
    IEnumerator moveObstacle(float timeinseconds){
        inMouse = true;
        Vector2 origCenter=new Vector2();
        Vector2 def_c = new Vector2 (curMouse.x-Obstacles[thObstacle].PosInPlanner.x,curMouse.y-Obstacles[thObstacle].PosInPlanner.y);
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

        mouse_p = Input.mousePosition; 
        mouse_p.x = mouse_p.x*canvasToPlanner;
        mouse_p.y= mouse_p.y*canvasToPlanner;
        origCenter.Set(Obstacles[thObstacle].PosInPlanner.x,Obstacles[thObstacle].PosInPlanner.y);
        //obstacleBuildMe
        MyBounder[0].Set(129, 129);
        MyBounder[1].Set(-5, -5);
        for (int b=0; b<Obstacles[thObstacle].numPolygon; b++){
            for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++){
                Me[b][c]=Obstacles[thObstacle].Polygons[b].plannerIVertices[c]-origCenter+new Vector2(mouse_p.x-def_c.x, mouse_p.y-def_c.y);
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
                origCenter.Set(Obstacles [thObstacle].PosInPlanner.x, Obstacles [thObstacle].PosInPlanner.y);
                //obstacleBuildMe
                MyBounder [0].Set(129, 129);
                MyBounder [1].Set(-5, -5);
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Me [b] [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + new Vector2(mouse_p.x - def_c.x, mouse_p.y - def_c.y);
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
                Obstacles [thObstacle].PosInPlanner.Set(mouse_p.x - def_c.x, mouse_p.y - def_c.y); 
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    Futile.stage.RemoveChild(sOPolygons [thObstacle] [b]);
                    toIPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Obstacles [thObstacle].Polygons [b].plannerIVertices [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + Obstacles [thObstacle].PosInPlanner;
                        toIPrint [c].x = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sOPolygons [thObstacle] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sOPolygons [thObstacle] [b].color = Color.gray;
                    Futile.stage.AddChild(sOPolygons [thObstacle] [b]);
                }      
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(0)){
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
                origCenter.Set(Obstacles [thObstacle].PosInPlanner.x, Obstacles [thObstacle].PosInPlanner.y);
                //obstacleBuildMe
                MyBounder [0].Set(129, 129);
                MyBounder [1].Set(-5, -5);
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Me [b] [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + new Vector2(mouse_p.x - def_c.x, mouse_p.y - def_c.y);
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
                Obstacles [thObstacle].PosInPlanner.Set(mouse_p.x - def_c.x, mouse_p.y - def_c.y); 
                for (int b=0; b<Obstacles[thObstacle].numPolygon; b++)
                {
                    Futile.stage.RemoveChild(sOPolygons [thObstacle] [b]);
                    toIPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
                    for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++)
                    {
                        Obstacles [thObstacle].Polygons [b].plannerIVertices [c] = Obstacles [thObstacle].Polygons [b].plannerIVertices [c] - origCenter + Obstacles [thObstacle].PosInPlanner;
                        toIPrint [c].x = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sOPolygons [thObstacle] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sOPolygons [thObstacle] [b].color = Color.gray;
                    Futile.stage.AddChild(sOPolygons [thObstacle] [b]);
                }   
                yield return new WaitForSeconds(timeinseconds);
                if (Input.GetMouseButtonDown(0))
                    break;
            }
        }
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
        float angle,totalAngle;
        Vector2 to=new Vector2();
        Vector2 from=new Vector2();
        Vector3 bef =Input.mousePosition;
        bef.x = bef.x *canvasToPlanner;
        bef.y = bef.y *canvasToPlanner;
        totalAngle = 0;
        Me=new Vector2[Robots[thRobot].numPolygon][];
        for(int i=0;i<Robots[thRobot].numPolygon;i++)
            Me[i]=new Vector2[Robots[thRobot].Polygons[i].numVertice];
        MyBounder = new Vector2[2];
        robotBuildOthers();
        if (IorG == 1)
        {
            yield return new WaitForSeconds(timeinseconds);            
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * canvasToPlanner;
                mouse_p.y = mouse_p.y * canvasToPlanner;              
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
                for (int b=0; b<Robots[thRobot].numPolygon; b++)
                {
                    toIPrint = new Vector2[Robots [thRobot].Polygons [b].numVertice];
                    Futile.stage.RemoveChild(sIRPolygons [thRobot] [b]);
                    for (int c=0; c<Robots[thRobot].Polygons[b].numVertice; c++)
                    {
                        Robots [thRobot].Polygons [b].plannerIVertices [c] = RotateBy(Robots [thRobot].Polygons [b].plannerIVertices [c] - Robots [thRobot].initSite.Vertice, angle) + Robots [thRobot].initSite.Vertice;
                        toIPrint [c].x = Robots [thRobot].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                        toIPrint [c].y = Robots [thRobot].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                    }
                    sIRPolygons [thRobot] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                    sIRPolygons [thRobot] [b].color = Color.cyan;
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
        } else
        {
            yield return new WaitForSeconds(timeinseconds);            
            while (true)
            {
                mouse_p = Input.mousePosition;
                mouse_p.x = mouse_p.x * canvasToPlanner;
                mouse_p.y = mouse_p.y * canvasToPlanner;
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
                for (int b=0; b<Robots[thRobot].numPolygon; b++)
                {
                    toGPrint = new Vector2[Robots [thRobot].Polygons [b].numVertice];
                    Futile.stage.RemoveChild(sGRPolygons [thRobot] [b]);
                    for (int c=0; c<Robots[thRobot].Polygons[b].numVertice; c++)
                    {
                        Robots [thRobot].Polygons [b].plannerGVertices [c] = RotateBy(Robots [thRobot].Polygons [b].plannerGVertices [c] - Robots [thRobot].goalSite.Vertice, angle) + Robots [thRobot].goalSite.Vertice;
                        toGPrint [c].x = Robots [thRobot].Polygons [b].plannerGVertices [c].x * x_PlannerToCanvas;
                        toGPrint [c].y = Robots [thRobot].Polygons [b].plannerGVertices [c].y * y_PlannerToCanvas;
                    }
                    sGRPolygons [thRobot] [b] = new WTPolygonSprite(new WTPolygonData(toGPrint));
                    sGRPolygons [thRobot] [b].color = Color.blue;
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
        }
        inMouse = false;        
    }
    IEnumerator rotateObstacle(float timeinseconds){ 
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
                    sOPolygons [thObstacle] [b].color = Color.gray;
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
                    sOPolygons [thObstacle] [b].color = Color.gray;
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
    }
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
    public void showBitmap(bool showCoordinate){
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
    }
    public int getU(Vector2 v,float a){
        int u = 0;
        Vector2 temp ;
        for (int i =0; i<Robots[0].numControlpt; i++){
            temp=RotateBy(Robots[0].Controlpts[i],a)+v;
            u += Bitmap [i,Mathf.FloorToInt(temp.y),Mathf.FloorToInt(temp.x)];

        }
        return u;
    }

/*    public float equalsGoal(Vector2 v,float a){
        if (Mathf.Abs(v.x-Robots[0].goalSite.Vertice.x)< 1f && Mathf.Abs(v.y-Robots[0].goalSite.Vertice.y)< 1f)
            return Robots[0].goalSite.Angle-a;
        return 1000f;
    }*/
    public float equalsGoal(Vector2 v,float a){
        if (Mathf.Abs(v.x-Robots[0].goalSite.Vertice.x)< 1f && Mathf.Abs(v.y-Robots[0].goalSite.Vertice.y)< 1f)
            return a-Robots[0].goalSite.Angle;
        return 1000f;
    }
    public void clearPATH(){       
        for (int a=0; a<sPATH.Length; a++){
            for (int b=0; b<sPATH[a].Length; b++){
                for (int c=0; c<sPATH [a] [b].Length; c++){
                    Futile.stage.RemoveChild(sPATH [a] [b] [c]);
                }
            }
        }
        ifShowPath=false;
    }

    public bool BFS(){
        Vector2 childVertice = new Vector2();
        float childAngle = 0f;
        Me = new Vector2[Robots [0].numPolygon][];
        Parent = new Vector2[Robots [0].numPolygon][];
        for (int i=0; i<Robots[0].numPolygon; i++){
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
//        cur.element.Vertice.x = Mathf.Floor(cur.element.Vertice.x) + (Robots [0].goalSite.Vertice.x - Mathf.Floor(Robots [0].goalSite.Vertice.x));
//        cur.element.Vertice.y = Mathf.Floor(cur.element.Vertice.y) + (Robots [0].goalSite.Vertice.y - Mathf.Floor(Robots [0].goalSite.Vertice.y));
        int i_minOpen = getU(cur.pos,cur.ang);
        Open [i_minOpen].AddFirst(cur);
        pathT.AddFirst(cur);
        Visited [Mathf.FloorToInt(cur.pos.x), Mathf.FloorToInt(cur.pos.y), Mathf.FloorToInt(cur.ang)] = true;
        bool i_valid=false;
        bool Success = false;

        float goalTurnLeft,goalTurnRight;
        for (goalTurnLeft=0; goalTurnLeft<=356; goalTurnLeft+=4)
        {
            robotBuildMe(Robots [0].goalSite.Vertice,Robots [0].goalSite.Angle+goalTurnLeft);
            if(collision())
                break;
        }
        if (goalTurnLeft > 356)//no collision
        {
            goalTurnRight =goalTurnLeft= 360f;
        } else
        {
            goalTurnLeft -= 4;//maybe 0
            for (goalTurnRight=0; goalTurnRight>=-356; goalTurnRight-=4)
            {
                robotBuildMe(Robots [0].goalSite.Vertice, Robots [0].goalSite.Angle + goalTurnRight);
                if (collision())
                    break;                
            }
            goalTurnRight += 4;//maybe 0
        }
/*        if ((testAngle = equalsGoal(cur.pos,cur.ang)) < 360){
            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                Success = true;
                cur.ang = Robots [0].goalSite.Angle;
                cur.pos = Robots [0].goalSite.Vertice;
            }
        }*/
        while (i_minOpen>=0 && !Success){//i_minOpen<0 : Open is empty
            /* get parent ( cur from Open[] ) */
            cur=Open [i_minOpen].First();
            Open [i_minOpen].RemoveFirst();
            if (Open [i_minOpen].Count() == 0){
                for (i_open=i_minOpen+1; i_open<Open.Length && Open [i_open].Count()==0; i_open++);
                i_minOpen =(i_open == Open.Length)?-1:i_open;
            }
            robotBuildParent(cur.pos,cur.ang);
            /* generate 6 childs ( childO to Open[], childT to pathT ) */
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
/*                        if ((testAngle = equalsGoal(child.pos,child.ang)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.ang = Robots [0].goalSite.Angle;
                                child.pos = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }*/
                        testAngle = equalsGoal(child.pos,child.ang);
                        if(testAngle<360){//same x,y
                            print(testAngle);
                            print("goalTurnLeft :"+goalTurnLeft);
                            print("goalTurnRight :"+goalTurnRight);
                            if(Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
    //                            child.ang = Robots [0].goalSite.Angle;
    //                            child.pos = Robots [0].goalSite.Vertice;
                                cur=child;
                                child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Robots [0].goalSite.Angle);
                                pathT.AddLast(child);    
                                break;
                            }
                            else if(goalTurnLeft==360f){//ok
                                Success = true;
                                if(testAngle>=180){//180~360,turn left,+4
                                    testAngle-=360;
                                }
                                else if(testAngle<=-180){//-180~-360,turn right,-4
                                    testAngle+=360;
                                }
                                if(testAngle>4){//turn right,-4
                                    while(testAngle>4){
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang - 4));
                                        pathT.AddLast(child);
                                        testAngle-=4;
                                    }
                                }
                                else{//turn left,+4
                                    while(testAngle<(-4)){
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang + 4));
                                        pathT.AddLast(child);
                                        testAngle+=4;
                                    }
                                }
                                cur=child;
                                child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Normalize(cur.ang - testAngle));
                                pathT.AddLast(child);
                                break;
                            }
                            else{
                                if(testAngle>4){//0~360
                                    if(testAngle<=goalTurnLeft){//turn right,-4
                                        Success = true;
                                        while(testAngle>4){
                                            cur=child;
                                            child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang - 4));
                                            pathT.AddLast(child);
                                            testAngle-=4;
                                        }
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Normalize(cur.ang - testAngle));
                                        pathT.AddLast(child);                                        
                                        break;
                                    }
                                    else if((testAngle-360)>=goalTurnRight){//turn left,+4
                                        Success = true;
                                        testAngle-=360;
                                        while(testAngle<(-4)){
                                            cur=child;
                                            child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang + 4));
                                            pathT.AddLast(child);
                                            testAngle+=4;
                                        }
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Normalize(cur.ang - testAngle));
                                        pathT.AddLast(child);
                                        break;
                                    }
                                }
                                else{//-360~0
                                    if(testAngle>=goalTurnRight){//turn left,+4
                                        Success = true;
                                        while(testAngle<(-4)){
                                            cur=child;
                                            child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang + 4));
                                            pathT.AddLast(child);
                                            testAngle+=4;
                                        }
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Normalize(cur.ang - testAngle));
                                        pathT.AddLast(child);                                        
                                        break;
                                    }
                                    else if((testAngle+360)<=goalTurnLeft){//turn right,-4
                                        Success = true;
                                        testAngle+=360;
                                        while(testAngle>4){
                                            cur=child;
                                            child = new Tnode(pathT.Count, cur.i_pathT, cur.pos, Normalize(cur.ang - 4));
                                            pathT.AddLast(child);
                                            testAngle-=4;
                                        }
                                        cur=child;
                                        child = new Tnode(pathT.Count, cur.i_pathT, Robots [0].goalSite.Vertice, Normalize(cur.ang - testAngle));
                                        pathT.AddLast(child);  
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }   
        if (Success){
            ifShowPath = true;
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
            ifAniPath= true;
            robotPath=new int[numNode];
            robotPath[0]=0;
            int i_path=numNode-1;
            int num = Mathf.FloorToInt(((float)numNode) / 5f);//for 5
            int ii;//for 5
            sPATH = new WTPolygonSprite[num][][];//for 5
//            sPATH = new WTPolygonSprite[numNode][][];//NOT FOR 5
            index = pathT_arr.Length - 1;
            numNode = 0;
            Tnode p;
            Vector2 from, to;
            Vector2[] screen=new Vector2[4];
            float vh;
            while (true){
                for (ii=0; ii<4&&index!=0; ii++){//for 5
                    robotPath[i_path--]=index;
                    index = pathT_arr [index].iParent_pathT;
                }
                if (index == 0&&ii<4)//for 5
                    break;
 //               robotPath[i_path--]=index;//NOT FOR 5
                p = pathT_arr [index];
                index = p.iParent_pathT;
                sPATH [numNode] = new WTPolygonSprite[Robots [0].numPolygon][];
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
                }
                if (index == -1)
                    break;
                numNode++;
            }
            print("the number of nodes in pathT : "+pathT.Count());
            return true;
        }
        return false;
    }
/*    public void BFS1(){
        Vector2 childVertice = new Vector2();
        float childAngle = 0f;
        Me = new Vector2[Robots [0].numPolygon][];
        Parent = new Vector2[Robots [0].numPolygon][];
        for (int i=0; i<Robots[0].numPolygon; i++){
            Me [i] = new Vector2[Robots [0].Polygons [i].numVertice];
            Parent [i] = new Vector2[Robots [0].Polygons [i].numVertice];
        }
        MyBounder = new Vector2[2];
        ParentsBounder = new Vector2[2];
        robotBuildOthers();
        LinkedList<Tnode> pathT = new LinkedList<Tnode>();
        LinkedList<Tnode>[] Open = new LinkedList<Tnode>[254 * Robots [0].numControlpt + 1];
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
        float testAngle, tempAngle;
        Tnode child ;
        Tnode cur = new Tnode(0, -1, Robots [0].initSite.Vertice, Robots [0].initSite.Angle);
        int i_minOpen = getU(cur.element);
        Open [i_minOpen].AddFirst(cur);
        pathT.AddFirst(cur);
        Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(cur.element.Angle)] = true;
        bool i_valid=false;
        bool Success = false;
        if ((testAngle = equalsGoal(cur.element)) < 360){
            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                Success = true;
                cur.element.Angle = Robots [0].goalSite.Angle;
                cur.element.Vertice = Robots [0].goalSite.Vertice;
            }
        }
        while (i_minOpen>=0 && !Success){//i_minOpen<0 : Open is empty
            /* get parent ( cur from Open[] ) */
/*            cur=Open [i_minOpen].First();
            Open [i_minOpen].RemoveFirst();
            if (Open [i_minOpen].Count() == 0){
                for (i_open=i_minOpen+1; i_open<Open.Length && Open [i_open].Count()==0; i_open++);
                i_minOpen =(i_open == Open.Length)?-1:i_open;
            }
            robotBuildParent(cur.element);
            /* generate 10 childs ( childO to Open[], childT to pathT ) */                          
            /* x+1 */
/*            if (Mathf.FloorToInt(cur.element.Vertice.x) < 127){
                childVertice.x = cur.element.Vertice.x + 1;
                if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(cur.element.Angle)]){
                    Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(cur.element.Angle)] = true;
                    robotBuildMe(1,0);
                    if (!collision()){
                        childVertice.y = cur.element.Vertice.y;
                        child = new Tnode(pathT.Count, cur.i_pathT, childVertice, cur.element.Angle);
                        tempU = getU(child.element);
                        Open [tempU].AddFirst(child);
                        pathT.AddLast(child);
                        if (tempU < i_minOpen || i_minOpen == -1)
                            i_minOpen = tempU;
                        if ((testAngle = equalsGoal(child.element)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.element.Angle = Robots [0].goalSite.Angle;
                                child.element.Vertice = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }
                    }
                }
            }
            /* x-1 */
/*            if (Mathf.FloorToInt(cur.element.Vertice.x) > 0){
                childVertice.x = cur.element.Vertice.x - 1;
                if (!Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(cur.element.Angle)]){
                    robotBuildMe(-1, 0);
                    Visited [Mathf.FloorToInt(childVertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(cur.element.Angle)] = true;
                    if (!collision()){
                        childVertice.y = cur.element.Vertice.y;
                        child = new Tnode(pathT.Count, cur.i_pathT, childVertice, cur.element.Angle);
                        tempU = getU(child.element);
                        Open [tempU].AddFirst(child);
                        pathT.AddLast(child);
                        if (tempU < i_minOpen || i_minOpen == -1)
                            i_minOpen = tempU;
                        if ((testAngle = equalsGoal(child.element)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.element.Angle = Robots [0].goalSite.Angle;
                                child.element.Vertice = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }
                    }
                }
            }
            /* y+1 */
/*            if (Mathf.FloorToInt(cur.element.Vertice.y) < 127){
                childVertice.y = cur.element.Vertice.y + 1;
                if (!Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.element.Angle)]){
                    robotBuildMe(0, 1);
                    Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.element.Angle)] = true;
                    if (!collision()){
                        childVertice.x = cur.element.Vertice.x;
                        child = new Tnode(pathT.Count, cur.i_pathT, childVertice, cur.element.Angle);

                        tempU = getU(child.element);
                        Open [tempU].AddFirst(child);
                        pathT.AddLast(child);
                        if (tempU < i_minOpen || i_minOpen == -1)
                            i_minOpen = tempU;
                        if ((testAngle = equalsGoal(child.element)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.element.Angle = Robots [0].goalSite.Angle;
                                child.element.Vertice = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }
                    }
                }
            }
            /* y-1 */
/*            if (Mathf.FloorToInt(cur.element.Vertice.y) > 0){
                childVertice.y = cur.element.Vertice.y - 1;
                if (!Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.element.Angle)]){
                    robotBuildMe(0, -1);
                    Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(childVertice.y), Mathf.FloorToInt(cur.element.Angle)] = true;
                    if (!collision()){
                        childVertice.x = cur.element.Vertice.x;
                        child = new Tnode(pathT.Count, cur.i_pathT, childVertice, cur.element.Angle);
                        tempU = getU(child.element);
                        Open [tempU].AddFirst(child);
                        pathT.AddLast(child);
                        if (tempU < i_minOpen || i_minOpen == -1)
                            i_minOpen = tempU;
                        if ((testAngle = equalsGoal(child.element)) < 360){
                            if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                                Success = true;
                                child.element.Angle = Robots [0].goalSite.Angle;
                                child.element.Vertice = Robots [0].goalSite.Vertice;
                                break;
                            }
                        }
                    }
                }
            }
            /* Angle+4 */                        
/*            childAngle = Normalize(cur.element.Angle + 4);
            if (!Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(childAngle)]){
                robotBuildMe(cur.element.Vertice, childAngle);
                Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(childAngle)] = true;
                if (!collision()){
                    child = new Tnode(pathT.Count, cur.i_pathT, cur.element.Vertice, childAngle);
                    tempU = getU(child.element);
                    Open [tempU].AddFirst(child);
                    pathT.AddLast(child);
                    if (tempU < i_minOpen || i_minOpen == -1)
                        i_minOpen = tempU;
                    if ((testAngle = equalsGoal(child.element)) < 360){
                        if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                            Success = true;
                            child.element.Angle = Robots [0].goalSite.Angle;
                            child.element.Vertice = Robots [0].goalSite.Vertice;
                            break;
                        }
                    }
                }
            }
            /* Angle-4 */
/*            childAngle = Normalize(cur.element.Angle - 4);
            if (!Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(childAngle)]){
                robotBuildMe(cur.element.Vertice, childAngle);
                Visited [Mathf.FloorToInt(cur.element.Vertice.x), Mathf.FloorToInt(cur.element.Vertice.y), Mathf.FloorToInt(childAngle)] = true;
                if (!collision()){
                    child = new Tnode(pathT.Count, cur.i_pathT, cur.element.Vertice, childAngle);
                    tempU = getU(child.element);
                    Open [tempU].AddFirst(child);
                    pathT.AddLast(child);
                    if (tempU < i_minOpen || i_minOpen == -1)
                        i_minOpen = tempU;
                    if ((testAngle = equalsGoal(child.element)) < 360){
                        if (Mathf.Abs(testAngle) <= 4 || Mathf.Abs(testAngle) >= 356){
                            Success = true;
                            child.element.Angle = Robots [0].goalSite.Angle;
                            child.element.Vertice = Robots [0].goalSite.Vertice;
                            break;
                        }
                    }
                }
            }  

        }   
        if (Success){
            ifShowPath = true;
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
            ifAniPath= true;
            robotPath=new int[numNode];
            robotPath[0]=0;
            int i_path=numNode-1;
            //            int num = Mathf.FloorToInt(((float)numNode) / 5f);
            //            int ii;
            //            sPATH = new WTPolygonSprite[num][][];
            sPATH = new WTPolygonSprite[numNode][][];//NOT FOR 5
            index = pathT_arr.Length - 1;
            numNode = 0;
            Tnode p;
            Vector2 from, to;
            Vector2[] screen=new Vector2[4];
            float vh;
            while (true){
                /*                for (ii=0; ii<4&&index!=0; ii++){
                    robotPath[i_path--]=index;
                    index = pathT_arr [index].iParent_pathT;
                }
                if (index == 0&&ii<4)
                    break;*/
/*                robotPath[i_path--]=index;//NOT FOR 5
                p = pathT_arr [index];
                index = p.iParent_pathT;
                sPATH [numNode] = new WTPolygonSprite[Robots [0].numPolygon][];
                for (int a=0; a<Robots[0].numPolygon; a++){
                    sPATH [numNode] [a] = new WTPolygonSprite[Robots [0].Polygons [a].numVertice];
                    for (int b=0; b<Robots[0].Polygons[a].numVertice; b++){
                        if (b == 0){
                            from = RotateBy(Robots [0].Polygons [a].Vertices [Robots [0].Polygons [a].numVertice - 1], p.element.Angle) + p.element.Vertice;
                        } else{
                            from = RotateBy(Robots [0].Polygons [a].Vertices [b - 1], p.element.Angle) + p.element.Vertice;
                        }
                        to = RotateBy(Robots [0].Polygons [a].Vertices [b], p.element.Angle) + p.element.Vertice;
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
                }
                if (index == -1)
                    break;
                numNode++;
            }
            print("the number of nodes in pathT : "+pathT.Count());
        }
    }*/

    LinkedList<Tnode> recDivide(int head,int tail){

        LinkedList<Tnode> temp = new LinkedList<Tnode>();

        if (tail == head){
            temp.AddLast(new Tnode(robotPathT [head]));
            return temp;
        }
        if ((tail - head) == 1){
            temp.AddLast(new Tnode(robotPathT [head]));
            temp.AddLast(new Tnode(robotPathT [tail]));
            return temp;
        }

        Configuration deff = new Configuration(robotPathT [tail].pos - robotPathT [head].pos, robotPathT [tail].ang - robotPathT [head].ang);

        if (deff.Angle >= 180)
        {
            deff.Angle -= 360;
        }
        else if(deff.Angle <= -180){
            deff.Angle += 360;
        }

        int maxDeff;
        float xDeff,yDeff,angDeff;
        maxDeff = (Mathf.Abs(deff.Vertice.x) >= Mathf.Abs(deff.Vertice.y)) ? Mathf.CeilToInt(Mathf.Abs(deff.Vertice.x)) : Mathf.CeilToInt(Mathf.Abs(deff.Vertice.y));
        if (Mathf.CeilToInt(Mathf.Abs(deff.Angle / 4f)) > maxDeff)
            maxDeff = Mathf.CeilToInt(Mathf.Abs(deff.Angle / 4f));

        if (maxDeff==0){
            return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
        }
//        else if(((maxDeff-deff.Vertice.x)>0&&(maxDeff-deff.Vertice.x)<1)|| ((maxDeff-deff.Vertice.x)>0&&(maxDeff-deff.Vertice.x)<1) && (maxDeff > (tail-head+1))){
//            return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
//        }
        else if((maxDeff+1) > (tail-head+1)){
            return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
        }

        xDeff = deff.Vertice.x / maxDeff;
        yDeff=deff.Vertice.y / maxDeff;
        angDeff=deff.Angle/maxDeff;

        robotBuildOthers();
        Vector2 tempV = new Vector2();
        float tempA;
        int i;
        for (i=0; i<=maxDeff; i++){
            tempV.x=robotPathT [head].pos.x+i*xDeff;
            tempV.y=robotPathT [head].pos.y+i*yDeff;
            tempA=robotPathT [head].ang+i*angDeff;
            robotBuildMe(tempV,tempA);
            if(collision())
               break;
            temp.AddLast(new Tnode(0,0,tempV,tempA));
        }
        if (i > maxDeff){
            return temp;
        } 
        else{
            temp = new LinkedList<Tnode>();

            if (deff.Angle >= 0){
                deff.Angle -= 360;
            }
            else{
                deff.Angle += 360;
            }

            maxDeff = Mathf.CeilToInt((Mathf.Abs(deff.Vertice.x) >= Mathf.Abs(deff.Vertice.y)) ? Mathf.Abs(deff.Vertice.x) : Mathf.Abs(deff.Vertice.y));
            if (Mathf.CeilToInt(Mathf.Abs(deff.Angle / 4f)) > maxDeff)
                maxDeff = Mathf.CeilToInt(Mathf.Abs(deff.Angle / 4f));

            if (maxDeff==0 || (maxDeff+1) > (tail-head+1)){
                return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
            }

            xDeff = deff.Vertice.x / maxDeff;
            yDeff=deff.Vertice.y / maxDeff;
            angDeff=deff.Angle/maxDeff;

            robotBuildOthers();
            for (i=0; i<=maxDeff; i++){
                tempV.x=robotPathT [head].pos.x+i*xDeff;
                tempV.y=robotPathT [head].pos.y+i*yDeff;
                tempA=robotPathT [head].ang+i*angDeff;
                robotBuildMe(tempV,tempA);
                if(collision())
                    break;
                temp.AddLast(new Tnode(0,0,tempV,tempA));
            }
            if (i > maxDeff){
                return temp;
            }
            else{
                return concat(recDivide(head,Mathf.FloorToInt((tail-head)/2)+head), recDivide(Mathf.FloorToInt((tail-head)/2+head)+1,tail));
            }
        }
    }
    LinkedList<Tnode> concat(LinkedList<Tnode> list1,LinkedList<Tnode> list2){
        foreach (Tnode item in list2)
            list1.AddLast(item);
        return list1;
    }
    void smoothing(){

        ifShowPath = true;

        robotPathT=new Tnode[robotPath.Length];
        for (int i=0; i<robotPath.Length; i++)
            robotPathT [i] = pathT_arr [robotPath [i]];
                
        LinkedList<Tnode> temp = recDivide(0, robotPath.Length - 1);

        pathT_arr = new Tnode[temp.Count];
        temp.CopyTo(pathT_arr, 0);
        robotPath=new int[pathT_arr.Count()];

        Vector2 from, to;
        Vector2[] screen=new Vector2[4];
        float vh;
//        sPATH = new WTPolygonSprite[Mathf.FloorToInt(((float)robotPath.Length) / 5f)][][];//for 5
        sPATH = new WTPolygonSprite[robotPath.Length][][];

        for (int i=0; i<pathT_arr.Count(); i++){

            robotPath [i] = i;

            sPATH [i] = new WTPolygonSprite[Robots [0].numPolygon][];
            for (int a=0; a<Robots[0].numPolygon; a++){
                sPATH [i] [a] = new WTPolygonSprite[Robots [0].Polygons [a].numVertice];
                for (int b=0; b<Robots[0].Polygons[a].numVertice; b++){
                    if (b == 0){
                        from = RotateBy(Robots [0].Polygons [a].Vertices [Robots [0].Polygons [a].numVertice - 1], pathT_arr[i].ang) + pathT_arr[i].pos;
                    } else{
                        from = RotateBy(Robots [0].Polygons [a].Vertices [b - 1], pathT_arr[i].ang) + pathT_arr[i].pos;
                    }
                    to = RotateBy(Robots [0].Polygons [a].Vertices [b], pathT_arr[i].ang) + pathT_arr[i].pos;
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
                    sPATH [i] [a] [b] = new WTPolygonSprite(new WTPolygonData(screen));
                    sPATH [i] [a] [b].color = Color.black;
                    Futile.stage.AddChild(sPATH [i] [a] [b]);
                }
            }
        }
    }
    IEnumerator animation(float timeinseconds){
//        inMouse = true;
        aniRobot=new WTPolygonSprite[Robots[0].numPolygon];
        Vector2 temp;
        for (int i=1; i<robotPath.Length; i++){
            for(int b=0;b<Robots[0].numPolygon;b++){ 
                if(i>1)
                    Futile.stage.RemoveChild(aniRobot[b]);
                toIPrint=new Vector2[Robots[0].Polygons[b].numVertice];
                for(int c=0;c<Robots[0].Polygons[b].numVertice;c++){
                    temp=RotateBy(Robots[0].Polygons[b].Vertices[c],pathT_arr[ robotPath[i] ].ang)+pathT_arr[ robotPath[i] ].pos;
                    toIPrint [c].x=temp.x * x_PlannerToCanvas;
                    toIPrint [c].y=temp.y * y_PlannerToCanvas;
                }
                aniRobot[b]=new WTPolygonSprite(new WTPolygonData(toIPrint));
                aniRobot[b].color= Color.cyan;
                Futile.stage.AddChild(aniRobot[b]);
            }
            yield return new WaitForSeconds(timeinseconds);
        }
        yield return new WaitForSeconds(timeinseconds*10);
        for(int b=0;b<Robots[0].numPolygon;b++)
            Futile.stage.RemoveChild(aniRobot[b]);
 //       inMouse = false;
    } 
    /*                            else{
                                print("testAngle<361");
                                print("original_testAngle:"+testAngle.ToString());
                                temp1=new Tnode(childO.i_pathT, childO.iParent_pathT, childO.element.Vertice,childO.element.Angle);
                                if(testAngle>180)
                                    testAngle-=360;
                                if(testAngle<-180)
                                    testAngle+=360;
                                tempAngle=testAngle;
                                if(testAngle>0){
                                    print("testAngle>0");
                                    print(testAngle);
                                    while(testAngle> 4){
//                                        childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, Normalize(childO.element.Angle + 4));
                                        childAngle=Normalize(childO.element.Angle + 4);
                                        robotBuildMe(childO.element.Vertice,childAngle);
                                        if (!collision()){
                                            testAngle-=4;
                                            childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, childAngle);
                                            if (Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)])
                                                continue;
                                            tempU = getU(childO.element);
                                            Open [tempU].AddFirst(childO);
                                            childT = new Tnode(childO.i_pathT, childO.iParent_pathT, childO.element.Vertice, childO.element.Angle);
                                            pathT.AddLast(childT);
                                            if (tempU < i_minOpen || i_minOpen == -1)
                                                i_minOpen = tempU;
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                        }else{
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                            break;
                                        }
                                    }
                                    if (testAngle<= 4){
                                        Success = true;
                                        childT.element.Angle = Robots[0].goalSite.Angle;
                                        childT.element.Vertice=Robots[0].goalSite.Vertice;
                                        print("second_ok");
                                        break;
                                    }
                                    testAngle=tempAngle-360;
                                    print("first_reverse");
                                    print(testAngle);
                                    childO = new Tnode(temp1.i_pathT,temp1.iParent_pathT, temp1.element.Vertice, temp1.element.Angle);
                                    while(testAngle< -4){
 //                                       childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, Normalize(childO.element.Angle - 4));
                                        childAngle=Normalize(childO.element.Angle - 4);
                                        robotBuildMe(childO.element.Vertice,childAngle);
                                        if (!collision()){
                                            testAngle+=4;
                                            childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, childAngle);
                                            if (Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)])
                                                continue;
                                            tempU = getU(childO.element);
                                            Open [tempU].AddFirst(childO);
                                            childT = new Tnode(childO.i_pathT, childO.iParent_pathT, childO.element.Vertice, childO.element.Angle);
                                            pathT.AddLast(childT);
                                            if (tempU < i_minOpen || i_minOpen == -1)
                                                i_minOpen = tempU;
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                        }else{
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                            break;
                                        }
                                    }
                                    if (testAngle>= -4){
                                        Success = true;
                                        childT.element.Angle = Robots[0].goalSite.Angle;
                                        childT.element.Vertice=Robots[0].goalSite.Vertice;
                                        print("third_ok");
                                        break;
                                    }
                                }else{       
                                    print("testAngle<0");
                                    print(testAngle);
                                    while(testAngle< -4){
//                                        childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, Normalize(childO.element.Angle - 4));
                                        childAngle=Normalize(childO.element.Angle - 4);
                                        robotBuildMe(childO.element.Vertice,childAngle);
                                        if (!collision()){
                                            testAngle+=4;
                                            childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, childAngle);
                                            if (Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childO.element.Angle)])
                                                continue;
                                            tempU = getU(childO.element);
                                            Open [tempU].AddFirst(childO);
                                            childT = new Tnode(childO.i_pathT, childO.iParent_pathT, childO.element.Vertice, childO.element.Angle);
                                            pathT.AddLast(childT);
                                            if (tempU < i_minOpen || i_minOpen == -1)
                                                i_minOpen = tempU;
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                        }else{
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                            break;
                                        }
                                    }
                                    if (testAngle>= -4){
                                        print("second_ok");
                                        Success = true;
                                        childT.element.Angle = Robots[0].goalSite.Angle;
                                        childT.element.Vertice=Robots[0].goalSite.Vertice;
                                        break;
                                    }
                                    testAngle=tempAngle+360;
                                    print("first_reverse");
                                    print(testAngle);
                                    childO = new Tnode(temp1.i_pathT,temp1.iParent_pathT, temp1.element.Vertice, temp1.element.Angle);
                                    while(testAngle>4){
 //                                       childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, Normalize(childO.element.Angle + 4));
                                        childAngle=Normalize(childO.element.Angle + 4);
                                        robotBuildMe(childO.element.Vertice,childAngle);
                                        if (!collision()){
                                            testAngle-=4;
                                            childO = new Tnode(pathT.Count, childO.i_pathT, childO.element.Vertice, childAngle);
                                            if (Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)])
                                                continue;
                                            tempU = getU(childO.element);
                                            Open [tempU].AddFirst(childO);
                                            childT = new Tnode(childO.i_pathT, childO.iParent_pathT, childO.element.Vertice, childO.element.Angle);
                                            pathT.AddLast(childT);
                                            if (tempU < i_minOpen || i_minOpen == -1)
                                                i_minOpen = tempU;
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                        }else{
                                            Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childAngle)]=true;
                                            break;
                                        }
                                    }
                                    if (testAngle<=4){
                                        print("third_ok");
                                        Success = true;
                                        childT.element.Angle = Robots[0].goalSite.Angle;
                                        childT.element.Vertice=Robots[0].goalSite.Vertice;
                                        break;
                                    }
                                }
                                print("goal_angle_test_failure");
                            }
                        }
                    }
                }
            }
        } */
    /*                    case 5:// x+1,y+1
                        if (Mathf.FloorToInt(cur.element.Vertice.x) < 127 && Mathf.FloorToInt(cur.element.Vertice.y) < 127){
                            childO = new Tnode(-1, -1, cur.element.Vertice, cur.element.Angle);
                            childO.element.Vertice.x++;
                            childO.element.Vertice.y++;
                            if (!Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childO.element.Angle)]){
                                robotBuildMe(1,1);
                                i_valid = true;
                            }
                        }
                        break;
                    case 6:// x+1,y-1 
                        if (Mathf.FloorToInt(cur.element.Vertice.x) < 127 && Mathf.FloorToInt(cur.element.Vertice.y) > 0){
                            childO = new Tnode(-1, -1, cur.element.Vertice, cur.element.Angle);
                            childO.element.Vertice.x++;
                            childO.element.Vertice.y--;
                            if (!Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childO.element.Angle)]){
                                robotBuildMe(1,-1);
                                i_valid = true;
                            }
                        }
                        break;
                    case 7:// x-1,y+1 
                        if (Mathf.FloorToInt(cur.element.Vertice.x) > 0 && Mathf.FloorToInt(cur.element.Vertice.y) < 127){
                            i_valid = true;
                            childO = new Tnode(-1, -1, cur.element.Vertice, cur.element.Angle);
                            childO.element.Vertice.x--;
                            childO.element.Vertice.y++;
                            if (!Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childO.element.Angle)]){
                                robotBuildMe(-1,1);
                                i_valid = true;
                            }
                        }
                        break;
                    case 8:// x-1,y-1 
                        if (Mathf.FloorToInt(cur.element.Vertice.x) > 0 && Mathf.FloorToInt(cur.element.Vertice.y) > 0) {
                            childO = new Tnode(-1, -1, cur.element.Vertice, cur.element.Angle);
                            childO.element.Vertice.x--;
                            childO.element.Vertice.y--;
                            if (!Visited [Mathf.FloorToInt(childO.element.Vertice.x), Mathf.FloorToInt(childO.element.Vertice.y), Mathf.FloorToInt(childO.element.Angle)]){
                                robotBuildMe(-1,-1);
                                i_valid = true;
                            }
                        }
                        break;*/

    /*    IEnumerator moveObstacle(float timeinseconds){  
        inMouse = true;
        Vector2 origCenter=new Vector2();
        Vector2 def_c = new Vector2 (curMouse.x-Obstacles[thObstacle].PosInPlanner.x,curMouse.y-Obstacles[thObstacle].PosInPlanner.y);
        yield return new WaitForSeconds(timeinseconds);       
        while (true){
            mouse_p = Input.mousePosition; 
            mouse_p.x = mouse_p.x*canvasToPlanner;
            mouse_p.y= mouse_p.y*canvasToPlanner;
            origCenter.Set(Obstacles[thObstacle].PosInPlanner.x,Obstacles[thObstacle].PosInPlanner.y);
            Obstacles [thObstacle].PosInPlanner.Set(mouse_p.x-def_c.x, mouse_p.y-def_c.y);            
            for (int b=0; b<Obstacles[thObstacle].numPolygon; b++){
                sOPolygons [thObstacle] [b].color = Color.clear;
                toIPrint = new Vector2[Obstacles [thObstacle].Polygons [b].numVertice];
                for (int c=0; c<Obstacles[thObstacle].Polygons[b].numVertice; c++){
                    Obstacles[thObstacle].Polygons[b].plannerIVertices[c]=Obstacles[thObstacle].Polygons[b].plannerIVertices[c]-origCenter+Obstacles [thObstacle].PosInPlanner;
                    toIPrint [c].x=Obstacles [thObstacle].Polygons [b].plannerIVertices [c].x * x_PlannerToCanvas;
                    toIPrint [c].y=Obstacles [thObstacle].Polygons [b].plannerIVertices [c].y * y_PlannerToCanvas;
                }
                sOPolygons [thObstacle] [b] = new WTPolygonSprite(new WTPolygonData(toIPrint));
                sOPolygons [thObstacle] [b].color = Color.gray;
                Futile.stage.AddChild(sOPolygons [thObstacle] [b]);
            }                
            yield return new WaitForSeconds(timeinseconds);
            if (Input.GetMouseButtonDown(2)){
                break;
            }            
        } 
        inMouse = false;
    }*/
    /*    public bool Intersection (Vector2 v1,Vector2 v2,Vector2 v3,Vector2 v4){
        Vector2 v122 = RotateBy(v2-v1, 90.0f);
        Vector2 v344 = RotateBy(v4 - v3, 90.0f);
        return (Vector2.Dot(v122, v3 - v1) * Vector2.Dot(v122, v4 - v1) < 0 && Vector2.Dot(v344, v1 - v3) * Vector2.Dot(v344, v2 - v3) < 0);
    }*/
}