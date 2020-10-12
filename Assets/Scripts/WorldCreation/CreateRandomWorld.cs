using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRandomWorld : MonoBehaviour
{

    struct TemporaryTile
    {
        public int positionX;
        public int positionY;

        public int water;
        public int temperature;
        public float height;
        public TileSO.TileTypeGeneral tileGeneralType;
        public TileSO.TileTypeBio tileBioType;
    }



    public GameObject tile;

    public TileSO[] allPossibleTiles;

    [Header("World Dim")]
    public int x = 200;
    public int y = 100;
    public float seaScaler = 1.1f;

    [Header("Perlin Noise Scalers")]
    public float scaler1 = 2.0f;
    public float scaler2 = 2.0f;
    public float scaler3 = 3.0f;

    public float zoomX1 = 0.2f;
    public float zoomY1 = 0.2f;

    public float zoomX2 = 2.0f;
    public float zoomY2 = 2.0f;

    public float zoomX3 = 4.0f;
    public float zoomY3 = 4.0f;


    [Header("World settings")]
    public int temperatureAtEquator = 35;
    public float temperatureReduction = 0.5f;
    public float waterSourceChance = 0.2f;
    public int numWaterSources = 4;
    public int waterReduction = 10;

    public float percentDeepSea = 0.2f;
    public float percentShallowSea = 0.2f;
    public float percentCoastalSea = 0.1f;
    public float percentPlains = 0.2f;
    public float percentHills = 0.2f;
    //public float percentMountains = 0.1f;

    private GameObject[,] grid;
    private TemporaryTile[,] theoreticalGrid;

    private TemporaryTile[] nPeaksWaterSource;
    private float[] minMaxAvgValues;
    private float[] ranges;

    private void Awake()
    {
        nPeaksWaterSource = new TemporaryTile[numWaterSources];
        minMaxAvgValues = GetMinAvgMaxValues(x, y);
        ranges = ComputeHeightRanges(minMaxAvgValues, percentDeepSea, percentShallowSea, percentCoastalSea, percentPlains, percentHills);

        grid = CreateTemporaryGrid(x, y);
    }

    // Start is called before the first frame update
    void Start()
    {
        //grid = CreateGrid(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //DestroyAll(x, y);
            //grid = CreateGrid(x, y);
        }
    }

    private GameObject[,] CreateTemporaryGrid(int x, int y)
    {
        TemporaryTile[,] gridWorld = new TemporaryTile[x, y];
        GameObject[,] actualGridWorld = new GameObject[x, y];
        BoxCollider2D bc2d = tile.GetComponent<BoxCollider2D>();
        int waterCounter = 0;
        // first pass
        // create height, general types, water sources, temperatures
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                float height = CreateValleyMountain(i, j);
                gridWorld[i, j] = new TemporaryTile { 
                    height = height, 
                    positionX = i, 
                    positionY = j, 
                    tileGeneralType = GetTileTypeGeneral(height, ranges),
                    temperature = GetTileTemperature(GetTileTypeGeneral(height, ranges), j),
                };
                
                if(Random.Range(0f, 1.0f) <= waterSourceChance && waterCounter < numWaterSources)
                {
                    gridWorld[i, j].water = 100;
                    nPeaksWaterSource[waterCounter] = gridWorld[i, j];
                    waterCounter++;
                }
            }
        }

        // second pass
        // set water levels
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if(gridWorld[i, j].water != 100 && 
                    gridWorld[i, j].tileGeneralType != TileSO.TileTypeGeneral.SeaDeep && 
                    gridWorld[i, j].tileGeneralType != TileSO.TileTypeGeneral.SeaShallow && 
                    gridWorld[i, j].tileGeneralType != TileSO.TileTypeGeneral.SeaShore)
                {
                    gridWorld[i, j].water = GetTileWater(gridWorld[i, j].positionX, gridWorld[i, j].positionY, nPeaksWaterSource);
                }

            }
        }

        // third pass
        // substitute the temporary tiles with actual game objects based on the calculated data
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                actualGridWorld[i, j] = Instantiate(tile,
                                new Vector3(bc2d.size.x * i + bc2d.size.x / 2.0f,
                                bc2d.size.y * j + bc2d.size.y / 2.0f,
                                0),
                                Quaternion.identity);
                TileScript s = actualGridWorld[i, j].GetComponent<TileScript>();
                gridWorld[i, j].tileBioType = ReturnTileBiome(gridWorld[i, j]);
                s.InitializeTile(GetCorrectTileSO(gridWorld[i, j]));
                s.water = gridWorld[i, j].water;
                s.temperature = gridWorld[i, j].temperature;
            }
        }

        return actualGridWorld;
    }

    private TileSO GetCorrectTileSO(TemporaryTile tile)
    {
        TileSO result = allPossibleTiles[0];
        for (int i = 0; i < allPossibleTiles.Length; i++)
        {
            if(tile.tileBioType == allPossibleTiles[i].tileBioType && 
                tile.tileGeneralType == allPossibleTiles[i].tileGeneralType)
            {
                result = allPossibleTiles[i];
                break;
            }
        }
        return result;
    }



    private TileSO.TileTypeBio ReturnTileBiome(TemporaryTile tile)
    {
        TileSO.TileTypeBio result;
        if(tile.tileGeneralType == TileSO.TileTypeGeneral.SeaDeep ||
            tile.tileGeneralType == TileSO.TileTypeGeneral.SeaShallow ||
            tile.tileGeneralType == TileSO.TileTypeGeneral.SeaShore)
        {
            result = TileSO.TileTypeBio.Water;
        }
        else
        {
            // temperature ranges
            if(tile.temperature <= 0)
            {
                result = TileSO.TileTypeBio.Snow;
            }else if(tile.temperature <= 20)
            {
                // trava, iglavci, desert
                if(tile.water <= 50)
                {
                    result = TileSO.TileTypeBio.Desert;
                }else if(tile.water <= 120)
                {
                    result = TileSO.TileTypeBio.Grass;
                }
                else
                {
                    result = TileSO.TileTypeBio.Conifers;
                }

                if(tile.tileGeneralType == TileSO.TileTypeGeneral.Mountains)
                {
                    result = TileSO.TileTypeBio.Snow;
                }
            }
            else
            {
                // trava, listavci, desert
                if (tile.water <= 50)
                {
                    result = TileSO.TileTypeBio.Desert;
                }
                else if (tile.water <= 120)
                {
                    result = TileSO.TileTypeBio.Grass;
                }
                else
                {
                    result = TileSO.TileTypeBio.Foliaceous;
                }

                if (tile.tileGeneralType == TileSO.TileTypeGeneral.Mountains)
                {
                    result = TileSO.TileTypeBio.Desert;
                }
            }
        }
        return result;
    }

    private int GetTileTemperature(TileSO.TileTypeGeneral generalType, int y)
    {
        int equatorDistance = Mathf.Abs(y - GetEquator());
        int tileTemperature = Mathf.RoundToInt(temperatureAtEquator - temperatureReduction * equatorDistance);
        switch (generalType)
        {
            case TileSO.TileTypeGeneral.Plains:break;
            case TileSO.TileTypeGeneral.Hills: tileTemperature -= 4; break;
            case TileSO.TileTypeGeneral.Mountains: tileTemperature -= 10; break;
            case TileSO.TileTypeGeneral.SeaShore: break;
            case TileSO.TileTypeGeneral.SeaShallow: tileTemperature -= 1; break;
            case TileSO.TileTypeGeneral.SeaDeep: tileTemperature -= 5; break;
        }
        return tileTemperature;
    }

    private int GetTileWater(int posX, int posY, TemporaryTile[] waterSources)
    {
        int result = 0;
        for (int i = 0; i < waterSources.Length; i++)
        {
            int distX = (waterSources[i].positionX - posX) * (waterSources[i].positionX - posX);
            int distY = (waterSources[i].positionY - posY) * (waterSources[i].positionY - posY);
            int dist = Mathf.RoundToInt(Mathf.Sqrt(distX + distY));

            int waterContribution = waterSources[i].water - WaterFunction(dist);
            if (waterContribution < 0) waterContribution = 0;
            result += waterContribution;
        }
        
        return result;
    }

    private int WaterFunction(int dist)
    {
        float coefficientWater1 = 1.2f;
        float coefficientWater2 = 2f;
        float constantWaterFunction = 2f;
        return Mathf.RoundToInt(coefficientWater1 * dist * dist + coefficientWater2 * dist + constantWaterFunction);
    }

    private int GetEquator()
    {
        return y / 2;
    }



    /*private GameObject[, ] CreateGrid(int x, int y)
    {
        GameObject[,] gridWorld = new GameObject[x, y];
        BoxCollider2D bc2d = land.transform.GetComponent<BoxCollider2D>();
        float seaLevel = GetSeaLevel(x, y);
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                float height = CreateValleyMountain(i, j);
                if (height > seaLevel)
                {
                    gridWorld[i, j] = Instantiate(land, 
                           new Vector3(bc2d.size.x * i + bc2d.size.x / 2.0f,
                           bc2d.size.y * j + bc2d.size.y / 2.0f,
                           0), 
                           Quaternion.identity);
                }
                else
                {
                    gridWorld[i, j] = Instantiate(sea,
                           new Vector3(bc2d.size.x * i + bc2d.size.x / 2.0f,
                           bc2d.size.y * j + bc2d.size.y / 2.0f,
                           0),
                           Quaternion.identity);
                }
            }
        }

        return gridWorld;
    }*/

    private float[] GetMinAvgMaxValues(int x, int y)
    {
        // 0 -> min, 1 -> avg, 2-> max
        float[] result = new float[3];
        float maxValue = int.MinValue;
        float minValue = int.MaxValue;
        float sum = 0.0f;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                float value = CreateValleyMountain(i, j);
                sum += value;
                if(value < minValue)
                {
                    minValue = value;
                }
                if(value > maxValue)
                {
                    maxValue = value;
                }
            }
        }
        result[0] = minValue;
        result[1] = (sum / (x * y));
        result[2] = maxValue;
        return result;
    }

    private float[] ComputeHeightRanges(float[] minMaxAvgValues, float p1, float p2, float p3, float p4, float p5)
    {
        float[] ranges = new float[5];
        float minmaxDelta = Mathf.Abs(minMaxAvgValues[0] - minMaxAvgValues[2]);

        // deep sea max range, everything bellow is deep sea
        ranges[0] = minMaxAvgValues[0] + minmaxDelta * p1;
        // shallow sea max range
        ranges[1] = ranges[0] + minmaxDelta * p2;
        // coastal sea max range
        ranges[2] = ranges[1] + minmaxDelta * p3;
        // plains max range
        ranges[3] = ranges[2] + minmaxDelta * p4;
        // hills max range, everything above are mountains
        ranges[4] = ranges[3] + minmaxDelta * p5;
        return ranges;
    }


    private TileSO.TileTypeGeneral GetTileTypeGeneral(float height, float[] heightRanges)
    {
        TileSO.TileTypeGeneral result = TileSO.TileTypeGeneral.SeaDeep;
        for (int i = 0; i < heightRanges.Length; i++)
        {
            if(height > heightRanges[i])
            {
                // geographic tile types are in order in the tiletype general, so we can just increase the value
                result++;
            }
            else
            {
                break;
            }
        }
        return result;
    }



    private float CreateValleyMountain(int x, int y)
    {
        return scaler1 * Mathf.PerlinNoise(x * zoomX1, y * zoomY1) + scaler2 * Mathf.PerlinNoise(x * zoomX2, y * zoomY2) + scaler3 * Mathf.PerlinNoise(x * zoomX3, y * zoomY3);
    }

    private void DestroyAll(int x, int y)
    {
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                Destroy(grid[i, j]);
            }
        }
        grid = new GameObject[x, y];
    }
}
