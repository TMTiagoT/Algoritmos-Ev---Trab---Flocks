using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EvolutionManager : MonoBehaviour
{
    [Header("References")]
    public FlockManager flockManager;

    [Header("Evolution parameter")]
    public int numberOfGenerations = 100;
    public float generationTime = 30f;
    public int generationSize = 20;
    [Range(0,1)]
    public float mutationRate = 0.25f;
    public int genomeCount = 7;
    public int evaluationRuns = 2;

    [Header("System parameters")]
    public float minBehaviour = 0;
    public float maxBehaviour = 10f;
    public float maxWeight = 5;
    public float minDistance;
    public float maxDistance;
    [Range(-1,1)]
    public float mutationMin = -0.1f;
    [Range(-1,1)]
    public float mutationMax = 0.1f;

    [Header("Results")]
    public string resultParametersPath = "Assets/Resources/parameters.txt";
    public string resultScorePath = "Assets/Resources/score.csv";

    Individual[] generation;
    Individual[] nextGeneration;


    List<Individual> bestOfGeneration;
    float[] scoreSum;
    int currentEvaluation;
    protected int currentGenerationIndex = 0;
    protected int currentBestIndex = -1;

    //Initialize population
    public void Start()
    {
        if(flockManager == null)
            flockManager = GetComponent<FlockManager>();

        generationSize = flockManager.flocksQuantityPerLayer;
        
        generation = new Individual[generationSize];
        nextGeneration = new Individual[generationSize];
        bestOfGeneration = new List<Individual>();
        scoreSum = new float[generationSize];
        for (int i = 0; i < generationSize; i++) scoreSum[i] = 0;

        InitializeGeneration();
    }

    public void InitializeGeneration()
    {
        for (int i = 0; i < generationSize; i++)
        {
            generation[i] = new Individual(genomeCount,0);

            // Assign behaviours weights
            int numBehaviour = flockManager.behaviorsWeights.Length;
            for (int b = 0; b < numBehaviour; b++)
            {
                generation[i].genome[b]= flockManager.behaviorsWeightsNArray[i*numBehaviour + b];
            }
            // Assign distance weights
            generation[i].genome[numBehaviour]= flockManager.neighborRadiusNArray[i];
            generation[i].genome[numBehaviour+1]= flockManager.avoidanceRadiusMultiplierNArray[i];

            //Normalize values
            float biggestValue = 0;
            //Find biggest value
            for (int j = 0; j < genomeCount; j++)
            {
                float normalizedValue = 0;
                if(j < numBehaviour)
                    normalizedValue = Mathf.InverseLerp(minBehaviour, maxBehaviour, generation[i].genome[j]);
                else if(j == numBehaviour)
                    normalizedValue = Mathf.InverseLerp(minDistance, maxDistance, generation[i].genome[j]);
                else
                    normalizedValue = generation[i].genome[j];

                if(normalizedValue > biggestValue)
                    biggestValue = normalizedValue;
            }

            //Normalize the genes
            for (int j = 0; j < genomeCount; j++)
            {
                float normalizedValue = 0;
                if(j < numBehaviour)
                    normalizedValue = Mathf.InverseLerp(minBehaviour, maxBehaviour, generation[i].genome[j]);
                else if(j == numBehaviour)
                    normalizedValue = Mathf.InverseLerp(minDistance, maxDistance, generation[i].genome[j]);
                else
                    normalizedValue = generation[i].genome[j];

                generation[i].genome[j] = normalizedValue / biggestValue;
            }
            
        }
        StartCoroutine(waitCycle(generationTime));
    }

    

    void EvolutionCicle()
    {
        Evaluate();
        MatchParents();
        CrossParents();
        Mutation();
        NormalizeGenes();
        AssignNewGeneration();
        Restart();

        Debug.Log("Best:" + AgentToString(bestOfGeneration[bestOfGeneration.Count-1]));
    }

    void FinishTraining()
    {
        StreamWriter writer = new StreamWriter(resultParametersPath, false);
        
        writer.Write(AgentToString(bestOfGeneration[bestOfGeneration.Count-1]));
        writer.Close();

        StreamWriter writer2 = new StreamWriter(resultScorePath,false);

        string csv = "Score;b0;b1;b2;b3;b4;distance;avoidance;\n";
        for (int i = 0; i < bestOfGeneration.Count; i++)
        {
            csv += AgentToCsvLine(bestOfGeneration[i]);
        }

        writer2.Write(csv);
        writer2.Close();
    }

    string AgentToString(Individual indiv)
    {
        string s = "Score: " + indiv.score + "; weights: [";
        // Assign behaviours weights
        int numBehaviour = flockManager.behaviorsWeights.Length;
        for (int b = 0; b < numBehaviour; b++)
        {
            s  += Mathf.Lerp(minBehaviour,maxBehaviour, indiv.genome[b]) + ";";
        }
        // Assign distance weights
        s += Mathf.Lerp(minDistance, maxDistance, indiv.genome[numBehaviour]) + ";";
        s += indiv.genome[numBehaviour+1] + "]";

        return s;
    }

    string AgentToCsvLine(Individual indiv)
    {
        string s = indiv.score + ";";
        // Assign behaviours weights
        int numBehaviour = flockManager.behaviorsWeights.Length;
        for (int b = 0; b < numBehaviour; b++)
        {
            s  += Mathf.Lerp(minBehaviour,maxBehaviour, indiv.genome[b]) + ";";
        }
        // Assign distance weights
        s += Mathf.Lerp(minDistance, maxDistance, indiv.genome[numBehaviour]) + ";";
        s += indiv.genome[numBehaviour+1] + "\n";

        return s;
    }



    IEnumerator waitCycle(float timeToWait)
    {
        while(currentGenerationIndex < numberOfGenerations)
        {
            yield return new WaitForSecondsRealtime(timeToWait);
            Debug.Log("Time ended");
            if(currentEvaluation < evaluationRuns)
                EvaluateRun();
            if(currentEvaluation >= evaluationRuns)
                EvolutionCicle();
        }
        FinishTraining();
    }

    void EvaluateRun()
    {
        List<int> numFlockColision = new List<int>();
        List<int> numObjectColision = new List<int>();

        //Initialize values
        for (int i = 0; i < generationSize; i++)
        {
            numFlockColision.Add(0);
            numObjectColision.Add(0);
        }

        //Find number of colisions per flock
        int numAgents = flockManager.flockAgents_ECS_FW.Length;
        int numAgentsOnSwarm = numAgents / generationSize;
        for (int i = 0; i < numAgents; i++)
        {
            FlockWho agent = flockManager.flockAgents_ECS_FW[i];
            numFlockColision[agent.flockManagerValue] += agent.flockCollisionCount;
            numObjectColision[agent.flockManagerValue] += agent.objectCollisionCount;
        }

        // Make the score
        for (int i = 0; i < generationSize; i++)
        {
            float meanFlockColision = (float) numFlockColision[i] / (float) numAgentsOnSwarm;
            float meanObjectColision = (float) numObjectColision[i] / (float) numAgentsOnSwarm;
            float score =  meanFlockColision + meanObjectColision;
            scoreSum[i] += score;
        }

        currentEvaluation++;
        
        flockManager.RestartFlocks();
    }

    //Evaluate
    void Evaluate()
    {
        
        float bestScore = Mathf.Infinity;
        int bestScoreIndex = -1;

        // Make the score
        for (int i = 0; i < generationSize; i++)
        {
            float score =  scoreSum[i]/evaluationRuns;
            generation[i].score = score;

            if(score < bestScore)
            {
                bestScore = score;
                bestScoreIndex = i;
            }
            Debug.Log("Geracao " + currentGenerationIndex + ", flock " + i + ", score: " + score);
        }

        bestOfGeneration.Add(new Individual(generation[bestScoreIndex]));
        currentBestIndex = bestScoreIndex;

        for (int i = 0; i < generationSize; i++) scoreSum[i] = 0;
        currentEvaluation = 0;

    }

    // Find parents
    /*
        Maneiras de selecionar pais
        1) Melhor reproduz com todos
        2) Roleta: Seleciona um aleatório usando pesos quanto melhor of fitness
        3) Torneio de 2: seleciona dois aleatoriamente, duela pelo melhor fitness. Esse é um dos pais
        Repete processo pro outro pai. Faz o mesmo até obter todos os filhos
    */
    void MatchParents()
    {
        //Será torneio. Selecionar 4 pais, não fazer para o melhor
        for (int i = 0; i < generationSize; i++)
        {
            if(i == currentBestIndex) continue;

            int p1,p2;
            int p11 = Random.Range(0,generationSize);
            int p12 = Random.Range(0,generationSize);
            int p21 = Random.Range(0,generationSize);
            int p22 = Random.Range(0,generationSize);

            //Melhor primeiro pai
            if(generation[p11].score < generation[p12].score)
                p1 = p11;
            else
                p1 = p12;

            // Melhor segundo pai
            if(generation[p21].score < generation[p22].score)
                p2 = p21;
            else
                p2 = p22;

            generation[i].indexParent1 = p1;
            generation[i].indexParent2 = p2;

        }
    }

    /*
        Essa etapa não é necessária, mas pode ajudar
        Você pode trocar em qualquer ponto, ou a partir de um ponto
        Nesse caso, será feito uma troca de genes entre pais
    */
    // Cross over
    void CrossParents()
    {
        for (int i = 0; i < generationSize; i++)
        {
            nextGeneration[i].Copy(generation[i]);
            nextGeneration[i].generation = currentGenerationIndex + 1;
            if(i == currentBestIndex) continue;

            int index1 = generation[i].indexParent1;
            int index2 = generation[i].indexParent2;
            nextGeneration[i].CrossParents(generation[index1],generation[index2]);
        }
    }


    // MUtation
    void Mutation()
    {
        for (int i = 0; i < generationSize; i++)
        {
            if(i == currentBestIndex) continue;

            if(Random.Range(0,1) < mutationRate)
                nextGeneration[i].Mutate(mutationMin,mutationMax);
        }
    }

    void NormalizeGenes()
    {
        for (int i = 0; i < generationSize; i++)
        {
            float highGene = 0;
            for (int j = 0; j < genomeCount; j++)
            {
                if(nextGeneration[i].genome[j] < 0)
                    nextGeneration[i].genome[j] = 0;
                if(nextGeneration[i].genome[j] > highGene)
                    highGene = nextGeneration[i].genome[j];
            }
            for (int j = 0; j < genomeCount; j++)
                nextGeneration[i].genome[j] /= highGene;
        }
    }

    // Rearange
    void AssignNewGeneration()
    {
        for (int i = 0; i < generationSize; i++)
        {
            generation[i].Copy(nextGeneration[i]);
        }
        currentGenerationIndex++;
    }

    void Restart()
    {
        for (int i = 0; i < generationSize; i++)
        {
            // Assign behaviours weights
            int numBehaviour = flockManager.behaviorsWeights.Length;
            for (int b = 0; b < numBehaviour; b++)
            {
                flockManager.behaviorsWeightsNArray[i*numBehaviour + b] = Mathf.Lerp(minBehaviour,maxBehaviour, generation[i].genome[b]);
            }
            // Assign distance weights
            flockManager.neighborRadiusNArray[i] = Mathf.Lerp(minDistance, maxDistance, generation[i].genome[numBehaviour]);
            flockManager.avoidanceRadiusMultiplierNArray[i] = generation[i].genome[numBehaviour+1];
        }
        flockManager.RestartFlocks();
    }

}
