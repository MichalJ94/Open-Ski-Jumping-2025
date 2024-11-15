using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping
{
    public class SnowParticleEffect : MonoBehaviour
    {
        public ParticleSystem snowParticleSystem;
        public Rigidbody skierRigidbody; // The skier's rigidbody to track speed
        public float lengthScale;
        public float startLifetime;
        public float playbackSpeed;
        private ParticleSystemRenderer particleRenderer;

        void Start()
        {
            particleRenderer = snowParticleSystem.GetComponent<ParticleSystemRenderer>();
        }

        void Update()
        {
            // Get the skier's speed
            float speed = skierRigidbody.velocity.magnitude;

            // Modify the particle system's stretch based on speed
            particleRenderer.lengthScale = Mathf.Lerp(1f, 3f, speed / lengthScale); // Scale the length depending on speed (adjust values as needed)
            snowParticleSystem.startLifetime = Mathf.Lerp(1f, 0.5f, speed / startLifetime); // Reduce lifetime at higher speeds
            snowParticleSystem.playbackSpeed = Mathf.Lerp(1f, 3f, speed / playbackSpeed); // Adjust the speed factor based on the skier's speed
        }
    }
}
