using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Individual
{
    public int numberOfGenes;
    public float[] genome;

    public int generation;

    public float score;   

    public int indexParent1;
    public int indexParent2;
    
    public Individual(int numberOfGenes = 7, int generation = -1)
    {
        this.numberOfGenes = numberOfGenes;
        this.generation = generation;
        this.genome = new float[numberOfGenes];
        this.score = 0;
        this.indexParent1 = -1;
        this.indexParent2 = -1;
    }

    public Individual(Individual indiv)
    {
        this.numberOfGenes = indiv.numberOfGenes;
        this.generation = indiv.generation;
        this.genome = new float[numberOfGenes];
        this.score = indiv.score;
        for (int i = 0; i < numberOfGenes; i++)
        {
            this.genome[i] = indiv.genome[i];
        }
        this.indexParent1 = indiv.indexParent1;
        this.indexParent2 = indiv.indexParent2;
    }

    public void CrossParents(Individual p1, Individual p2)
    {
        for(int i = 0; i < numberOfGenes; i++)
        {
            genome[i] = (p1.genome[i] + p2.genome[i]) * 0.5f;
        }
    }

    public void Mutate(float rangeMin, float rangeMax)
    {
        int parameterToMutate = Random.Range(0,numberOfGenes);
        genome[parameterToMutate] += Random.Range(rangeMin, rangeMax);
    }

    public void Copy(Individual indiv)
    {
        this.numberOfGenes = indiv.numberOfGenes;
        this.generation = indiv.generation;
        if(this.genome == null || this.genome.Length != numberOfGenes)
            this.genome = new float[numberOfGenes];
        this.score = indiv.score;
        for (int i = 0; i < numberOfGenes; i++)
        {
            this.genome[i] = indiv.genome[i];
        }
    }

}
