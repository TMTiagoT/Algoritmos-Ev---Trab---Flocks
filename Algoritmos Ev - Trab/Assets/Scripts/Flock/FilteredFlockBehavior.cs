using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FilteredFlockBehavior : FlockBehavior
{
    //public
    public ContextFilter filter; //filtro para um flock (para filtrar vizinhos, etc.)
}
