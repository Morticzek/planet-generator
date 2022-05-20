using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random=UnityEngine.Random;


/*public struct Triangle {
    public Vector3 A;
    public Vector3 B;
    public Vector3 C;
};*/

public class Chunk
{
    //chunk Id
    private int ID;

    // Size of chunk
    private int size;

    // Center of a chunk
    private Vector3 center;

    //beggining of a chunk
    public Vector3Int chunkStartCoord;

    // Color of mesh inside the chunk
    private Color color;

    // three dimensional array containing noiseAtChunk inside the chunk
    public float [,,] noiseAtChunk;

    // mesh points that are inside the chunk
    public Vector3 [,,] points;

    // triangles generated from marching cubes algo
    private List<Vector3> triangles;

    //look up table for marching cubes algo
    private marchLookUp lookUpTable = new marchLookUp();

    //interpolation flag
    private bool interpolate;

    private float treshhold = 0.5f;

    public Chunk(int id, int size, Vector3 center, Vector3Int chunkStart, Color col, bool interpol)
    {
        this.ID = id;
        this.size = size;
        this.center = center;
        this.chunkStartCoord = chunkStart;
        this.color = col;
        this.interpolate = interpol;

        this.noiseAtChunk = new float [size, size, size];
        this.points = new Vector3 [size, size, size];

        //@todo -- get some algo for hashing the chunks, how to get the proper chunk and mesh from 3d coords
        // add marching cubes and mesh generation for every chunk
        // add chunk generation
        // add terrain generation across different chunks

    }

    private Vector3 interpolateCoords(int cornerA, int cornerB, float[] cubeNoise, Vector3[] cubeCoords)
    {
        float valueA = cubeNoise[cornerA];
        float valueB = cubeNoise[cornerB];

        float t = (treshhold - valueA) / (valueB - valueA);

        Vector3 resultVertex = new Vector3();

        resultVertex = cubeCoords[cornerA] + t *(cubeCoords[cornerB] - cubeCoords[cornerA]);


        // Debug.Log(valueA + " " + valueB + " " + t);

        // Debug.Log("Interpolated point: " + resultVertex.x + " " + resultVertex.y + " " + resultVertex.z);

        return resultVertex;
    }

    public void marchCubes(float[,,] globalNoise, Vector3[,,] globalPoints)
    {
        for(int z = 0; z < size; z++)
        {
            for(int y = 0; y < size; y++)
            {
                for(int x = 0; x < size; x++)
                {
                    //find and store all vertices from generated ones in current cube or generate here
                    float[] currentCubeNoise = new float[8];
                    Vector3[] currentCubeVertices = new Vector3[8];

                    int absX = this.chunkStartCoord.x + x;
                    int absY = this.chunkStartCoord.y + y;
                    int absZ = this.chunkStartCoord.z + z;

                    currentCubeNoise[0] = globalNoise[absX, absY, absZ];
                    currentCubeNoise[1] = globalNoise[absX + 1, absY, absZ];
                    currentCubeNoise[2] = globalNoise[absX + 1, absY, absZ+1];
                    currentCubeNoise[3] = globalNoise[absX, absY, absZ + 1];
                    currentCubeNoise[4] = globalNoise[absX, absY + 1, absZ];
                    currentCubeNoise[5] = globalNoise[absX+1, absY + 1, absZ];
                    currentCubeNoise[6] = globalNoise[absX+1, absY+1, absZ+1];
                    currentCubeNoise[7] = globalNoise[absX, absY+1, absZ+1];

                    currentCubeVertices[0] = globalPoints[absX, absY, absZ];
                    currentCubeVertices[1] = globalPoints[absX + 1, absY, absZ];
                    currentCubeVertices[2] = globalPoints[absX + 1, absY, absZ + 1];
                    currentCubeVertices[3] = globalPoints[absX, absY, absZ + 1];
                    currentCubeVertices[4] = globalPoints[absX, absY + 1, absZ];
                    currentCubeVertices[5] = globalPoints[absX + 1, absY + 1, absZ];
                    currentCubeVertices[6] = globalPoints[absX + 1, absY + 1, absZ + 1];
                    currentCubeVertices[7] = globalPoints[absX, absY + 1, absZ + 1];

                    int cubeIndex = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if(currentCubeNoise[i] > treshhold)
                            cubeIndex += 1 << i;
                    }
                    

                    // Create triangles for current cube configuration
                    for (int i = 0; lookUpTable.triangulation[cubeIndex, i] != -1; i +=3) {
                        // Get indices of corner points A and B for each of the three edges
                        // of the cube that need to be joined to form the triangle.
                        int a0 = lookUpTable.cornerIndexAFromEdge[lookUpTable.triangulation[cubeIndex,i]];
                        int b0 = lookUpTable.cornerIndexBFromEdge[lookUpTable.triangulation[cubeIndex,i]];

                        int a1 = lookUpTable.cornerIndexAFromEdge[lookUpTable.triangulation[cubeIndex,i+1]];
                        int b1 = lookUpTable.cornerIndexBFromEdge[lookUpTable.triangulation[cubeIndex,i+1]];

                        int a2 = lookUpTable.cornerIndexAFromEdge[lookUpTable.triangulation[cubeIndex,i+2]];
                        int b2 = lookUpTable.cornerIndexBFromEdge[lookUpTable.triangulation[cubeIndex,i+2]];

                        Vector3 pointA;
                        Vector3 pointB;
                        Vector3 pointC;

                        if(interpolate)
                        {
                            pointA = interpolateCoords(a0, b0, currentCubeNoise, currentCubeVertices);
                            pointB = interpolateCoords(a1, b1, currentCubeNoise, currentCubeVertices);
                            pointC = interpolateCoords(a2, b2, currentCubeNoise, currentCubeVertices);
                        
                        }
                        else
                        {
                            pointA = new Vector3();
                            pointB = new Vector3();
                            pointC = new Vector3();

                            pointA = (currentCubeVertices[a0] + currentCubeVertices[b0]) * 0.5f;
                            pointB = (currentCubeVertices[a1] + currentCubeVertices[b1]) * 0.5f;
                            pointC = (currentCubeVertices[a2] + currentCubeVertices[b2]) * 0.5f;

                        }

                        triangles.Add(pointA);
                        triangles.Add(pointB);
                        triangles.Add(pointC);

                        //triangle inside the mesh, sometimes might be usefull
                        // triangles.Add(pointC);
                        // triangles.Add(pointB);
                        // triangles.Add(pointA);
                    }
                                  
                }
            }
        }
        

    }

    public void constructMesh(List<GameObject> meshes, Transform container, float[,,] globalNoise, Vector3[,,] globalPoints)
    {
        triangles = new List<Vector3>();
        marchCubes(globalNoise, globalPoints);
        // Debug.Log("Generated verts: " + verts.Count);

        int maxVertsPerMesh = 30000; //must be divisible by 3, ie 3 verts == 1 triangle
        int numMeshes = triangles.Count / maxVertsPerMesh + 1;
        
        // Transform container = new GameObject("Meshys").transform;

        
        for (int i = 0; i < numMeshes; i++)
            {
                List<Vector3> splitVerts = new List<Vector3>();
                List<int> splitIndices = new List<int>();
              

                for (int j = 0; j < maxVertsPerMesh; j++)
                {
                    int idx = i * maxVertsPerMesh + j;

                    if (idx < triangles.Count)
                    {
                        splitVerts.Add(triangles[idx]);
                        splitIndices.Add(j);
                    }
                }

                if (splitVerts.Count == 0) continue;

                Color[] colors = new Color[splitVerts.Count];
                for(int ic = 0; ic < splitVerts.Count; ic++)
                    colors[ic] = color;


                Mesh mesh = new Mesh();
                mesh.SetVertices(splitVerts);
                mesh.SetTriangles(splitIndices, 0);
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();

                
                GameObject go = new GameObject("Mesh");
                go.transform.parent = container;
                go.AddComponent<MeshFilter>();
                go.AddComponent<MeshRenderer>();
                go.GetComponent<Renderer>().material = new Material(Shader.Find("Particles/Standard Surface"));
                // go.GetComponent<Renderer>().material.color = color;
                go.GetComponent<MeshFilter>().mesh = mesh;
                go.GetComponent<MeshFilter>().mesh.colors = colors;
                go.AddComponent<MeshCollider>();

                meshes.Add(go);
            } 
    }



}


public class GenerateChunkTerrain : MonoBehaviour
{
    [SerializeField]
    private Color color;

    private List<Chunk> chunks;

    [SerializeField]
    private bool interpolate = false;

    public float[,,] globalNoise;

    public Vector3[,,] globalPoints;

    // Meshe contained within this chunk
    private List<GameObject> meshes= new List<GameObject>();


    private void generateChunkedPlanet()
    {

        Transform container = new GameObject("Meshys").transform;

        

        //initalize the chunks
        int planetChunksNum = 8;
        int chunkSize = 16;

        int arraysSize = planetChunksNum * chunkSize + 1;

        globalNoise = new float[arraysSize, arraysSize, arraysSize];

        globalPoints = new Vector3[arraysSize, arraysSize, arraysSize];

        int radius = chunkSize * (planetChunksNum / 2) - 1;

        chunks = new List<Chunk>();

        int chunkId = 0;
        for(int x = 0; x < planetChunksNum; x++)
        {
            for(int y = 0; y < planetChunksNum; y++)
            {
                for(int z = 0; z < planetChunksNum; z++)
                {
                    // compute chunk center
                    Vector3Int coord = new Vector3Int(x, y, z);
					float posX = (-(planetChunksNum - 1f) / 2 + x) * chunkSize;
					float posY = (-(planetChunksNum - 1f) / 2 + y) * chunkSize;
					float posZ = (-(planetChunksNum - 1f) / 2 + z) * chunkSize;
					Vector3 centre = new Vector3(posX, posY, posZ);

                    Vector3Int chunkStartCoords = new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize);

                    Chunk chunk = new Chunk(chunkId, chunkSize, centre, chunkStartCoords, color, interpolate);
                    
                    chunks.Add(chunk);
                }

            }
        }

        //populate chunks with noiseAtChunk
        foreach (Chunk h in chunks)
        {
            for(int x = 0; x < chunkSize; x++)
            {
                for(int y = 0; y < chunkSize; y++)
                {
                    for(int z = 0; z < chunkSize; z++)
                    {
                        int absX = h.chunkStartCoord.x + x;
                        int absY = h.chunkStartCoord.y + y;
                        int absZ = h.chunkStartCoord.z + z;
                        globalPoints[absX, absY, absZ] = new Vector3(absX, absY, absZ);
                        globalNoise[absX, absY, absZ] = (radius - 1) - Vector3.Distance(new Vector3(absX, absY, absZ), Vector3.one * (radius-1));
                    }
                }
            }
        }

        foreach(Chunk h in chunks)
        {
            // h.marchCubes();
            h.constructMesh(meshes, container, globalNoise, globalPoints);
        }
    }

    void Update()
    {
            if (Input.GetKeyDown(KeyCode.U)) {
                Debug.Log("Deleting cubes");

                Destroy(GameObject.Find("Meshys"));

                // foreach (GameObject m in allMeshes)//meshes still exist even though they aren't in the scene anymore. destroy them so they don't take up memory.
                    // Destroy(m);
                }

            if(Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("Generating cubes");
                generateChunkedPlanet();
                // PopulateNoiseMap();
                // Generate();
            }
    }


}

