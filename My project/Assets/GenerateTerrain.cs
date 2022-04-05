using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    [SerializeField] 
    // private GameObject blockPrefab;

    private float noiseScale = 0.05f;

    private int chunkSize = 20;

    private List<Mesh> meshes = new List<Mesh>();//used to avoid memory issues
    private List<GameObject> cubesListToCombine = new List<GameObject>();
    // Start is called before the first frame update

    /*void combineMesh(List<CombineInstance> blockData)
    {

    }*/


    void Generate()
    {
        //this list contains data for creating final mesh
        // List<CombineInstance> blockData = new List<CombineInstance>();
        //create a unit cube
        // MeshFilter blockMesh = Instantiante(PrimitiveType.Cube, Vector3.zero, Quaternion.identity).getComponent<MeshFilter>();

        for (int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                for(int z = 0; z < chunkSize; z++)
                {
                    float noiseValue = Perlin3D(x * noiseScale * Time.time, y * noiseScale* Time.time, z * noiseScale * Time.time);
                    if( noiseValue >= 0.5f)
                    {
                        
                        /*blockMesh.transform.position = new Vector3(x, y, z);
                        CombineInstance ci = new CombineInstance{
                            mesh = blockMesh;
                            transform = blockMesh.transform.localToWorldMatrix,
                        };
                        blockData.Add(ci);*/
                        // old approach with the primitive cube
                        GameObject cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        /*if you need add position, scale and color to the cube*/
                        cubeObject.transform.localPosition = new Vector3(x, y, z);
                        cubesListToCombine.Add(cubeObject); 
                    }
                    
                }
            }
        }

        // Destroy(blockMesh);
        // combineMesh(blockData);
    }

    // Update is called once per frame
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.D)) {
                Debug.Log("Deleting cubes");
                foreach(GameObject cube in cubesListToCombine)
                {
                    Destroy(cube);
                }
                // Generate();
            }

            if(Input.GetKeyDown(KeyCode.G))
            {
                Debug.Log("Generating cubes");
                Generate();
            }
    }

    public static float Perlin3D (float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }   

}

