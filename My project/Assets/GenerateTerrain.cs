using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    // [SerializeField] 
    private float noiseScale = 0.05f;

    // [SerializeField]
    private int chunkSize = 200;

    private List<Mesh> meshes = new List<Mesh>();//used to avoid memory issues
    private List<GameObject> cubesListToCombine = new List<GameObject>();
    // Start is called before the first frame update

    void combineMesh(List<CombineInstance>  blockData)
    {
    //divide meshes into groups of 65536 vertices. Meshes can only have 65536 vertices so we need to divide them up into multiple block lists.

        List<List<CombineInstance>> blockDataLists = new List<List<CombineInstance>>();//we will store the meshes in a list of lists. each sub-list will contain the data for one mesh. same data as blockData, different format.
        int vertexCount = 0;
        blockDataLists.Add(new List<CombineInstance>());//initial list of mesh data
        for (int i = 0; i < blockData.Count; i++) {//go through each element in the previous list and add it to the new list.
            vertexCount += blockData[i].mesh.vertexCount;//keep track of total vertices
            if (vertexCount > 65536) {//if the list has reached it's capacity. if total vertex count is more then 65536, reset counter and start adding them to a new list.
                vertexCount = 0;
                blockDataLists.Add(new List<CombineInstance>());
                i--;
            } else {//if the list hasn't yet reached it's capacity. safe to add another block data to this list 
                blockDataLists.Last().Add(blockData[i]);//the newest list will always be the last one added
            }
        }


        Transform container = new GameObject("Meshys").transform;//create container object
        foreach (List<CombineInstance> data in blockDataLists) {//for each list (of block data) in the list (of other lists)
            GameObject g = new GameObject("Meshy");//create gameobject for the mesh
            g.transform.parent = container;//set parent to the container we just made
            MeshFilter mf = g.AddComponent<MeshFilter>();//add mesh component
            MeshRenderer mr = g.AddComponent<MeshRenderer>();//add mesh renderer component
            mr.material = new Material(Shader.Find("Diffuse"));//set material to avoid evil pinkness of missing texture
            mf.mesh.CombineMeshes(data.ToArray());//set mesh to the combination of all of the blocks in the list
            meshes.Add(mf.mesh);//keep track of mesh so we can destroy it when it's no longer needed
            //g.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh;//setting colliders takes more time. disabled for testing.
        }

    }


    void Generate()
    {
        GameObject blockPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);

        List<CombineInstance> blockData = new List<CombineInstance>();//this will contain the data for the final mesh
        MeshFilter blockMesh = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity).GetComponent<MeshFilter>();//create a unit cube and store the mesh from it
        float radius = chunkSize / 2;
        for (int x = 0; x < chunkSize; x++)
        {
            for(int y = 0; y < chunkSize; y++)
            {
                for(int z = 0; z < chunkSize; z++)
                {
                    float noiseValue = Perlin3D(x * noiseScale * Time.time, y * noiseScale* Time.time, z * noiseScale * Time.time);
                    if( noiseValue >= 0.5f)
                    {
                        if (Vector3.Distance(new Vector3(x, y, z), Vector3.one * radius) < radius)
                        {
                            blockMesh.transform.position = new Vector3(x, y, z);//move the unit cube to the intended position
                            CombineInstance ci = new CombineInstance {//copy the data off of the unit cube
                                mesh = blockMesh.sharedMesh,
                                transform = blockMesh.transform.localToWorldMatrix,
                            }; 
                            blockData.Add(ci);
                        }
                    }
                    
                }
            }
        }

        Destroy(blockMesh);
        Destroy(blockPrefab);
        combineMesh(blockData);
        Destroy(GameObject.Find("Cube(Clone)"));
    }

    // Update is called once per frame
    void Update()
    {
            if (Input.GetKeyDown(KeyCode.D)) {
                Debug.Log("Deleting cubes");

                Destroy(GameObject.Find("Meshys"));//destroy parent gameobject as well as children.
                foreach (Mesh m in meshes)//meshes still exist even though they aren't in the scene anymore. destroy them so they don't take up memory.
                    Destroy(m);
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

