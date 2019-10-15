using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/AlignmentBehavior")] //criar uma aba para criar o scriptable object
public class AlignmentBehavior : FilteredFlockBehavior
{ //alinhamento -> objetos/flocks/agents andarem "juntos" / "na mesma direcao" em "harmonia" 
    //public

    //private

    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        if (nearObjects.Count == 0) return flockAgent.transform.up; //se nao tiver objetos proximos, retornar/manter a mesma direcao na qual estava "andando"

        List<Transform> filteredNearObjects = (filter == null) ? nearObjects : filter.Filter(flockAgent, nearObjects); //verificar se precisa filtrar/filtrar objetos proximos para pegar apenas os do "flock necessario"

        Vector2 alignmentMove = Vector2.zero; //inicializar valores
        foreach (Transform obj in filteredNearObjects) //para cada objeto "proximo"
        {
            alignmentMove += (Vector2)obj.transform.up; //somar a direcao dos objetos
        }
        alignmentMove /= nearObjects.Count; //tirar a "media" das direcoes somadas

        return alignmentMove; //retornar //(nao precisa de diferenca de "alignment local", pois eh unico)
    }
}
