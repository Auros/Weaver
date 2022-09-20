using System.Diagnostics;
using UnityEngine;

namespace Weaver
{
    public class Runner : MonoBehaviour
    {
        private void Start()
        {
            var sw = Stopwatch.StartNew();
            WeaverAssembler assembler = new(@"P:\Development\Beat Saber\Mods\SiraUtil\.git");
            sw.Stop();
            
            print(sw.ElapsedMilliseconds + "ms");
            
        }
    }
}