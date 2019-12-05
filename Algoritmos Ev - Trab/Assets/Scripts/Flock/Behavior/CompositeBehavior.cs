using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/CompositeBehavior")] //criar uma aba para criar o scriptable object
public class CompositeBehavior : FlockBehavior
{ //combinar -> combina os comportamendos do objeto/flock/agent 
    //public
    [HideInInspector]
    public FlockBehavior[] flockBehaviors; //comportamentos do objeto
    [HideInInspector]
    public float[] behaviorsWeights; //pesos dos comportamentos nas acoes do objeto

    //private

    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        if (behaviorsWeights.Length != flockBehaviors.Length) //caso o tamanho dos arrays seja diferente
        {
            Debug.Log("Arrays Size Error!");
            return Vector2.zero;
        }

        Vector2 compositeMovement = Vector2.zero; //inicializar valores

        for (int i = 0; i < flockBehaviors.Length; i++) //para cada comportamento
        {
            Vector2 partialMovement = flockBehaviors[i].CalculateMove(flockAgent, nearObjects, flockManager) * behaviorsWeights[i]; //atribuir valor do movimento para o comportamento

            if (partialMovement != Vector2.zero) //se for diferente de "0"
            {
                if (partialMovement.sqrMagnitude > behaviorsWeights[i] * behaviorsWeights[i]) //se o movimento for maior que o peso
                {
                    partialMovement.Normalize(); //normalizar movimento
                    partialMovement *= behaviorsWeights[i]; //colocar no "maximo" para o peso
                }

                compositeMovement += partialMovement; //adicionar valor no movimento total
            }
        }

        return compositeMovement; //retornar
    }
}
