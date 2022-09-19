using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Weaver
{
    public class Runner : MonoBehaviour
    {
        private void Start()
        {
            WeaverAssembler assembler = new(@"P:\Development\Beat Saber\Mods\SiraUtil\.git");
        }
    }
}
