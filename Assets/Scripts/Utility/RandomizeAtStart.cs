using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeAtStart : MonoBehaviour
{
    public GameObject object2Affect;

    [Space] //This is how to space between variables

    public bool RandomizeRotation = true;
    public Vector2 rotationRandom = new Vector2(0, 360);

    [Space] //This is how to space between variables

    public bool RandomizeScale = false;
    public Vector3 minScale = new Vector3(1, 1, 1);
    public Vector3 maxScale = new Vector3(1, 1, 1);

    public bool lockYtoFloorLevel = true;

    [Space] //This is how to space between variables

    public bool RandomizeColor = false;
    public string colorName = "_MainTex";
    public Color minColor = new Color(1, 1, 1, 1);
    public Color maxColor = new Color(1, 1, 1, 1);



    // Start is called before the first frame update
    void Awake()
    {
        if (RandomizeRotation)
        {
            
            if (object2Affect != null)
            {
                object2Affect.transform.eulerAngles = new Vector3(transform.rotation.x, (int)Random.Range(rotationRandom.x, rotationRandom.y), transform.rotation.z);
            }
            else
            {
                transform.eulerAngles = new Vector3(transform.rotation.x, (int)Random.Range(rotationRandom.x, rotationRandom.y), transform.rotation.z);
            }

            
        }

        if (RandomizeScale)
        {
            if (object2Affect != null)
            {
                object2Affect.transform.localScale = new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));
            }
            else
            {
                transform.localScale = new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));
            }
        }

        if (lockYtoFloorLevel)
        {
            

            if (object2Affect != null)
            {
                object2Affect.transform.localPosition = new Vector3(object2Affect.transform.localPosition.x, object2Affect.transform.localScale.y / 2, object2Affect.transform.localPosition.z);
            }
            else
            {
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localScale.y / 2, transform.localPosition.z);
            }
        }

        if (RandomizeColor)
        {
            if(object2Affect != null)
            {
                object2Affect.GetComponent<MeshRenderer>().material.SetColor(colorName,
                new Color(Random.Range(minColor.r, maxColor.r),
                            Random.Range(minColor.g, maxColor.g),
                            Random.Range(minColor.b, maxColor.b),
                            Random.Range(minColor.a, maxColor.a)));
            }
            else
            {
                GetComponent<MeshRenderer>().material.SetColor(colorName,
                new Color(Random.Range(minColor.r, maxColor.r),
                            Random.Range(minColor.g, maxColor.g),
                            Random.Range(minColor.b, maxColor.b),
                            Random.Range(minColor.a, maxColor.a)));
            }
            
            
        }


    }

}
