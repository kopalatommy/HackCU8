using SharpNeat.Genomes.Neat;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeatTest : MonoBehaviour
{
    private void Start()
    {
        NeatGenomeFactory neatGenomeFactory = new NeatGenomeFactory(10, 10);
    }
}
