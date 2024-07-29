using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Isometric;
using UnityEngine.SceneManagement;

public class ControllerScript : MonoBehaviour
{
    [SerializeField]
    private float speed = 10.0f;
    [SerializeField]
    private float zoomSpeed = 10.0f;
    [SerializeField]
    private Vector2 zoomLimit = new Vector2(0.5f, 10.0f);
    [SerializeField]
    private Transform worldParent;
    [SerializeField]
    private string backScene;

    private IsometricPerspective perspective;
    private List<Isometric.Grid[][]> world;
    private Biome biome;

    private void Awake()
    {
        BiomeController.GetInstance().GenerateWorld(this);
    }

    private void Update()
    {
        if(Application.platform != RuntimePlatform.Android) UpdateInput();
    }

    public void AssignWorld(List<Isometric.Grid[][]> world, Biome biome)
    {
        this.world = world;
        this.biome = biome;

        if(perspective == null) perspective = new IsometricPerspective();

        perspective.ArrangeBiomeInIsometric(biome, world);

        foreach(Isometric.Grid[][] floor in world)
        {
            foreach(Isometric.Grid[] row in floor)
            {
                foreach(Isometric.Grid obj in row) 
                {
                    if(obj == null) continue;
                    obj.GetGameObject().transform.SetParent(worldParent);
                }
            }
        }
    }

    private void UpdateInput()
    {
        if(Input.GetKeyDown(KeyCode.A)) MoveLeft();
        else if(Input.GetKeyDown(KeyCode.D)) MoveRight();
        else if(Input.GetKeyDown(KeyCode.W)) MoveUp();
        else if(Input.GetKeyDown(KeyCode.S)) MoveDown();
        else if(Input.GetKeyDown(KeyCode.Z)) ZoomOut();
        else if(Input.GetKeyDown(KeyCode.X)) ZoomIn();
    }

    public void NextPerspective()
    {
        perspective.SetNextPerspective();
        perspective.ArrangeBiomeInIsometric(biome, world);
    }

    public void PrevioiusPerspectice()
    {
        perspective.SetPreviousPerspective();
        perspective.ArrangeBiomeInIsometric(biome, world);
    }

    public void MoveUp()
    {
        Camera.main.gameObject.transform.position += new Vector3(0, speed * Time.deltaTime, 0);
    }

    public void MoveDown()
    {
        Camera.main.gameObject.transform.position -= new Vector3(0, speed * Time.deltaTime, 0);
    }

    public void MoveRight()
    {
        Camera.main.gameObject.transform.position += new Vector3(speed * Time.deltaTime, 0, 0);
    }

    public void MoveLeft()
    {
        Camera.main.gameObject.transform.position -= new Vector3(speed * Time.deltaTime, 0, 0);
    }

    public void ZoomIn()
    {
        float size = Camera.main.orthographicSize - zoomSpeed * Time.deltaTime;
        Camera.main.orthographicSize = (size < zoomLimit.x)? zoomLimit.x : size;
    }

    public void ZoomOut()
    {
        float size = Camera.main.orthographicSize + zoomSpeed * Time.deltaTime;
        Camera.main.orthographicSize = (size > zoomLimit.y)? zoomLimit.y : size;
    }

    public void Back()
    {
        SceneManager.LoadScene(backScene);
    }
}
