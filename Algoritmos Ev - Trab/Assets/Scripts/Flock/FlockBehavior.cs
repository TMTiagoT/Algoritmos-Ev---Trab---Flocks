using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FlockBehavior : ScriptableObject
{
    public abstract Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager); //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
}
