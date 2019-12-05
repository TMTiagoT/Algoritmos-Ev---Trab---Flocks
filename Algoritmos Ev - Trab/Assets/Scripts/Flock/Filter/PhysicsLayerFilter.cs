using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockFilter/PhysicsLayerFilter")] //criar uma aba para criar o scriptable object
public class PhysicsLayerFilter : ContextFilter
{   //filtro de fisica -> filtra objetos/flocks/agents fisicos
    //public
    public LayerMask mask; //mascara dos objetos para filtrar

    //private

    public override List<Transform> Filter(FlockAgent flockAgent, List<Transform> originalList)  //funcao que filtra os flocks de um flock
    {
        List<Transform> filteredFlock = new List<Transform>(); //inicializar valores

        foreach (Transform obj in originalList) //para cada flock na lista
        {
            if (mask == (mask | (1 << obj.gameObject.layer))) //verificar se o objeto esta na lista de layers da mascara
            {
                filteredFlock.Add(obj); //adicionar na lista de flocks "filtrada" para usar
            }
        }

        return filteredFlock; //retornar
    }
}
