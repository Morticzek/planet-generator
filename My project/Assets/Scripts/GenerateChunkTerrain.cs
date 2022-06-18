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
    private Color primaryColor;

    private Color secondaryColor;

    // three dimensional array containing noiseAtChunk inside the chunk
    public float [,,] noiseAtChunk;

    // mesh points that are inside the chunk
    public Vector3 [,,] points;

    // triangles generated from marching cubes algo
    private List<Vector3> triangles;

    // list of angles between triangles and planet center
    private List<float>  angles;

    //look up table for marching cubes algo
    private marchLookUp lookUpTable = new marchLookUp();

    //interpolation flag
    private bool interpolate;

    private float treshhold = 0.4f;

    // Meshes contained within this chunk
    public List<GameObject> meshes;

    public Chunk(int id, int size, Vector3 center, Vector3Int chunkStart, Color primaryCol, Color secondaryColor, bool interpol)
    {
        this.ID = id;
        this.size = size;
        this.center = center;
        this.chunkStartCoord = chunkStart;
        this.primaryColor = primaryCol;
        this.secondaryColor = secondaryColor;
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

        return resultVertex;
    }

    public void marchCubes(ref float[,,] globalNoise, ref Vector3[,,] globalPoints, Vector3 planetCenter)
    {
        for(int z = 0; z < size; z++)
        {
            for(int y = 0; y < size; y++)
            {
                for(int x = 0; x < size; x++)
                {

                    // get the absoulte coordinates for global noise and points access
                    int absX = this.chunkStartCoord.x + x;
                    int absY = this.chunkStartCoord.y + y;
                    int absZ = this.chunkStartCoord.z + z;

                    //check if the points are contained inside generated chunks
                    if(absY+1 > globalPoints.Length )
                    {
                        Debug.Log("Index out of global points scope");
                    }



                    //find and store all vertices from generated ones in current cube or generate here
                    float[] currentCubeNoise = new float[8];
                    Vector3[] currentCubeVertices = new Vector3[8];



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

                        /*if(pointA.x >= globalPoints.Length - 1 || pointA.y >= globalPoints.Length - 1 || pointA.z >= globalPoints.Length -1)
                            Debug.Log("Point A out of scope");

                        if(pointB.x >= globalPoints.Length -1|| pointB.y >= globalPoints.Length-1 || pointB.z >= globalPoints.Length -1)
                            Debug.Log("Point B out of scope");

                        if(pointC.x >= globalPoints.Length -1|| pointC.y >= globalPoints.Length-1 || pointC.z >= globalPoints.Length -1)
                            Debug.Log("Point C out of scope");*/

                        triangles.Add(pointA);
                        triangles.Add(pointB);
                        triangles.Add(pointC);

                        //calculate normal vector to this new triangle
                        Vector3 side1 = pointB - pointA;
                        Vector3 side2 = pointC - pointA;
                        Vector3 norm = Vector3.Cross(side1, side2);
                        Vector3 direction = new Vector3(pointA.x - planetCenter.x, pointA.y - planetCenter.y, pointA.z - planetCenter.z);
                        float angle = Vector3.Angle(direction, norm);
                        // Debug.Log(angle);

                        angles.Add(angle);

                    }
                                  
                }
            }
        }
        

    }

    public void constructMesh(Transform container, ref float[,,] globalNoise, ref Vector3[,,] globalPoints, Vector3 planetCenter)
    {

        this.meshes = new List<GameObject>();
        triangles = new List<Vector3>();
        angles = new List<float>();

        marchCubes(ref globalNoise, ref globalPoints, planetCenter);
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
                {
                    if(angles[ic/3] > 20.0f)
                        colors[ic] = primaryColor;
                    else if (angles[ic/3] > 16.0f)
                        colors[ic] = new Color((primaryColor.r + secondaryColor.r)/2, (primaryColor.g + secondaryColor.g)/2, (primaryColor.b + secondaryColor.b)/2);
                    else 
                        colors[ic] = secondaryColor;
                }


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
    private Color primaryColor;

    [SerializeField]
    private Color secondaryColor;

    private List<Chunk> chunks;

    [SerializeField]
    private bool interpolate = false;

    public int planetChunksNum = 8;

    public int chunkSize = 16;

    public float noiseScale = 5.0f;

    public float noiseFrequency = 0.05f;

    public float caveNoiseFrequency = 0.05f;

    public int brushSize = 3;

    public float brushSpeed = 0.7f;

    public float[,,] globalNoise;

    public Vector3[,,] globalPoints;

    private float noiseXOffset;
    private float noiseYOffset;
    private float noiseZOffset; 

    Transform container;

    Vector3 planetCenter;

    private void generateChunkedPlanet()
    {

        noiseXOffset = Random.Range(0f, 999999f);
        noiseYOffset = Random.Range(0f, 999999f);
        noiseZOffset = Random.Range(0f, 999999f);

        container = new GameObject("Meshys").transform;


        int arraysSize = this.planetChunksNum * this.chunkSize + 1;

        globalNoise = new float[arraysSize, arraysSize, arraysSize];

        globalPoints = new Vector3[arraysSize, arraysSize, arraysSize];

        int radius = chunkSize * (planetChunksNum / 2) - 1;

        planetCenter = new Vector3(radius, radius, radius);

        Debug.Log("Planet center: " + planetCenter);

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

                    Chunk chunk = new Chunk(chunkId, chunkSize, centre, chunkStartCoords, primaryColor, secondaryColor, interpolate);
                    
                    chunks.Add(chunk);
                    chunkId++;
                }

            }
        }


        //populate chunks with noiseAtChunk
        foreach (Chunk h in chunks)
        {
            for(int x = 0; x <= chunkSize; x++)
            {
                for(int y = 0; y <= chunkSize; y++)
                {
                    for(int z = 0; z <= chunkSize; z++)
                    {
                        int absX = h.chunkStartCoord.x + x;
                        int absY = h.chunkStartCoord.y + y;
                        int absZ = h.chunkStartCoord.z + z;
                        globalPoints[absX, absY, absZ] = new Vector3(absX, absY, absZ);

                        //this one makes perfect sphere
                        // globalNoise[absX, absY, absZ] = (radius - 1) - Vector3.Distance(new Vector3(absX, absY, absZ), Vector3.one * (radius-1));

                        globalNoise[absX, absY, absZ] = (radius) - Vector3.Distance(new Vector3(absX, absY, absZ), Vector3.one * (radius)) - Perlin3D(absX*noiseFrequency, absY*noiseFrequency, absZ*noiseFrequency) * noiseScale;
                        
                        // float caveNoise = Perlin3D(absX*noiseFrequency, absY*noiseFrequency, absZ*noiseFrequency);
                        // globalNoise[absX, absY, absZ] =+ globalNoise[absX, absY, absZ] - caveNoise * 50;

                        if(globalNoise[absX, absY, absZ] > 0.1f)
                        {
                            float caveNoise = Perlin3D(absX*caveNoiseFrequency,  absY*caveNoiseFrequency, absZ*caveNoiseFrequency);
                            if(caveNoise < 0.42f)
                                globalNoise[absX, absY, absZ] = caveNoise ;
                        }
 

                        //This line makes cool caves

                        // globalNoise[absX, absY, absZ] = Perlin3D(absX*noiseFrequency, absY*noiseFrequency, absZ*noiseFrequency);


                    }
                }
            }
        }

        foreach(Chunk h in chunks)
        {
            // h.marchCubes();
            h.constructMesh(container, ref globalNoise, ref globalPoints, planetCenter);
        }

        //Place assets on surface of cube
        for(int i = 0; i < 1; i++)
        {
            //get a random position on a sphere

            RaycastHit hit;

            Vector3 randomPos = Random.onUnitSphere * radius;
            randomPos += planetCenter;
            GameObject instance = Instantiate(Resources.Load("tree1", typeof(GameObject))) as GameObject;
            Destroy(instance.transform.GetChild(0).gameObject);
            instance.transform.position = randomPos;
            instance.transform.RotateAround(planetCenter, Vector3.up, 90);  //more sophisticated rotation is needed

            Vector3 toPlanetVector = planetCenter - instance.transform.position;
                                                            //here cast a new ray from instance to planet center
            if(Physics.Raycast(instance.transform.position, toPlanetVector, out hit, 100.0f))
                instance.transform.position = hit.point;
        }
    }

    private int coordToChunkId(int x, int y, int z)
    {
        Double xId = (x / this.chunkSize) * Math.Pow((Double)this.planetChunksNum, 2);
        Double yId = (y / this.chunkSize) * Math.Pow((Double)this.planetChunksNum, 1);
        Double zId = (z / this.chunkSize) * Math.Pow((Double)this.planetChunksNum, 0);

        int id = (int) (xId + yId + zId);

        return id;
    }

    private void rebuildChunk(int id)
    {

        foreach(GameObject mesh in this.chunks[(int)id].meshes)
            Destroy(mesh);

        this.chunks[(int)id].constructMesh(container, ref globalNoise, ref globalPoints, planetCenter);
    }


    public void modifyTerrain(Vector3 collisionPoint, bool creatingTerrain)
    {
        int x = (int) collisionPoint.x;
        int y = (int) collisionPoint.y;
        int z = (int) collisionPoint.z;


        int affectedChunkId;
        List<int> affectedChunks = new List<int>();

        for(int i = -1 - brushSize; i < brushSize+1; i++)
        {
            for(int j = -1 - brushSize; j < brushSize+1; j++)
            {
                for(int k = -1 - brushSize; k < brushSize+1; k++)
                {
                    if(x + i > 0 && y + j > 0 && z + k > 0)
                    {
                        if(x + i < this.planetChunksNum * this.chunkSize + 1 && y + j < this.planetChunksNum * this.chunkSize + 1 && z + k < this.planetChunksNum * this.chunkSize + 1)
                        {

                            // Debug.Log(Vector3.Distance(new Vector3(x + i, y + j, z + k), collisionPoint ));
                            if(creatingTerrain)
                            {
                                globalNoise[x + i, y + j, z + k] -=  brushSize - Vector3.Distance(new Vector3(x + i, y + j, z + k), collisionPoint) * brushSpeed * Time.deltaTime;
                            }
                            else
                            {
                                globalNoise[x + i, y + j, z + k] +=  brushSize - Vector3.Distance(new Vector3(x + i, y + j, z + k), collisionPoint)  *  brushSpeed * Time.deltaTime; 
                            }
                        }
                    }

                    affectedChunkId = coordToChunkId(x + i, y + j, z + k);
                    if(!affectedChunks.Contains(affectedChunkId))
                        affectedChunks.Add(affectedChunkId);

                }
            }
        }

        foreach(int id in affectedChunks)
        {
            if(id > 0 && id < this.chunks.Count)
                rebuildChunk(id);

        }
            

    }

    public float Perlin3D (float x, float y, float z)
    {
        //this produces new pattern everytime but is a bit wired tho
        x = x + noiseXOffset;
        y = y + noiseYOffset;
        z = z + noiseZOffset;
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
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

