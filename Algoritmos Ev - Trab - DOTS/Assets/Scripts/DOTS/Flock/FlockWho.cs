using Unity.Entities;
using System;

[Serializable]
public struct FlockWho : IComponentData
{
    public int flockValue; //qual eh o flock no manager
    public int flockManagerValue; //qual o "manager" do flock
    public int flockLayerValue; //qual a layer do flock
    public int flockCollisionCount;
    public int objectCollisionCount;
}