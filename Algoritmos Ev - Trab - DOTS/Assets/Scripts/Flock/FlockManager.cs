using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Physics;

public class FlockManager : MonoBehaviour
{ // "..._ECS" -> (com ECS/DOTS/JobSystem, etc.)
    //public
    public GameObject flockAgentPref; //prefab do flockAgent
    //public int flockManagerNumber = 1; //qual eh o numero do flockManager

    //public FlockAgent flockAgentPrefab; //script flockAgent
    //public FlockBehavior flockBehavior; //script flockBehavior

    [Range(1, 10000)]
    public int startingCount; //quantidade inicial de agentes na cena //default 250

    [Range(1f, 100f)]
    public float driveFactor; //fator de multiplicacao na velocidade de reajuste dos agentes //default 10
    [Range(1f, 100f)]
    public float maxSpeed; //velocidade maxima de um agente //default 5

    [Range(1f, 10f)]
    public float neighborRadius; //raio ao redor do agente que ira considerar objetos //default 1.5
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier; //distancia minima de um agente do outro (para separar --> com relacao ao neighborRadius) //default 0.5

    [Range(0f, 1f)]
    public float agentDensity; //densidade de agentes proximos na hora de instanciar (define, junto com a quantidade de agentes a ser gerada, o raio de "spawn" dos agentes) //default 0.08

    //[Range(1, 10)]
    //public int initialHealth; //vida inicial dos flocks //default 3

    //[Space(5)]
    //public Color noNeighborColor; //cor de um agente quando nao tem objetos perto //default branco
    //public Color fullNeighborColor; //cor de um agente quando tem muitos/todos objetos perto //default vermelho
    //[Range(1f, 50f)]
    //public int fullNeighborColorCount; //quantidade de objetos proximos ao agente para setar a cor "fullNeighborColor" completa //default 6

    [Space(5)]
    public bool showGizmosOnInspector; //se eh para mostrar ou nao o gizmos no inspector //(para testes)

    [HideInInspector]
    public float squareAvoidanceRadius; //variavel auxiliar

    //private
    [HideInInspector]
    public NativeArray<Entity> flockAgents_ECS; //lista de flocks
    [HideInInspector]
    public NativeArray<Translation> flockAgents_ECS_T; //lista de flocks (translation)
    [HideInInspector]
    public NativeArray<Rotation> flockAgents_ECS_R; //lista de flocks (rotation)
    [HideInInspector]
    public NativeArray<FlockWho> flockAgents_ECS_FW; //lista de flocks (flock who)

    [HideInInspector]
    public NativeArray<Entity> objectAgents_ECS; //lista de flocks
    [HideInInspector]
    public NativeArray<Translation> objectAgents_ECS_T; //lista de flocks (translation)
    //private List<FlockAgent_ECS> flockAgents; //lista de scripts flockAgent

    [HideInInspector]
    public float squareMaxSpeed; //variavel auxiliar
    private float squareNeighborRadius; //variavel auxiliar

    [HideInInspector]
    public EntityManager entityManager; //entityManager
    private Entity flockAgentEntity; //entidade para o flockAgent 

    //Dados dos Behavious
    [Header("Dados dos Behaviors")]

    //composite behavior
    [ReadOnly]
    [Tooltip("Nomes apenas para informacao da ordem na unity")]
    public string[] flockBehaviors = new string[] { "SteeredCohesion", "Alignment", "StayInRadius", "Avoidance", "AvoidanceRadius" }; //comportamentos do objeto
    public float[] behaviorsWeights; //pesos dos comportamentos nas acoes do objeto
    [HideInInspector]
    public NativeArray<float> behaviorsWeightsNArray; //pesos dos comportamentos nas acoes do objeto

    [HideInInspector]
    public NativeArray<float> neighborRadiusNArray; //neighborRadius dos objetos
    [HideInInspector]
    public NativeArray<float> squareNeighborRadiusNArray; //squareNeighborRadius dos objetos
    [HideInInspector]
    public NativeArray<float> avoidanceRadiusMultiplierNArray; //avoidanceRadiusMultiplier dos objetos
    [HideInInspector]
    public NativeArray<float> squareAvoidanceRadiusNArray; //squareAvoidanceRadius dos objetos

    //steered cohesion
    public float agentSmoothTime; //tempo para "suavizar" o movimento //default 0.5

    //stay in radius
    public float3 radiusCenter; //centro do raio //default (0, 0)

    public float radius; //raio //default 5f
    public float radiusLimiterPer; //porcentagem de limite para sair/chegar perto do raio maximo para comecar a afetar os objetos //default 0.9f

    //-----------------
    [Range(1, 100)]
    [Header("Dados da geracao")]
    public GameObject flocksLayerPrefab; //prefab de uma layer de flocks para ser spawnada

    [Range(1, 10)]
    public int layersQuantity; //quantidade de layers para spawnar
    [Range(1, 100)]
    public int flocksQuantityPerLayer; //quantidade de flocks por layer para spawnar

    [Range(1f, 100f)]
    public float layersDistance; //distancia das layers pra spawnar

    [Header("Dados da geracao de objetos")]
    [Range(0, 100)]
    public int qtdOfLines;// quantidade de linhas/objetos pra gerar //default 10
    [Range(0, 100)]
    public int lineSize; //quantidade de linhas/objetos pra gerar (comprimento) //default 15
    [Range(0, 100)]
    public int lineHeight; //quantidade de linhas/objetos pra gerar (altura) //default 1

    [HideInInspector]
    public int qtdObjects;//quantidade total de objetos nos objetos

    private int qtdFlocksInFlock;
    private int qtdFlocks;

    // Use this for initialization
    void Awake()
    {
        squareMaxSpeed = maxSpeed * maxSpeed; //inicializar valores
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
        qtdObjects = qtdOfLines * lineSize * lineHeight;
        qtdFlocks = layersQuantity * flocksQuantityPerLayer;
        qtdFlocksInFlock = qtdFlocks * startingCount;

        entityManager = World.Active.EntityManager; //incializar valores
        //entityManager.CreateEntity();
        flockAgentEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(flockAgentPref, World.Active);

        //flockAgents = new List<FlockAgent_ECS>(); //inicializar valores
        flockAgents_ECS = new NativeArray<Entity>(qtdFlocksInFlock, Allocator.Persistent);
        flockAgents_ECS_T = new NativeArray<Translation>(qtdFlocksInFlock, Allocator.Persistent);
        flockAgents_ECS_R = new NativeArray<Rotation>(qtdFlocksInFlock, Allocator.Persistent);
        flockAgents_ECS_FW = new NativeArray<FlockWho>(qtdFlocksInFlock, Allocator.Persistent);

        objectAgents_ECS = new NativeArray<Entity>(layersQuantity * qtdObjects, Allocator.Persistent);
        objectAgents_ECS_T = new NativeArray<Translation>(layersQuantity * qtdObjects, Allocator.Persistent);

        behaviorsWeightsNArray = new NativeArray<float>(behaviorsWeights.Length * qtdFlocks, Allocator.Persistent);

        for (int i = 0; i < behaviorsWeights.Length * qtdFlocks; i++) //incializar pesos
        {
            behaviorsWeightsNArray[i] = behaviorsWeights[i % behaviorsWeights.Length];
        }

        neighborRadiusNArray = new NativeArray<float>(qtdFlocks, Allocator.Persistent);
        squareNeighborRadiusNArray = new NativeArray<float>(qtdFlocks, Allocator.Persistent);
        squareAvoidanceRadiusNArray = new NativeArray<float>(qtdFlocks, Allocator.Persistent);
        avoidanceRadiusMultiplierNArray = new NativeArray<float>(qtdFlocks, Allocator.Persistent);

        for (int i = 0; i < qtdFlocks; i++) //incializar pesos
        {
            neighborRadiusNArray[i] = neighborRadius;
            squareNeighborRadiusNArray[i] = squareNeighborRadius;
            squareAvoidanceRadiusNArray[i] = squareAvoidanceRadius;
            avoidanceRadiusMultiplierNArray[i] = avoidanceRadiusMultiplier;
        }

        //InitializeFlock_ECS(); //inicializar cena/flocks
        SpawnFlocks(); //inicializar cena/flocks

        entityManager.DestroyEntity(flockAgentEntity);
    }

    public void InitializeFlock_ECS(int flockManagerN, float zPos, int flockLayer) //inicializar cena/flocks
    {
        for (int i = 0; i < startingCount; i++) //para cada flock
        {
            int index = i + (flockManagerN) * startingCount;

            Vector2 flockPosition = UnityEngine.Random.insideUnitCircle * startingCount * agentDensity; //posicao "aleatoria" dentro do range especificado para o novo flock
            Quaternion flockRotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)); //rotacao "aleatoria" para o novo flock (de 0 a 360)

            Translation newT = new Translation { Value = new float3(flockPosition.x, flockPosition.y, gameObject.transform.position.z + zPos) };
            entityManager.SetComponentData(flockAgents_ECS[index], newT); //setar a posicao
            Rotation newR = new Rotation { Value = flockRotation };
            entityManager.SetComponentData(flockAgents_ECS[index], newR); //setar a rotacao

            FlockWho newFW = new FlockWho { flockValue = i, flockManagerValue = flockManagerN, flockLayerValue = flockLayer, flockCollisionCount = 0, objectCollisionCount = 0 };
            entityManager.AddComponentData(flockAgents_ECS[index], newFW); //setar a numeracao do flock e do respectivo manager

            //newFlockAgent.flockManager = this; //ajustar quem criou o flock
            flockAgents_ECS_T[index] = newT; //setar arrays auxiliares
            flockAgents_ECS_R[index] = newR;
            flockAgents_ECS_FW[index] = newFW;
        }
    }

    public void UpdateData() //atualiza os dados necessarios das entidades
    {
        for (int i = 0; i < qtdFlocksInFlock; i++) //para cada flock
        {
            flockAgents_ECS_T[i] = entityManager.GetComponentData<Translation>(flockAgents_ECS[i]); //pegar a posicao
            flockAgents_ECS_R[i] = entityManager.GetComponentData<Rotation>(flockAgents_ECS[i]); //pegar a rotacao
            flockAgents_ECS_FW[i] = entityManager.GetComponentData<FlockWho>(flockAgents_ECS[i]); //pegar as informacoes (pode atualizar a vida, etc)
        }
    }

    public bool restartTest = false;
    //Update is called once per frame
    void Update()
    {
        if (restartTest) RestartFlocks();
        //for (int i = 0; i < qtdFlocks; i++) //para cada flock (para testes)
        //{
        //    Debug.Log(flockAgents_ECS_FW[i].health);
        //}
    }

    public void SpawnFlocks() //spawnar/criar os flocks
    {
        entityManager.Instantiate(flockAgentEntity, flockAgents_ECS); //criar entidades dos flocks

        for (int i = 0; i < layersQuantity; i++) //para cada layer (que vai ser criada)
        {
            Instantiate(flocksLayerPrefab, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z + (i * layersDistance) + 1), gameObject.transform.rotation); //criar uma nova layer de flocks

            for (int j = 0; j < flocksQuantityPerLayer; j++) //para cada flock (que vai ser criado)
            {
                InitializeFlock_ECS(j + flocksQuantityPerLayer * i, (i * layersDistance) + 1, i); //k + (j + layersQuantity * i) * flocksQuantityPerLayer
            }

            SpawnObjects(i, (i * layersDistance) + 1);
        }
    }

    public void SpawnObjects(int flockLayer, float zPos) //instanciar alguns objetos feitos de flocks na cena
    {
        entityManager.Instantiate(flockAgentEntity, objectAgents_ECS); //criar entidades dos flocks

        float dist = neighborRadius * avoidanceRadiusMultiplier / 2;

        Quaternion flockInitialRotation = Quaternion.Euler(Vector3.forward * 90); //Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)); //rotacao "aleatoria" para o novo flock (de 0 a 360)

        for (int i = 0; i < qtdOfLines; i++) //para cada objeto
        {
            Vector2 flockInitialPosition = UnityEngine.Random.insideUnitCircle * startingCount * agentDensity; //posicao "aleatoria" dentro do range especificado para o novo flock

            for (int j = 0; j < lineHeight; j++) //para cada objeto
            {
                for (int k = 0; k < lineSize; k++) //para cada objeto
                {
                    int index = k + lineSize * (j + lineHeight * i) + (flockLayer) * qtdObjects;
                    //Debug.Log(index);
                    Translation newT = new Translation { Value = new float3(flockInitialPosition.x + k * dist, flockInitialPosition.y + j * dist, gameObject.transform.position.z + zPos) };
                    entityManager.SetComponentData(objectAgents_ECS[index], newT); //setar a posicao
                    Rotation newR = new Rotation { Value = flockInitialRotation };
                    entityManager.SetComponentData(objectAgents_ECS[index], newR); //setar a rotacao

                    objectAgents_ECS_T[index] = newT; //setar arrays auxiliares
                }
            }
        }
    }

    private void OnDrawGizmos() //ao desenhar o gizmos da unity //para testes
    {
        if (showGizmosOnInspector)
        {
            Gizmos.DrawWireSphere(transform.position, startingCount * agentDensity); //desenhar o circulo de spawn dos agentes
            Gizmos.color = new Color(0f, 0.8f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, neighborRadius); //desenhar o circulo de alcance dos agentes //para testes
            Gizmos.color = new Color(0.8f, 0f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, neighborRadius * avoidanceRadiusMultiplier); //desenhar o circulo de "evasao" dos agentes //para testes

            Gizmos.color = new Color(0f, 0f, 0.8f, 1f);
            Gizmos.DrawWireSphere(radiusCenter, radius); //desenhar o circulo de alcance
            Gizmos.color = new Color(0f, 0.8f, 0.8f, 1f);
            Gizmos.DrawWireSphere(radiusCenter, radius * radiusLimiterPer); //desenhar o circulo de alcance interno //para testes
        }
    }

    private void OnApplicationQuit() //ao sair da aplicacao
    {
        flockAgents_ECS.Dispose(); //liberar o array alocado
        flockAgents_ECS_T.Dispose(); //liberar o array alocado
        flockAgents_ECS_R.Dispose(); //liberar o array alocado
        flockAgents_ECS_FW.Dispose(); //liberar o array alocado

        objectAgents_ECS.Dispose(); //liberar o array alocado
        objectAgents_ECS_T.Dispose(); //liberar o array alocado

        behaviorsWeightsNArray.Dispose(); //liberar o array alocado

        neighborRadiusNArray.Dispose(); //liberar o array alocado
        squareNeighborRadiusNArray.Dispose(); //liberar o array alocado
        squareAvoidanceRadiusNArray.Dispose(); //liberar o array alocado
        avoidanceRadiusMultiplierNArray.Dispose(); //liberar o array alocado
    }

    public void RestartFlocks()
    {
        for (int i = 0; i < qtdFlocksInFlock; i++) //para cada flock
        {
            Vector2 flockPosition = UnityEngine.Random.insideUnitCircle * startingCount * agentDensity; //posicao "aleatoria" dentro do range especificado para o novo flock
            Quaternion flockRotation = Quaternion.Euler(Vector3.forward * UnityEngine.Random.Range(0f, 360f)); //rotacao "aleatoria" para o novo flock (de 0 a 360)

            Translation newT = new Translation { Value = new float3(flockPosition.x, flockPosition.y, flockAgents_ECS_T[i].Value.z) };
            entityManager.SetComponentData(flockAgents_ECS[i], newT); //setar a posicao
            Rotation newR = new Rotation { Value = flockRotation };
            entityManager.SetComponentData(flockAgents_ECS[i], newR); //setar a rotacao

            FlockWho newFW = new FlockWho { flockValue = flockAgents_ECS_FW[i].flockValue, flockManagerValue = flockAgents_ECS_FW[i].flockManagerValue, flockLayerValue = flockAgents_ECS_FW[i].flockLayerValue, flockCollisionCount = 0, objectCollisionCount = 0 };
            entityManager.SetComponentData(flockAgents_ECS[i], newFW); //setar a numeracao do flock e do respectivo manager

            //newFlockAgent.flockManager = this; //ajustar quem criou o flock
            flockAgents_ECS_T[i] = newT; //setar arrays auxiliares
            flockAgents_ECS_R[i] = newR;
            flockAgents_ECS_FW[i] = newFW;
        }

        for (int i = 0; i < qtdFlocks; i++)
        {
            squareNeighborRadiusNArray[i] = neighborRadiusNArray[i] * neighborRadiusNArray[i];
            squareAvoidanceRadiusNArray[i] = squareNeighborRadiusNArray[i] * avoidanceRadiusMultiplierNArray[i] * avoidanceRadiusMultiplierNArray[i];
        }
    }
}
