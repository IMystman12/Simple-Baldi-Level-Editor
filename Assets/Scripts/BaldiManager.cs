using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaldiManager : MonoBehaviour
{
    public List<EntityInstance> entities = new List<EntityInstance>();
    public IEnumerator BeginPlay()
    {
        entities.ForEach(a => a.GameStart());
        while (true)
        {
            yield return null;
            entities.ForEach(a => a.GameUpdate());
        }
    }
}
