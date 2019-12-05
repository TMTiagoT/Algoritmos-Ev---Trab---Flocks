using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlocksSpawner : MonoBehaviour
{
    //public
    public GameObject flocksLayerPrefab; //prefab de uma layer de flocks para ser spawnada
    public GameObject flockManagerPrefab; //prefab de um flockManager para ser spawnado 

    [Range(1, 100)]
    public int layersQuantity; //quantidade de layers para spawnar
    [Range(1, 10)]
    public int flocksQuantityPerLayer; //quantidade de flocks por layer para spawnar

    [Range(1, 100)]
    public float layersDistance; //distancia das layers pra spawnar

    //private

    private void Awake()
    {
        SpawnFlocks(); //spawnar/criar os flocks
    }

    // Use this for initialization
    void Start()
    {

    }

    public void SpawnFlocks() //spawnar/criar os flocks
    {
        for (int i = 0; i < layersQuantity; i++) //para cada layer (que vai ser criada)
        {
            /*GameObject newFlocksLayer = */
            Instantiate(flocksLayerPrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + (i * layersDistance) + 1), gameObject.transform.rotation); //criar uma nova layer de flocks

            for (int j = 0; j < flocksQuantityPerLayer; j++) //para cada flock (que vai ser criado)
            {
                GameObject newFlockManager = Instantiate(flockManagerPrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + (i * layersDistance)), gameObject.transform.rotation); //criar um novo flock manager
                FlockManager flockManager = newFlockManager.GetComponent<FlockManager>(); //pegar script flockManager

                flockManager.fullNeighborColor = Random.ColorHSV(0, 1, 0, 1, 0, 1); //gerar uma cor aleatoria para o flock
                flockManager.fullNeighborColor.a = 1; //setar o alpha como 1

                if (flockManager.fullNeighborColor.r > flockManager.fullNeighborColor.g && flockManager.fullNeighborColor.r > flockManager.fullNeighborColor.b) //se a cor red for maior
                {
                    flockManager.noNeighborColor = new Color(flockManager.fullNeighborColor.r, 0.85f * flockManager.fullNeighborColor.r, 0.85f * flockManager.fullNeighborColor.r, flockManager.fullNeighborColor.a); //usar cor red como paramentro para variar as outras
                }
                else if (flockManager.fullNeighborColor.g > flockManager.fullNeighborColor.r && flockManager.fullNeighborColor.g > flockManager.fullNeighborColor.b) //se a cor green for maior
                {
                    flockManager.noNeighborColor = new Color(0.85f * flockManager.fullNeighborColor.g, flockManager.fullNeighborColor.g, 0.85f * flockManager.fullNeighborColor.g, flockManager.fullNeighborColor.a); //usar cor green como paramentro para variar as outras
                }
                else //caso contrario
                {
                    flockManager.noNeighborColor = new Color(0.85f * flockManager.fullNeighborColor.b, 0.85f * flockManager.fullNeighborColor.b, flockManager.fullNeighborColor.b, flockManager.fullNeighborColor.a); //usar cor blue como paramentro para variar as outras
                }
            }
        }
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
