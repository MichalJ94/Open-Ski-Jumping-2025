using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OpenSkiJumping
{
    public class SnowParticleEffect : MonoBehaviour
    {
        public ParticleSystem snowParticleSystem;
        public Rigidbody skierRigidbody; // The skier's rigidbody to track speed

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
            particleRenderer.lengthScale = Mathf.Lerp(1f, 3f, speed / 80f); // Scale the length depending on speed (adjust values as needed)
            snowParticleSystem.startLifetime = Mathf.Lerp(1f, 0.5f, speed / 20f); // Reduce lifetime at higher speeds
            snowParticleSystem.playbackSpeed = Mathf.Lerp(1f, 3f, speed / 20f); // Adjust the speed factor based on the skier's speed
        }
    }
}
