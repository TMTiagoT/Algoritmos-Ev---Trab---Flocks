using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] //requerir que exista um collider2D no objeto
[RequireComponent(typeof(SpriteRenderer))] //requerir que exista um spriteRenderer no objeto
public class FlockAgent : MonoBehaviour
{
    //public
    [HideInInspector]
    public Collider2D agentCollider2D; //collider2D
    [HideInInspector]
    public SpriteRenderer agenteSpriteRenderer; //spriteRenderer

    [HideInInspector]
    public FlockManager flockManager;

    //private


    // Use this for initialization
    void Start()
    {
        agentCollider2D = gameObject.GetComponent<Collider2D>(); //inicializar valores
        agenteSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    public void Move(Vector2 velocity) //mover o objeto
    {
        transform.up = velocity; //ajustar direcao do objeto
        transform.position += (Vector3)velocity * Time.deltaTime; //ajustar movimento/posicao do objeto //(constante com o frame rate)
    }
}
