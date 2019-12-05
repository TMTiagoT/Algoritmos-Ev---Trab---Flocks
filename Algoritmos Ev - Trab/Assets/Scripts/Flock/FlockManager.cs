using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    //public
    public FlockAgent flockAgentPrefab; //script flockAgent
    public FlockBehavior flockBehavior; //script flockBehavior

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

    [Space(5)]
    public Color noNeighborColor; //cor de um agente quando nao tem objetos perto //default branco
    public Color fullNeighborColor; //cor de um agente quando tem muitos/todos objetos perto //default vermelho
    [Range(1f, 50f)]
    public int fullNeighborColorCount; //quantidade de objetos proximos ao agente para setar a cor "fullNeighborColor" completa //default 6

    [Space(5)]
    public bool showGizmosOnInspector; //se eh para mostrar ou nao o gizmos no inspector //(para testes)

    [HideInInspector]
    public float squareAvoidanceRadius; //variavel auxiliar

    //private
    private List<FlockAgent> flockAgents; //lista de scripts flockAgent

    private float squareMaxSpeed; //variavel auxiliar
    private float squareNeighborRadius; //variavel auxiliar


    // Use this for initialization
    void Start()
    {
        flockAgents = new List<FlockAgent>(); //inicializar valores

        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;

        InitializeFlock(); //inicializar cena/flocks
    }

    public void InitializeFlock() //inicializar cena/flocks
    {
        for (int i = 0; i < startingCount; i++) //para cada flock
        {
            Vector2 flockPosition = Random.insideUnitCircle * startingCount * agentDensity; //posicao "aleatoria" dentro do range especificado para o novo flock
            Quaternion flockRotation = Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)); //rotacao "aleatoria" para o novo flock (de 0 a 360)

            FlockAgent newFlockAgent = Instantiate(flockAgentPrefab, new Vector3(flockPosition.x, flockPosition.y, gameObject.transform.position.z), flockRotation, transform); //instanciar novo flock (como filho do manager)
            newFlockAgent.name = "Agent " + i; //ajustar nome
            newFlockAgent.flockManager = this; //ajustar quem criou o flock

            flockAgents.Add(newFlockAgent); //adicionar novo flock na lista de flocks
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (FlockAgent agent in flockAgents) //para cada agente na lista de agentes
        {
            List<Transform> nearObjects = FindNearbyObjects(agent); //encontrar lista de objetos proximos ao agente
            agent.agenteSpriteRenderer.color = Color.Lerp(noNeighborColor, fullNeighborColor, nearObjects.Count / (float)fullNeighborColorCount); //alterar cor do agente baseado no numero de objetos em volta //para testes

            Vector2 movement = flockBehavior.CalculateMove(agent, nearObjects, this); //calcular movimento do agente
            movement *= driveFactor; //ajustar velocidade/movimento do agente

            if (movement.sqrMagnitude > squareMaxSpeed) //se passar da velocidade maxima
            {
                movement = movement.normalized * maxSpeed; //colocar agente na velocidade maxima
            }

            agent.Move(movement); //movimentar o agente
        }
    }

    public List<Transform> FindNearbyObjects(FlockAgent agent) //encontra uma lista de objetos proximos ao objeto passado
    {
        List<Transform> nearObjects = new List<Transform>(); //inicializar valores

        Collider2D[] nearObjectsColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius); //encontrar os colliders ao redor do agente passado em um raio (/circulo em volta do agente)

        foreach (Collider2D col2D in nearObjectsColliders) //para cada collider na lista dos encontrados "perto" do agente
        {
            if (col2D != agent.agentCollider2D) //se nao for o proprio collider do objeto/agente
            {
                nearObjects.Add(col2D.transform); //adicionar transform do objeto na lista de transforms de objetos "proximos"
            }
        }

        return nearObjects;
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

            if (flockBehavior.GetType() == typeof(CompositeBehavior)) //se for um composite behavior
            {
                CompositeBehavior compositeBehavior = (CompositeBehavior)flockBehavior;
                if (compositeBehavior.flockBehaviors != null) //se nao for null
                {
                    for (int i = 0; i < compositeBehavior.flockBehaviors.Length; i++) //para cada comportamento no array
                    {
                        if (compositeBehavior.flockBehaviors[i].GetType() == typeof(StayInRadiusBehavior)) //se for um stayInRadius behavior
                        {
                            ((StayInRadiusBehavior)compositeBehavior.flockBehaviors[i]).OnDrawGizmos(); //desenhar o gizmos (se possivel)
                        }
                    }
                }
            }
        }
    }
}
