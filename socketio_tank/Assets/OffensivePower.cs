using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffensivePower : MonoBehaviour
{
    public float power = default;
    public float Power { get; private set; }
    void Start()
    {
        Power = power;
    }
}
