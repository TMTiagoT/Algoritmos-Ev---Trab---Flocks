using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FlockBehavior/StayInRadiusBehavior")] //criar uma aba para criar o scriptable object
public class StayInRadiusBehavior : FlockBehavior
{ //ficar no raio -> objetos/flocks/agents andarem dentro de um raio/range (quanto mais longe do raio "aceitavel", mais "forte" fica a atrcao para o "centro")
    //public
    public Vector2 radiusCenter; //centro do raio //default (0, 0)

    public float radius; //raio //default 5f
    public float radiusLimiterPer; //porcentagem de limite para sair/chegar perto do raio maximo para comecar a afetar os objetos //default 0.9f

    [Space(5)]
    public bool showGizmosOnInspector; //se eh para mostrar ou nao o gizmos no inspector //(para testes)

    //private

    public override Vector2 CalculateMove(FlockAgent flockAgent, List<Transform> nearObjects, FlockManager flockManager) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
    {
        Vector2 centerOffset = radiusCenter - (Vector2)flockAgent.transform.position; //pegar posicao/distancia do objeto com relacao ao centro

        float inRadiusValue = centerOffset.magnitude / radius; //para saber se o objeto esta dentro do raio (<= 1 --> dentro do raio, > 1 --> fora do raio)

        if (inRadiusValue <= radiusLimiterPer) return Vector2.zero; //se estiver "dentro do raio" (<= 0.9), nao fazer nada

        return centerOffset * inRadiusValue * inRadiusValue; //retornar nova posicao/movimento do objeto
    }

    public void OnDrawGizmos() //ao desenhar o gizmos da unity //para testes
    {
        if (showGizmosOnInspector)
        {
            Gizmos.color = new Color(0f, 0f, 0.8f, 1f);
            Gizmos.DrawWireSphere(radiusCenter, radius); //desenhar o circulo de alcance
            Gizmos.color = new Color(0f, 0.8f, 0.8f, 1f);
            Gizmos.DrawWireSphere(radiusCenter, radius * radiusLimiterPer); //desenhar o circulo de alcance interno //para testes
        }
    }
}
