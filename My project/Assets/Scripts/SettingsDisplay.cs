using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsDisplay : MonoBehaviour
{
    GenerateChunkTerrain generateChunkTerrain;
    [SerializeField]
    GameObject settingsPanel;
    public Text settingsText;

    // Start is called before the first frame update
    void Awake()
    {
        generateChunkTerrain = settingsPanel.GetComponent<GenerateChunkTerrain>();
    }

    // Update is called once per frame
    void Update()
    {
        settingsText.text = "Chunk Amount: " + generateChunkTerrain.planetChunksNum + System.Environment.NewLine +
            "Chunk Size: " + generateChunkTerrain.chunkSize + System.Environment.NewLine +
            "Noise Scale: " + generateChunkTerrain.noiseScale + System.Environment.NewLine +
            "Noise Frequency: " + generateChunkTerrain.noiseFrequency + System.Environment.NewLine +
            "Brush Size: " + generateChunkTerrain.brushSize + System.Environment.NewLine +
            "Brush Speed: " + generateChunkTerrain.brushSpeed;
    }
}
