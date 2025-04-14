using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    public Color MouseOverColor;
    private Color OriginalColor;
    private MeshRenderer m_Renderer;
    public Material m;

    public bool renderTextureDetail = true;

    public float gridBlockIntensity = 0.05f;
    public GameObject gridTexture;

    //pathfinding stuff
    public bool debugPathfindingCosts = false;

    public int fCost = 0;
    public int hCost = 0;
    public int gCost = 0;
    public GameObject cameFromTile = null;

    public TextMesh fCostOBJ;
    public TextMesh gCostOBJ;
    public TextMesh hCostOBJ;

    public bool MouseOver = false;
    public bool selected = false;

    public Vector2 leftSprite = new Vector2(0.5f,0.5f);
    public Vector2 rightSprite = new Vector2(0, 0.5f);
    public Vector2 upSprite = new Vector2(0, 0);
    public Vector2 endSprite = new Vector2(0.5f, 0);

    public bool generateArtAssets = true;
    public int grassBladeAmount = 10;
    public int flowerAmountProbability = 100;
    public int grassPatchAmountProbability = 20;

    public enum TileState
    {
        Left,
        Right,
        Forward,
        End
    }

    public TileState tileState = TileState.Forward;

    void Awake()
    {
        //Fetch the mesh renderer component from the GameObject
        m_Renderer = GetComponent<MeshRenderer>();

        if (renderTextureDetail)
        {
            //randomize offset
            Vector2 offsetRandomized = new Vector2(Random.Range(0.0f, 1.01f), Random.Range(0.0f, 1.01f));
            float scaleRandomized = Random.Range(0.3f, 0.5f);

            m_Renderer.material.mainTextureScale = new Vector2(scaleRandomized, scaleRandomized);
            m_Renderer.material.mainTextureOffset = offsetRandomized;
        }


        m_Renderer.material.color = m_Renderer.material.color - new Color(Random.Range(-gridBlockIntensity, gridBlockIntensity), 0, 0, 0);

        //Fetch the original color of the GameObject
        OriginalColor = m_Renderer.material.color;
        m = m_Renderer.material;

        tileState = TileState.Forward;
        //m.SetTextureOffset("_MainTex", upSprite);

        if (!debugPathfindingCosts)
        {
            fCostOBJ.gameObject.SetActive(false);
            gCostOBJ.gameObject.SetActive(false);
            hCostOBJ.gameObject.SetActive(false);
        }

        if(generateArtAssets)
        {
            //generate some art stuff here
            Beautify();
        }
    }

    void Update()
    {
        if (Game.ActiveLogic)
        {
            if (Input.GetMouseButton(0) && !GridManager.startPuzzle)
            {
                gridTexture.SetActive(true);
            }
            else
            {
                gridTexture.SetActive(false);
            }


            if (selected)
            {
                if (m_Renderer.material.color != MouseOverColor)
                    m_Renderer.material.color = MouseOverColor;
            }
            else
            {
                if (m_Renderer.material.color != OriginalColor)
                    m_Renderer.material.color = OriginalColor;
            }
        }
            
    }

    void UpdateSprite(Vector2 sprite)
    {
        m.SetTextureOffset("_MainTex", sprite);

        //if(m.GetTextureOffset("_MainTex") == 
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;

        if(debugPathfindingCosts)
        {
            fCostOBJ.text = "" + fCost;
            gCostOBJ.text = "" + gCost;
            hCostOBJ.text = "" + hCost;

            int lengthF = 3;
            int lengthG = 3;
            int lengthH = 3;

            if (fCostOBJ.text.Length < 3)
                lengthF = fCostOBJ.text.Length;

            if (hCostOBJ.text.Length < 3)
                lengthH = hCostOBJ.text.Length;

            if (gCostOBJ.text.Length < 3)
                lengthG = gCostOBJ.text.Length;


            fCostOBJ.text = fCostOBJ.text.Substring(0, lengthF);
            gCostOBJ.text = gCostOBJ.text.Substring(0, lengthG);
            hCostOBJ.text = hCostOBJ.text.Substring(0, lengthH);
        }
    }

    void OnMouseEnter()
    {
        MouseOver = true;          
    }

    void OnMouseExit()
    {
        MouseOver = false;
    }

    void Beautify()
    {
        int randomFlowers = Random.Range(1, flowerAmountProbability);
        int randomGrass = Random.Range(1, grassPatchAmountProbability);

        float colorSBTRKTVar = 0.25f;

        for (int i = 0; i < grassBladeAmount; i++)
        {
            GameObject grassBlade1 = Instantiate((GameObject)Resources.Load("Decorative/GrassBlade1"), transform.position, Quaternion.identity, transform);
            grassBlade1.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f), 0);
            grassBlade1.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = m_Renderer.material.color;
            grassBlade1.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", m_Renderer.material.color - new Color(colorSBTRKTVar, colorSBTRKTVar, colorSBTRKTVar, 1));
        }

        for (int i = 0; i < grassBladeAmount; i++)
        {
            GameObject grassBlade2 = Instantiate((GameObject)Resources.Load("Decorative/GrassBlade2"), transform.position, Quaternion.identity, transform);
            grassBlade2.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f), 0);
            grassBlade2.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = m_Renderer.material.color;
            grassBlade2.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", m_Renderer.material.color - new Color(colorSBTRKTVar, colorSBTRKTVar, colorSBTRKTVar, 1));
        }

        for (int i = 0; i < grassBladeAmount; i++)
        {
            GameObject grassBlade3 = Instantiate((GameObject)Resources.Load("Decorative/GrassBlade3"), transform.position, Quaternion.identity, transform);
            grassBlade3.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f), 0);
            grassBlade3.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = m_Renderer.material.color;
            grassBlade3.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetColor("_EmissionColor", m_Renderer.material.color - new Color(colorSBTRKTVar, colorSBTRKTVar, colorSBTRKTVar, 1));
        }

        switch (randomFlowers)
        {
            case 10:
                GameObject flower3 = Instantiate((GameObject)Resources.Load("Decorative/Flowers_Red"), transform.position, Quaternion.identity, transform);
                flower3.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f),0);
                break;
            case 5:
                GameObject flower2 = Instantiate((GameObject)Resources.Load("Decorative/Flowers_White"), transform.position, Quaternion.identity, transform);
                flower2.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f),0);
                break;
            case 1:
                GameObject flower1 = Instantiate((GameObject)Resources.Load("Decorative/Flowers_Blue"), transform.position, Quaternion.identity, transform);
                flower1.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f),0);
                break;
            default:
                //print("no need for flowers");
                break;
        }

        switch (randomGrass)
        {
            case 2:
                GameObject grassLo = Instantiate((GameObject)Resources.Load("Decorative/GrassLow"), transform.position, Quaternion.identity, transform);
                grassLo.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f), 0);
                break;
            case 1:
                GameObject grassHi = Instantiate((GameObject)Resources.Load("Decorative/GrassHigh"), transform.position, Quaternion.identity, transform);
                grassHi.transform.localPosition = new Vector3(Random.Range(0.5f, -0.5f), Random.Range(0.5f, -0.5f), 0);
                break;
            default:
                //print("no need for grass");
                break;
        }


    }
}
