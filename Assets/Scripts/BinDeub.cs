using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class BinDeub : MonoBehaviour
{
    public int a, b;
    public bool reverse, reverse2;
    public string orginal, result;
    // Update is called once per frame
    void Update()
    {
        if (reverse2)
        {
            a = Convert.ToInt32(orginal, 2);
            b = Convert.ToInt32(result, 2);
        }
        else
        {
            orginal = Convert.ToString(a, 2);
            if (reverse)
            {
                result = Convert.ToString(a << b, 2);
            }
            else
            {
                result = Convert.ToString(a >> b, 2);
            }
        }
    }
}
