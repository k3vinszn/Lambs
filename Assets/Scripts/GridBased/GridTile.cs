using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    // ====================================
    // == COMPONENT REFERENCES & SETTINGS ==
    // ====================================
    public Color MouseOverColor;
    private Color OriginalColor;
    private MeshRenderer m_Renderer;
    public Material m;

    // ================================
    // == GRID TEXTURE CONFIGURATION ==
    // ================================
    public bool renderTextureDetail = true;
    public float gridBlockIntensity = 0.05f;
    public GameObject gridTexture;

    // ============================
    // == PATHFINDING PARAMETERS ==
    // ============================
    public bool debugPathfindingCosts = false;
    public int fCost = 0;
    public int hCost = 0;
    public int gCost = 0;
    public GameObject cameFromTile = null;

    // ==============================
    // == PATHFINDING DEBUG OBJECTS ==
    // ==============================
    public TextMesh fCostOBJ;
    public TextMesh gCostOBJ;
    public TextMesh hCostOBJ;

    // =======================
    // == TILE STATE FLAGS ==
    // =======================
    public bool MouseOver = false;
    public bool selected = false;

    // ========================
    // == SPRITE UV OFFSETS ==
    // ========================
    public Vector2 leftSprite = new Vector2(0.5f, 0.5f);
    public Vector2 rightSprite = new Vector2(0, 0.5f);
    public Vector2 upSprite = new Vector2(0, 0);
    public Vector2 endSprite = new Vector2(0.5f, 0);

    // ===============================
    // == DECORATION/ART GENERATION ==
    // ===============================
    public bool generateArtAssets = true;
    public int grassBladeAmount = 10;
    public int flowerAmountProbability = 100;
    public int grassPatchAmountProbability = 20;

    // ========================
    // == TILE STATE ENUM ==
    // ========================
    public enum TileState { Left, Right, Forward, End }
    public TileState tileState = TileState.Forward;

    // =====================
    // == INITIALIZATION ==
    // =====================
    void Awake()
    {
        m_Renderer = GetComponent<MeshRenderer>();

        if (renderTextureDetail)
        {
            Vector2 offsetRandomized = new Vector2(Random.Range(0f, 1.01f), Random.Range(0f, 1.01f));
            float scaleRandomized = Random.Range(0.3f, 0.5f);

            m_Renderer.material.mainTextureScale = new Vector2(scaleRandomized, scaleRandomized);
            m_Renderer.material.mainTextureOffset = offsetRandomized;
        }

        Color variation = new Color(Random.Range(-gridBlockIntensity, gridBlockIntensity), 0, 0, 0);
        m_Renderer.material.color -= variation;

        OriginalColor = m_Renderer.material.color;
        m = m_Renderer.material;
        tileState = TileState.Forward;

        if (!debugPathfindingCosts)
        {
            fCostOBJ.gameObject.SetActive(false);
            gCostOBJ.gameObject.SetActive(false);
            hCostOBJ.gameObject.SetActive(false);
        }

        if (generateArtAssets)
        {
            Beautify();
        }
    }

    // =================
    // == MAIN UPDATE ==
    // =================
    void Update()
    {
        if (!Game.ActiveLogic) return;

        gridTexture.SetActive(Input.GetMouseButton(0) && !GridManager.startPuzzle);
        m_Renderer.material.color = selected ? MouseOverColor : OriginalColor;
    }

    // ===========================
    // == MOUSE INTERACTIONS ==
    // ===========================
    void OnMouseEnter() => MouseOver = true;
    void OnMouseExit() => MouseOver = false;

    // ================================
    // == PATHFINDING COST CALCULATION ==
    // ================================
    public void CalculateFCost()
    {
        fCost = gCost + hCost;

        if (!debugPathfindingCosts) return;

        fCostOBJ.text = fCost.ToString();
        gCostOBJ.text = gCost.ToString();
        hCostOBJ.text = hCost.ToString();

        fCostOBJ.text = TruncateText(fCostOBJ.text, 3);
        gCostOBJ.text = TruncateText(gCostOBJ.text, 3);
        hCostOBJ.text = TruncateText(hCostOBJ.text, 3);
    }

    // ==============================
    // == HELPER METHOD: TRUNCATION ==
    // ==============================
    string TruncateText(string text, int maxLength) =>
        text.Length <= maxLength ? text : text.Substring(0, maxLength);

    // =========================
    // == SPRITE UV CONTROL ==
    // =========================
    void UpdateSprite(Vector2 sprite)
    {
        m.SetTextureOffset("_MainTex", sprite);
    }

    // ==========================
    // == DECORATION GENERATION ==
    // ==========================
    void Beautify()
    {
        float emissionOffset = 0.25f;

        for (int i = 1; i <= 3; i++)
        {
            for (int j = 0; j < grassBladeAmount; j++)
            {
                SpawnGrassBlade($"Decorative/GrassBlade{i}", emissionOffset);
            }
        }

        TrySpawnDecoration("Decorative/Flowers_Red", 10);
        TrySpawnDecoration("Decorative/Flowers_White", 5);
        TrySpawnDecoration("Decorative/Flowers_Blue", 1);

        TrySpawnDecoration("Decorative/GrassLow", 2);
        TrySpawnDecoration("Decorative/GrassHigh", 1);
    }

    // ==========================================
    // == SPAWN INDIVIDUAL GRASS BLADE OBJECTS ==
    // ==========================================
    void SpawnGrassBlade(string resourcePath, float emissionOffset)
    {
        GameObject blade = Instantiate(Resources.Load<GameObject>(resourcePath), transform.position, Quaternion.identity, transform);
        blade.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

        Renderer bladeRenderer = blade.transform.GetChild(0).GetComponent<Renderer>();
        bladeRenderer.material.color = m_Renderer.material.color;
        bladeRenderer.material.SetColor("_EmissionColor", m_Renderer.material.color - new Color(emissionOffset, emissionOffset, emissionOffset, 1));
    }

    // =================================
    // == DECORATION SPAWNER HELPERS ==
    // =================================
    void TrySpawnDecoration(string resourcePath, int triggerValue)
    {
        int randomValue = Random.Range(1, resourcePath.Contains("Flower") ? flowerAmountProbability : grassPatchAmountProbability);
        if (randomValue == triggerValue)
        {
            GameObject decor = Instantiate(Resources.Load<GameObject>(resourcePath), transform.position, Quaternion.identity, transform);
            decor.transform.localPosition = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
        }
    }
}
