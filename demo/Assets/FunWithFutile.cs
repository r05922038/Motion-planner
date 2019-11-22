
using UnityEngine;

public class FunWithFutile : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
		FutileParams futileParams = new FutileParams(true, true, true, true);		
		futileParams.AddResolutionLevel(1024, 1, 1, "");		
		futileParams.origin = new Vector2(0.5f, 0.5f);
		Futile.instance.Init(futileParams);		
		Futile.atlasManager.LoadImage("background");
		FSprite fSprite = new FSprite("background");
		Futile.stage.AddChild(fSprite);
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}
}