using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/AvoidancePointBehavior")] //criar uma aba para criar o scriptable object
public class AvoidancePointBehavior : FilteredFlockBehavior
{ //evasao -> objetos/flocks/agents andarem "juntos" / "separados" (nao tao juntos/evitar obstaculos) em "harmonia" (evitar um ponto)
    //public
    //public float visionRange; //alcance de visao do objeto
    public LayerMask mask; //mascara do ponto/objeto a ser evitado

    //private

    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        if (nearObjects.Count == 0) return Vector2.zero; //se nao tiver objetos proximos, retornar "0"

        Vector2 avoidanceMove = Vector2.zero; //inicializar valores
        int inAvoidRadiusCount = 0;

        List<Transform> filteredNearObjects = (filter == null) ? nearObjects : filter.Filter(flockAgent, nearObjects); //verificar se precisa filtrar/filtrar objetos proximos para pegar apenas os do "flock necessario"

        foreach (Transform obj in filteredNearObjects) //para cada objeto "proximo"
        {
            Vector2 avoidPoint = Physics2D.Raycast(flockAgent.transform.position, (obj.position - flockAgent.transform.position).normalized, 100f, mask).point; //encontrar "ponto de colisao" com o objeto e "evitar" ele
            if (Vector2.SqrMagnitude(avoidPoint - (Vector2)flockAgent.transform.position) < flockManager.squareAvoidanceRadius) //verificar se o objeto esta dentro do raio de "evasao"
            {
                inAvoidRadiusCount += 1; //somar a quantidade de objetos dentro do raio de "evasao"
                avoidanceMove += (Vector2)flockAgent.transform.position - avoidPoint; //somar a distancia do flock do objeto //(para enviar o agente na direcao contraria para "separar" do objeto)
            }

        }
        if (inAvoidRadiusCount > 0) //se a quantidade de objetos dentro do raio de "evasao" for maior do que zero
            avoidanceMove /= inAvoidRadiusCount; //tirar a "media" das distancias/posicoes somadas

        return avoidanceMove; //retornar
    }
}
