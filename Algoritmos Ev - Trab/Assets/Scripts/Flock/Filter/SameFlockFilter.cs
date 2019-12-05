using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockFilter/SameFlockFilter")] //criar uma aba para criar o scriptable object
public class SameFlockFilter : ContextFilter
{   //filtro de igualdade -> filtra objetos/flocks/agents que estao no mesmo flock 
    //public

    //private

    public override List<Transform> Filter(FlockAgent flockAgent, List<Transform> originalList)  //funcao que filtra os flocks de um flock
    {
        List<Transform> filteredFlock = new List<Transform>(); //inicializar valores

        foreach (Transform obj in originalList) //para cada flock na lista
        {
            FlockAgent flockAgentAux = obj.GetComponent<FlockAgent>(); //pegar o flock

            if (flockAgentAux != null && flockAgentAux.flockManager == flockAgent.flockManager) //se for "do mesmo flock" do flock passado
            {
                filteredFlock.Add(obj); //adicionar na lista de flocks "filtrada" para usar
            }
        }

        return filteredFlock; //retornar
    }
}
