using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Tile", menuName = "World/Tile")]
public class TileSO : ScriptableObject
{

    public enum TileTypeGeneral
    {
        SeaDeep,
        SeaShallow,
        SeaShore,
        Plains,
        Hills,
        Mountains,
    }

    public enum TileTypeBio
    {
        Grass,
        Conifers,
        Foliaceous,
        Desert,
        Snow,
        Water,
    }


    public Sprite tileSprite;

    public TileTypeGeneral tileGeneralType;
    public TileTypeBio tileBioType;


    public int minGrassEatingBugs;
    public int maxGrassEatingBugs;

    public int minNectarBugs;
    public int maxNectarBugs;
    
    public int minSunLit;
    public int maxSunLit;

    public int minPhDirt;
    public int maxPhDirt;

    public int minGrassEatingAnimals;
    public int maxGrassEatingAnimals;

}
