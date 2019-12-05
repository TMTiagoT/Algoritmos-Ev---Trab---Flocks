using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;

//[RequireComponent(typeof(Collider2D))] //requerir que exista um collider2D no objeto
//[RequireComponent(typeof(SpriteRenderer))] //requerir que exista um spriteRenderer no objeto
public class FlockAgent : MonoBehaviour, IConvertGameObjectToEntity
{
    //public
    //[HideInInspector]
    //public Collider2D agentCollider2D; //collider2D
    //[HideInInspector]
    //public SpriteRenderer agenteSpriteRenderer; //spriteRenderer

    //[HideInInspector]
    //public FlockManager flockManager;

    //private


    // Use this for initialization
    void Start()
    {
        //agentCollider2D = gameObject.GetComponent<Collider2D>(); //inicializar valores
        //agenteSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    //public void Move(Vector2 velocity) //mover o objeto
    //{
    //    transform.up = velocity; //ajustar direcao do objeto
    //    transform.position += (Vector3)velocity * Time.deltaTime; //ajustar movimento/posicao do objeto //(constante com o frame rate)
    //}

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponent(entity, typeof(MoveForward));

        //MoveSpeed moveSpeed = new MoveSpeed { value = velocity };
        //dstManager.AddComponent(entity, moveSpeed);

        //dstManager.AddComponent(entity, typeof(Unity.Physics.SphereCollider));
        //dstManager.AddComponentData(entity, new FlockWho { flockValue = 0, flockManagerValue = 0 }); //setar a numeracao do flock e do respectivo manager

    }
}
