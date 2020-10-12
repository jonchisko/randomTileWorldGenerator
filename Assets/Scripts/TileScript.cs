using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    public Sprite tileSprite;

    public int water;
    public int temperature;
    public TileSO.TileTypeGeneral tileGeneralType;
    public TileSO.TileTypeBio tileBioType;


    public int grassEatingBugs;

    public int nectarBugs;

    public int sunLit;

    public int phDirt;

    public int grassEatingAnimals;

    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeTile(TileSO t)
    {
        grassEatingAnimals = Random.Range(t.minGrassEatingAnimals, t.maxGrassEatingAnimals + 1);
        nectarBugs = Random.Range(t.minNectarBugs, t.maxNectarBugs + 1);
        sunLit = Random.Range(t.minSunLit, t.maxSunLit + 1);
        phDirt = Random.Range(t.minPhDirt, t.maxPhDirt + 1);
        grassEatingBugs = Random.Range(t.minGrassEatingBugs, t.maxGrassEatingBugs + 1);
        tileSprite = t.tileSprite;
        tileGeneralType = t.tileGeneralType;
        tileBioType = t.tileBioType;

        _sr.sprite = tileSprite;
    }

}
