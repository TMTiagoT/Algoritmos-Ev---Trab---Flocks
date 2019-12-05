using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/AvoidanceRadiusBehavior")] //criar uma aba para criar o scriptable object
public class AvoidanceRadiusBehavior : FilteredFlockBehavior
{ //evasao -> objetos/flocks/agents andarem "juntos" / "separados" (nao tao juntos/evitar obstaculos) em "harmonia" (evitar pontos em uma range)
    //public
    //public float visionRange; //alcance de visao do objeto
    public LayerMask mask; //mascara do ponto/objeto a ser evitado

    [Range(0, 359)]
    public int degreesView; //range em graus de visualizacao de um flock (comecando pela frente e indo metade pra cada lado)
    [Range(1, 120)]
    public int spaceBetweenRays; //espaco entre os raios tracados (para lagar menos se precisar)

    //private

    //FALTA CONFERIR E FINALIZAR (TESTAR COM RAYCAST NA TELA TBM, CRIAR OS SCRIPTABLE OBJECTS, ETC.)
    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        if (nearObjects.Count == 0) return Vector2.zero; //se nao tiver objetos proximos, retornar "0"

        Vector2 avoidanceMove = Vector2.zero; //inicializar valores
        int inAvoidRadiusCount = 0;

        List<Transform> filteredNearObjects = (filter == null) ? nearObjects : filter.Filter(flockAgent, nearObjects); //verificar se precisa filtrar/filtrar objetos proximos para pegar apenas os do "flock necessario"

        if (filteredNearObjects.Count > 0) //se tiver objetos perto do flock
        {
            Quaternion flockRotation = flockAgent.gameObject.transform.rotation; //rotacao atual do flock
            for (int i = 0; i < (degreesView / spaceBetweenRays); i++) //para cada grau, ate a metade da visao escolhida
            {
                Quaternion flockRayDirection = flockRotation * Quaternion.Euler(new Vector3(0, 0, (i % 2 == 0) ? i / 2 : -(i - 1) / 2) * spaceBetweenRays); //direcao do raio do flock (0 -> 0 pra direita, 1 -> 0 pra esquerda, 2 -> 1 pra direita, 3 -> 1 pra esquerda, etc.)
                //Quaternion flockRayDirection = Quaternion.Euler(new Vector3(0, 0, (i % 2 == 0) ? i / 2 : -(i - 1) / 2));

                //Debug.DrawLine(flockAgent.transform.position, flockAgent.transform.position + (flockRayDirection * flockAgent.transform.up).normalized, Color.white, 1f);
                Vector2 avoidPoint = Physics2D.Raycast(flockAgent.transform.position, (flockRayDirection * flockAgent.transform.up).normalized, 100f, mask).point; //encontrar "ponto de colisao", se tiver, com um objeto para "evitar" ele
                if (Vector2.SqrMagnitude(avoidPoint - (Vector2)flockAgent.transform.position) < flockManager.squareAvoidanceRadius) //verificar se o objeto esta dentro do raio de "evasao"
                {
                    inAvoidRadiusCount += 1; //somar a quantidade de objetos dentro do raio de "evasao"
                    avoidanceMove += (Vector2)flockAgent.transform.position - avoidPoint; //somar a distancia do flock do objeto //(para enviar o agente na direcao contraria para "separar" do objeto)
                }
            }
        }

        if (inAvoidRadiusCount > 0) //se a quantidade de objetos dentro do raio de "evasao" for maior do que zero
            avoidanceMove /= inAvoidRadiusCount; //tirar a "media" das distancias/posicoes somadas

        return avoidanceMove; //retornar
    }
}
