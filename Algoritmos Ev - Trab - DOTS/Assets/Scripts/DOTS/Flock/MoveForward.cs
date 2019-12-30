using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

//[UpdateBefore(typeof(MoveForward))] //encontrar a velocidade/movimento antes de mover
public class MoveForward : JobComponentSystem
{
    [BurstCompile]
    struct MoveForwardJob : IJobForEach<Translation, Rotation, FlockWho>
    {
        //dados de movimentacao
        [ReadOnly]
        public float timeDT; //variaveis necessarias
        [ReadOnly]
        public int qtdBehaviors; //quantidade de behaviors sendo utilizados
        [ReadOnly]
        public NativeArray<float> behaviorsWeights; //pesos dos comportamentos nas acoes do objeto
        [ReadOnly]
        public float agentSmoothTime; //tempo para "suavizar" o movimento //default 0.5
        [ReadOnly]
        public float3 radiusCenter; //centro do raio //default (0, 0)
        [ReadOnly]
        public float radius; //raio //default 5f
        [ReadOnly]
        public float radiusLimiterPer; //porcentagem de limite para sair/chegar perto do raio maximo para comecar a afetar os objetos //default 0.9f

        [ReadOnly]
        public NativeArray<Translation> possibleNearEntitiesT; //entidades que podem ser consideradas para estarem proximas
        [ReadOnly]
        public NativeArray<Rotation> possibleNearEntitiesR; //entidades que podem ser consideradas para estarem proximas
        [ReadOnly]
        public NativeArray<FlockWho> possibleNearEntitiesFW; //entidades que podem ser consideradas para estarem proximas
        [ReadOnly]
        public NativeArray<Translation> possibleNearObjectsT; //lista de flocks (translation)

        //dados de conta
        [ReadOnly]
        public float driveFactor; //fator de multiplicacao na velocidade de reajuste dos agentes //default 10
        [ReadOnly]
        public float maxSpeed; //velocidade maxima de um agente //default 5

        //[ReadOnly]
        //public float neighborRadius; //raio ao redor do agente que ira considerar objetos //default 1.5
        //[ReadOnly]
        //public float avoidanceRadiusMultiplier; //distancia minima de um agente do outro (para separar --> com relacao ao neighborRadius) //default 0.5

        [ReadOnly]
        public float squareMaxSpeed; //variavel auxiliar //default 25
        //[ReadOnly]
        //public float squareAvoidanceRadius; //variavel auxiliar

        [ReadOnly]
        public NativeArray<float> neighborRadiusNArray; //neighborRadius dos objetos
        //[ReadOnly]
        //public NativeArray<float> squareNeighborRadiusNArray; //squareNeighborRadius dos objetos
        [ReadOnly]
        public NativeArray<float> avoidanceRadiusMultiplierNArray; //avoidanceRadiusMultiplier dos objetos
        [ReadOnly]
        public NativeArray<float> squareAvoidanceRadiusNArray; //squareAvoidanceRadius dos objetos

        private float3 currentVelocity; //velocidade atual do behavior
        private int index; //valor/posicao do flock

        //---
        [ReadOnly]
        public int layersQuantity; //quantidade de layers para spawnar
        [ReadOnly]
        public int flocksQuantityPerLayer; //quantidade de flocks por layer para spawnar
        [ReadOnly]
        public int startingCount; //quantidade inicial de agentes na cena //default 250

        [ReadOnly]
        public int qtdObjects;//quantidade total de objetos nos objetos

        public void Execute(ref Translation pos, ref Rotation rot, ref FlockWho flockWho)
        {
            index = flockWho.flockValue + (flockWho.flockManagerValue) * flocksQuantityPerLayer;

            float3 velocity = GetFlockMovement(pos.Value, rot.Value, ref flockWho); //pegar o movimento/velocidade do flock (+ direcao)
            velocity.z = 0f;

            if (!velocity.Equals(float3.zero))
                rot.Value = quaternion.LookRotation(math.forward(rot.Value), velocity); //setar a rotacao
            else
                rot.Value = quaternion.LookRotation(math.forward(rot.Value), math.up()); //setar a rotacao
            pos.Value += velocity * timeDT; //setar a posicao
        }

        public float3 GetFlockMovement(float3 agentPos, quaternion rot, ref FlockWho flockWho) //retornar o movimento/velocidade do flock (+ direcao)
        {
            NativeList<float3> nearObjectsT = FindNearbyFlocks(agentPos, flockWho, out NativeList<quaternion> nearObjectsR); //encontrar lista de objetos proximos ao agente
            NativeList<float3> nearObjectsOT = FindNearbyObjects(agentPos, ref flockWho); //encontrar lista de objetos proximos ao agente

            float3 movement = CalculateMove_CompositeBehavior(agentPos, nearObjectsT, flockWho.flockManagerValue, rot, nearObjectsR, nearObjectsOT, flockWho); //calcular movimento do agente
            movement *= driveFactor; //ajustar velocidade/movimento do agente

            float lenAux = math.length(movement);
            if (lenAux * lenAux > squareMaxSpeed) //se passar da velocidade maxima
            {
                movement = math.normalize(movement) * maxSpeed; //colocar agente na velocidade maxima
            }

            nearObjectsT.Dispose();
            nearObjectsR.Dispose();
            nearObjectsOT.Dispose();
            return movement; //retornar o movimento do agente
        }

        public NativeList<float3> FindNearbyFlocks(float3 agentPos, FlockWho flockWho, out NativeList<quaternion> nearObjectsR) //encontra uma lista de objetos proximos ao objeto passado
        {
            NativeList<float3> nearObjectsT = new NativeList<float3>(Allocator.Temp); //inicializar valores
            nearObjectsR = new NativeList<quaternion>(Allocator.Temp); //inicializar valores

            int startVal = (flockWho.flockManagerValue) * startingCount;
            for (int i = startVal; i < startVal + startingCount; i++)
            {
                //Debug.Log(possibleNearEntitiesT[i].Value);
                float distAux = math.distance(possibleNearEntitiesT[i].Value, agentPos);
                if (possibleNearEntitiesFW[i].flockValue != flockWho.flockValue &&
                    distAux < neighborRadiusNArray[flockWho.flockManagerValue]) //se nao for o proprio collider do objeto/agente (e estiver dentro da range)
                {
                    nearObjectsT.Add(possibleNearEntitiesT[i].Value); //adicionar transform do objeto na lista de transforms de objetos "proximos"
                    nearObjectsR.Add(possibleNearEntitiesR[i].Value); //adicionar transform do objeto na lista de transforms de objetos "proximos"

                    if (distAux < neighborRadiusNArray[flockWho.flockManagerValue] * avoidanceRadiusMultiplierNArray[flockWho.flockManagerValue]) //se "bater" no obstaculo
                    {
                        flockWho.flockCollisionCount += 1;
                    }
                }
            }

            return nearObjectsT;
        }

        public float3 CalculateMove_CompositeBehavior(float3 flockAgent, NativeList<float3> nearObjectsT, int flockManagerN, quaternion rot, NativeList<quaternion> nearObjectsR, NativeList<float3> nearObjectsOT, FlockWho flockWho) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
        {
            float3 compositeMovement = float3.zero; //inicializar valores
            float3 partialMovement;
            float lenAux;

            int posBehavior = flockManagerN * qtdBehaviors;
            //steered cohesion
            partialMovement = CalculateMove_SteeredCohesionBehavior(flockAgent, nearObjectsT, flockManagerN, rot, nearObjectsR) * behaviorsWeights[posBehavior + 0]; //atribuir valor do movimento para o comportamento
            if (!partialMovement.Equals(float3.zero)) //se for diferente de "0"
            {
                lenAux = math.length(partialMovement);
                if (lenAux * lenAux > behaviorsWeights[posBehavior + 0] * behaviorsWeights[posBehavior + 0]) //se o movimento for maior que o peso
                {
                    partialMovement = math.normalize(partialMovement); //normalizar movimento
                    partialMovement *= behaviorsWeights[posBehavior + 0]; //colocar no "maximo" para o peso
                }
                compositeMovement += partialMovement; //adicionar valor no movimento total
            }

            //alignment
            partialMovement = CalculateMove_AlignmentBehavior(flockAgent, nearObjectsT, flockManagerN, rot, nearObjectsR) * behaviorsWeights[posBehavior + 1]; //atribuir valor do movimento para o comportamento
            if (!partialMovement.Equals(float3.zero)) //se for diferente de "0"
            {
                lenAux = math.length(partialMovement);
                if (lenAux * lenAux > behaviorsWeights[posBehavior + 1] * behaviorsWeights[posBehavior + 1]) //se o movimento for maior que o peso
                {
                    partialMovement = math.normalize(partialMovement); //normalizar movimento
                    partialMovement *= behaviorsWeights[posBehavior + 1]; //colocar no "maximo" para o peso
                }
                compositeMovement += partialMovement; //adicionar valor no movimento total
            }

            //stay in radius
            partialMovement = CalculateMove_StayInRadiusBehavior(flockAgent, nearObjectsT, flockManagerN, rot, nearObjectsR) * behaviorsWeights[posBehavior + 2]; //atribuir valor do movimento para o comportamento
            if (!partialMovement.Equals(float3.zero)) //se for diferente de "0"
            {
                lenAux = math.length(partialMovement);
                if (lenAux * lenAux > behaviorsWeights[posBehavior + 2] * behaviorsWeights[posBehavior + 2]) //se o movimento for maior que o peso
                {
                    partialMovement = math.normalize(partialMovement); //normalizar movimento
                    partialMovement *= behaviorsWeights[posBehavior + 2]; //colocar no "maximo" para o peso
                }
                compositeMovement += partialMovement; //adicionar valor no movimento total
            }

            //avoidance
            partialMovement = CalculateMove_AvoidanceBehavior(flockAgent, nearObjectsT, flockManagerN, rot, nearObjectsR, flockWho) * behaviorsWeights[posBehavior + 3]; //atribuir valor do movimento para o comportamento
            if (!partialMovement.Equals(float3.zero)) //se for diferente de "0"
            {
                lenAux = math.length(partialMovement);
                if (lenAux * lenAux > behaviorsWeights[posBehavior + 3] * behaviorsWeights[posBehavior + 3]) //se o movimento for maior que o peso
                {
                    partialMovement = math.normalize(partialMovement); //normalizar movimento
                    partialMovement *= behaviorsWeights[posBehavior + 3]; //colocar no "maximo" para o peso
                }
                compositeMovement += partialMovement; //adicionar valor no movimento total
            }

            //avoidance objects
            partialMovement = CalculateMove_AvoidanceBehavior(flockAgent, nearObjectsOT, flockManagerN, rot, nearObjectsR, flockWho) * behaviorsWeights[posBehavior + 4]; //atribuir valor do movimento para o comportamento
            if (!partialMovement.Equals(float3.zero)) //se for diferente de "0"
            {
                lenAux = math.length(partialMovement);
                if (lenAux * lenAux > behaviorsWeights[posBehavior + 4] * behaviorsWeights[posBehavior + 4]) //se o movimento for maior que o peso
                {
                    partialMovement = math.normalize(partialMovement); //normalizar movimento
                    partialMovement *= behaviorsWeights[posBehavior + 4]; //colocar no "maximo" para o peso
                }
                compositeMovement += partialMovement; //adicionar valor no movimento total
            }

            return compositeMovement; //retornar
        }

        public float3 CalculateMove_SteeredCohesionBehavior(float3 flockAgent, NativeList<float3> nearObjectsT, int flockManagerN, quaternion rot, NativeList<quaternion> nearObjectsR) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
        {
            if (nearObjectsT.Length == 0) return float3.zero; //se nao tiver objetos proximos, retornar "0"

            float3 cohesionMove = float3.zero; //inicializar valores
            for (int i = 0; i < nearObjectsT.Length; i++) //para cada objeto "proximo"
            {
                cohesionMove += nearObjectsT[i]; //somar a posicao do objeto
            }
            cohesionMove /= nearObjectsT.Length; //tirar a "media" das posicoes somadas

            cohesionMove -= flockAgent; //tirar a diferenca da posicao global para a posicao local do objeto

            cohesionMove = SmoothDamp(math.mul(rot, math.up()), cohesionMove, ref currentVelocity, agentSmoothTime, 1000000, timeDT); //suavizar o movimento do objeto

            return cohesionMove; //retornar
        }

        public float3 CalculateMove_AlignmentBehavior(float3 flockAgent, NativeList<float3> nearObjectsT, int flockManagerN, quaternion rot, NativeList<quaternion> nearObjectsR) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
        {
            if (nearObjectsR.Length == 0) return math.mul(rot, math.up()); //se nao tiver objetos proximos, retornar/manter a mesma direcao na qual estava "andando"

            float3 alignmentMove = float3.zero; //inicializar valores
            for (int i = 0; i < nearObjectsR.Length; i++) //para cada objeto "proximo"
            {
                alignmentMove += math.mul(nearObjectsR[i], math.up()); //somar a direcao dos objetos
            }
            alignmentMove /= nearObjectsR.Length; //tirar a "media" das direcoes somadas

            return alignmentMove; //retornar //(nao precisa de diferenca de "alignment local", pois eh unico)
        }

        public float3 CalculateMove_StayInRadiusBehavior(float3 flockAgent, NativeList<float3> nearObjectsT, int flockManagerN, quaternion rot, NativeList<quaternion> nearObjectsR) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
        {
            float3 centerOffset = radiusCenter - flockAgent; //pegar posicao/distancia do objeto com relacao ao centro

            float inRadiusValue = math.length(centerOffset) / radius; //para saber se o objeto esta dentro do raio (<= 1 --> dentro do raio, > 1 --> fora do raio)

            if (inRadiusValue <= radiusLimiterPer) return float3.zero; //se estiver "dentro do raio" (<= 0.9), nao fazer nada

            return centerOffset * inRadiusValue * inRadiusValue; //retornar nova posicao/movimento do objeto
        }

        public float3 CalculateMove_AvoidanceBehavior(float3 flockAgent, NativeList<float3> nearObjectsT, int flockManagerN, quaternion rot, NativeList<quaternion> nearObjectsR, FlockWho flockWho) //funcao que calcula o movimento de um individuo (baseado tambem nos objetos e/ou "vizinhos" ao seu redor)
        {
            if (nearObjectsT.Length == 0) return float3.zero; //se nao tiver objetos proximos, retornar "0"

            float3 avoidanceMove = float3.zero; //inicializar valores
            int inAvoidRadiusCount = 0;

            float lenAux;
            for (int i = 0; i < nearObjectsT.Length; i++) //para cada objeto "proximo"
            {
                lenAux = math.length(nearObjectsT[i] - flockAgent);
                if (lenAux * lenAux < squareAvoidanceRadiusNArray[flockWho.flockManagerValue]) //verificar se o objeto esta dentro do raio de "evasao"
                {
                    inAvoidRadiusCount += 1; //somar a quantidade de objetos dentro do raio de "evasao"
                    avoidanceMove += (flockAgent - nearObjectsT[i]); //somar a distancia do flock do objeto //(para enviar o agente na direcao contraria para "separar" do objeto)
                }

            }
            if (inAvoidRadiusCount > 0) //se a quantidade de objetos dentro do raio de "evasao" for maior do que zero
                avoidanceMove /= inAvoidRadiusCount; //tirar a "media" das distancias/posicoes somadas

            return avoidanceMove; //retornar
        }

        //objetos
        public NativeList<float3> FindNearbyObjects(float3 agentPos, ref FlockWho flockWho) //encontra uma lista de objetos proximos ao objeto passado
        {
            NativeList<float3> nearObjectsT = new NativeList<float3>(Allocator.Temp); //inicializar valores

            int startVal = (flockWho.flockLayerValue) * qtdObjects;
            for (int i = startVal; i < startVal + qtdObjects; i++)
            {
                //Debug.Log(possibleNearEntitiesT[i].Value);
                float distAux = math.distance(possibleNearObjectsT[i].Value, agentPos);
                if (distAux < neighborRadiusNArray[flockWho.flockManagerValue]) //se nao for o proprio collider do objeto/agente (e estiver dentro da range)
                {
                    nearObjectsT.Add(possibleNearObjectsT[i].Value); //adicionar transform do objeto na lista de transforms de objetos "proximos"

                    if (distAux < neighborRadiusNArray[flockWho.flockManagerValue] * avoidanceRadiusMultiplierNArray[flockWho.flockManagerValue]) //se "bater" no obstaculo
                    {
                        flockWho.objectCollisionCount += 1;
                    }
                }
            }

            return nearObjectsT;
        }

        //funcao da unity (ajustada)
        public float3 SmoothDamp(float3 current, float3 target, ref float3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            float output_x = 0f;
            float output_y = 0f;
            float output_z = 0f;

            // Based on Game Programming Gems 4 Chapter 1.10
            smoothTime = math.max(0.0001F, smoothTime);
            float omega = 2F / smoothTime;

            float x = omega * deltaTime;
            float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

            float change_x = current.x - target.x;
            float change_y = current.y - target.y;
            float change_z = current.z - target.z;
            float3 originalTo = target;

            // Clamp maximum speed
            float maxChange = maxSpeed * smoothTime;

            float maxChangeSq = maxChange * maxChange;
            float sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
            if (sqrmag > maxChangeSq)
            {
                var mag = (float)math.sqrt(sqrmag);
                change_x = change_x / mag * maxChange;
                change_y = change_y / mag * maxChange;
                change_z = change_z / mag * maxChange;
            }

            target.x = current.x - change_x;
            target.y = current.y - change_y;
            target.z = current.z - change_z;

            float temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
            float temp_y = (currentVelocity.y + omega * change_y) * deltaTime;
            float temp_z = (currentVelocity.z + omega * change_z) * deltaTime;

            currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
            currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;
            currentVelocity.z = (currentVelocity.z - omega * temp_z) * exp;

            output_x = target.x + (change_x + temp_x) * exp;
            output_y = target.y + (change_y + temp_y) * exp;
            output_z = target.z + (change_z + temp_z) * exp;

            // Prevent overshooting
            float origMinusCurrent_x = originalTo.x - current.x;
            float origMinusCurrent_y = originalTo.y - current.y;
            float origMinusCurrent_z = originalTo.z - current.z;
            float outMinusOrig_x = output_x - originalTo.x;
            float outMinusOrig_y = output_y - originalTo.y;
            float outMinusOrig_z = output_z - originalTo.z;

            if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0)
            {
                output_x = originalTo.x;
                output_y = originalTo.y;
                output_z = originalTo.z;

                currentVelocity.x = (output_x - originalTo.x) / deltaTime;
                currentVelocity.y = (output_y - originalTo.y) / deltaTime;
                currentVelocity.z = (output_z - originalTo.z) / deltaTime;
            }

            return new float3(output_x, output_y, output_z);
        }
    }

    FlockManager flockManager; //script flockManager
    protected override JobHandle OnUpdate(JobHandle inputDeps) //ao atualizar
    {
        flockManager.UpdateData(); //atualizar os dados

        MoveForwardJob moveForward = new MoveForwardJob //setar variaveis necessarias
        {
            timeDT = Time.deltaTime,
            driveFactor = flockManager.driveFactor,
            maxSpeed = flockManager.maxSpeed,
            squareMaxSpeed = flockManager.squareMaxSpeed,
            //neighborRadius = flockManager.neighborRadius,
            possibleNearEntitiesT = flockManager.flockAgents_ECS_T,
            possibleNearEntitiesR = flockManager.flockAgents_ECS_R,
            possibleNearEntitiesFW = flockManager.flockAgents_ECS_FW,
            qtdBehaviors = flockManager.behaviorsWeights.Length,
            behaviorsWeights = flockManager.behaviorsWeightsNArray,
            agentSmoothTime = flockManager.agentSmoothTime,
            radius = flockManager.radius,
            radiusCenter = flockManager.radiusCenter,
            radiusLimiterPer = flockManager.radiusLimiterPer,
            //squareAvoidanceRadius = flockManager.squareAvoidanceRadius,
            flocksQuantityPerLayer = flockManager.flocksQuantityPerLayer,
            layersQuantity = flockManager.layersQuantity,
            startingCount = flockManager.startingCount,
            //avoidanceRadiusMultiplier = flockManager.avoidanceRadiusMultiplier,
            possibleNearObjectsT = flockManager.objectAgents_ECS_T,
            qtdObjects = flockManager.qtdObjects,
            neighborRadiusNArray = flockManager.neighborRadiusNArray,
            squareAvoidanceRadiusNArray = flockManager.squareAvoidanceRadiusNArray,
            avoidanceRadiusMultiplierNArray = flockManager.avoidanceRadiusMultiplierNArray
        };

        return moveForward.Schedule(this, inputDeps); //colocar job para funcionar
    }

    protected override void OnCreate() //ao criar o manager
    {
        base.OnCreate();
        flockManager = (FlockManager)GameObject.FindObjectOfType(typeof(FlockManager));
    }
}
