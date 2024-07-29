using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric;
using TMPro;
using UnityEngine.SceneManagement;

public class BiomeController : MonoBehaviour
{
    [SerializeField]
    private Vector3Int gridLimit;
    [SerializeField]
    private Biome[] selectableBiomes;
    [SerializeField]
    private string nextScene;
    [SerializeField]
    private TextMeshProUGUI rowText;
    [SerializeField]
    private TextMeshProUGUI columnText;
    [SerializeField]
    private TextMeshProUGUI heightText;
    [SerializeField]
    private TextMeshProUGUI biomeText;
    [SerializeField]
    private GameObject loadingScreen;

    private int index = 0;
    private Vector3Int grid = new Vector3Int(10, 10, 1);
    private static BiomeController instance = null; 

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;
    }

    public static BiomeController GetInstance()
    {
        return instance;
    }

    private void Update()
    {
        rowText.text = grid.x.ToString();
        columnText.text = grid.y.ToString();
        heightText.text = grid.z.ToString();
        biomeText.text = selectableBiomes[index].name;
    }

    public void LoadScene()
    {
        loadingScreen.SetActive(true);
        SceneManager.LoadScene(nextScene);
    }

    public void GenerateWorld(ControllerScript script)
    {
        script.AssignWorld(
        BiomeGenerator.GenerateBiome(selectableBiomes[index], grid),
        selectableBiomes[index]);
        Destroy(gameObject);
    }

    public void UpdateBiome(int i)
    {
        int newIndex = index + i;

        if(newIndex < 0) index = selectableBiomes.Length - 1;
        else if(newIndex >= selectableBiomes.Length) index = 0;
        else index = newIndex;
    }

    public void UpdateGridRow(int x)
    {
        int rowSize = grid.x + x;
        
        if(rowSize < 1) grid.x = 1;
        else if(rowSize > gridLimit.x) grid.x = gridLimit.x;
        else grid.x = rowSize;
    }

    public void UpdateGridColumn(int y)
    {
        int columnSize = grid.y + y;
        
        if(columnSize < 1) grid.y = 1;
        else if(columnSize > gridLimit.y) grid.y = gridLimit.y;
        else grid.y = columnSize;
    }

    public void UpdateGridHeight(int z)
    {
        int heightSize = grid.z + z;
        
        if(heightSize < 1) grid.z = 1;
        else if(heightSize > gridLimit.z) grid.z = gridLimit.z;
        else grid.z = heightSize;
    }
}
