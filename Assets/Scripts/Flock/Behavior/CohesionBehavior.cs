using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/CohesionBehavior")] //criar uma aba para criar o scriptable object
public class CohesionBehavior : FilteredFlockBehavior
{ //coesao -> objetos/flocks/agents andarem "juntos" / "agruparem" em "harmonia" 
    //public

    //private

    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        if (nearObjects.Count == 0) return Vector2.zero; //se nao tiver objetos proximos, retornar "0"

        List<Transform> filteredNearObjects = (filter == null) ? nearObjects : filter.Filter(flockAgent, nearObjects); //verificar se precisa filtrar/filtrar objetos proximos para pegar apenas os do "flock necessario"

        Vector2 cohesionMove = Vector2.zero; //inicializar valores
        foreach (Transform obj in filteredNearObjects) //para cada objeto "proximo"
        {
            cohesionMove += (Vector2)obj.position; //somar a posicao do objeto
        }
        cohesionMove /= nearObjects.Count; //tirar a "media" das posicoes somadas

        cohesionMove -= (Vector2)flockAgent.transform.position; //tirar a diferenca da posicao global para a posicao local do objeto

        return cohesionMove; //retornar
    }
}
