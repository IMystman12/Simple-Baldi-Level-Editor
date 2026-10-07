using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public int bin;
    public List<Towards> directions;
    void Update()
    {
        directions = TowardsExtension.OpenTowardsFromBin(bin);
    }
}
