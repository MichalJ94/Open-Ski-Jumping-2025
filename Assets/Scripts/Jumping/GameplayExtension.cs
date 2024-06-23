using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping
{
    [CreateAssetMenu]
    public class GameplayExtension : ScriptableObject
    {
        // Start is called before the first frame update

        /*public float inrunDragModifier (int skill)
        {
            float modifier = 0.000005f;
            return (80-skill)*modifier;
             
        }*/


        public float forceScaleModifier(int skill)
        {
            float modifier = 0.008f;
            return (80 - skill) * modifier;

        }

    }
}
