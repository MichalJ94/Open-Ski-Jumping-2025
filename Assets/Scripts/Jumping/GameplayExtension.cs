using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping
{
    [CreateAssetMenu]
    public class GameplayExtension : ScriptableObject
    {
        // Start is called before the first frame update

        public float inrunDragModifier (int skill)
        {
            float modifier = 0.00001f;
            return (80-skill)*modifier;
             
        }


    }
}
