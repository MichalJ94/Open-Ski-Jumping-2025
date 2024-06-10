﻿using System;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Data;
using OpenSkiJumping.Jumping;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace OpenSkiJumping.New
{
    public class JumperController2 : MonoBehaviour
    {
        [Space] [Header("Flight")] public double angle;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float brakeForce;
        [SerializeField] private float inrunDrag = 0.0011f;
        [SerializeField] private int windThrustDelayCounter = 0;
        [SerializeField] private SkiJumperDataController skiJumperDataController;
        private float torqueCoef = 0f;
        private int WindThrustDeterminer;
        private int WindThrustDeterminerTimesUsed;

        bool button0, button1;
        private bool deductedforlanding;
        public float dirChange;        
        public double drag = 0.001d;
        public float forceChange;
        private int goodSamples;

        private bool judged;

        public JudgesController judgesController;
        public float jumperAngle;

        public JumperModel jumperModel;


        [Space] [Header("Parameters")] public float jumpSpeed;
        [SerializeField] private float forceScale = 1f;

        private int landing;
        public double lift = 0.001d;
        [SerializeField] private GameConfigRuntime gameConfig;


        public UnityEvent OnStartEvent;
        private Rigidbody rb;
        public FloatVariable rotCoef;
        public GameObject rSkiClone, lSkiClone;

        public float sensCoef = 0.01f;
        public float smoothCoef = 0.01f;

        private bool takeoff;
        public int totalSamples;

        // public float mouseSensitivity = 2f; 
        [Space] [Header("Wind")] public Vector2 windDir;
        public float windForce;
        private static readonly int JumperCrash = Animator.StringToHash("JumperCrash");
        private static readonly int JumperState = Animator.StringToHash("JumperState");
        private static readonly int DownForce = Animator.StringToHash("DownForce");
        private static readonly int JumperAngle = Animator.StringToHash("JumperAngle");
        private static readonly int Landing = Animator.StringToHash("Landing");

        public int State
        {
            get => state;
            private set => state = value;
        }

        public float Distance { get; private set; }
        public bool Landed { get; private set; }
        public bool OnInrun { get; private set; }
        public bool OnOutrun { get; private set; }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Inrun"))
            {
                OnInrun = true;
            }

            if (other.CompareTag("LandingArea"))
            {
                OnOutrun = true;
            }

            if (!Landed && other.CompareTag("LandingArea"))
            {
                judgesController.OnDistanceMeasurement((jumperModel.distCollider1.transform.position +
                                                        jumperModel.distCollider2.transform.position) / 2.0f);

                if (!jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Pre-landing"))
                {
                    Crash();
                }

                Landed = true;
                State = 4;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Inrun") && State > 0)
            {
                OnInrun = false;
                judgesController.OnSpeedMeasurement(rb.velocity.magnitude);
            }

            if (other.CompareTag("LandingArea"))
            {
                OnOutrun = false;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (State >= 2 && !jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Take-off") &&
                other.collider.CompareTag("Inrun"))
            {
                Crash();
            }

            if (!Landed && other.collider.CompareTag("LandingArea"))
            {
                // Debug.Log("Landed: " + other.impulse.magnitude / Time.fixedDeltaTime);
                // if (other.relativeVelocity.magnitude > 4)
                // {
                //     Crash();
                //     Landed = true;
                // }

                if (!jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Pre-landing"))
                {
                    if (State == 3 && !deductedforlanding)
                    {
                        judgesController.PointDeduction(1, 1);
                        deductedforlanding = true;
                    }
                    else
                    {
                        judgesController.OnDistanceMeasurement(
                            (jumperModel.distCollider1.transform.position +
                             jumperModel.distCollider2.transform.position) / 2.0f);

                        Crash();
                        Landed = true;
                    }
                }
            }
        }

        private void Awake()
        {
            State = 0;
            Landed = false;

            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;

            ResetValues();
        }

        public void ResetValues()
        {
            State = 0;
            jumperModel.animator.SetBool(JumperCrash, false);
            jumperModel.animator.SetInteger(JumperState, State);
            jumperModel.animator.SetFloat(DownForce, 0f);
            Landed = false;
            rb.isKinematic = true;
            jumperModel.GetComponent<Transform>().localPosition = new Vector3();
            jumperAngle = 1;
            button0 = button1 = false;
            rSkiClone.SetActive(false);
            lSkiClone.SetActive(false);
            jumperModel.skiRight.SetActive(true);
            jumperModel.skiLeft.SetActive(true);
            deductedforlanding = false;
            judged = false;
            takeoff = false;
            goodSamples = 0;
            windThrustDelayCounter = 0;
            torqueCoef = 0.2f;
            WindThrustDeterminerTimesUsed = 0;
        }

        private bool shouldStart;
        private bool shouldNextJumper;
        [SerializeField] private int state;

        private void Update()
        {
            if (OnInrun || OnOutrun)
            {
                audioSource.mute = false;
                audioSource.pitch = Mathf.Sqrt(rb.velocity.magnitude / 20.0f);
            }
            else
            {
                audioSource.mute = true;
            }

            jumperModel.animator.SetInteger(JumperState, State);

            if (State == 1 && Input.GetMouseButtonDown(0))
            {
                Jump();
            }
            else if ((State == 2 || State == 3 || State == 4) &&
                     (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                button0 |= Input.GetMouseButtonDown(0);
                button1 |= Input.GetMouseButtonDown(1);
                //Debug.Log("Wind Thrust used " + WindThrustDeterminerTimesUsed + " times.");
                Land();

            }

            if (State == 2 && !takeoff)
            {
                jumperAngle += Time.deltaTime * Input.GetAxis("Mouse Y") * gameConfig.Config.mouseSensitivity;
                jumperAngle /= 1.05f;
                jumperAngle = Mathf.Clamp(jumperAngle, -1, 1);


                judgesController.FlightStability(jumperAngle);


                var torque = new Vector3(0.0f, 0.0f, jumperAngle * rotCoef.Value);
                rb.AddRelativeTorque(torque, ForceMode.Acceleration);

                jumperModel.animator.SetFloat(JumperAngle, jumperAngle);
            }

            if (rb.transform.position.x > judgesController.hill.U.x)
            {
                Brake();
            }
        }

        private void FixedUpdate()
        {
            var vel = rb.velocity + rb.velocity.normalized * windForce;
            //Debug.Log("rb.velocity: " + rb.velocity + " rb velocity.normalized: " + rb.velocity.normalized);

            var liftVec = new Vector3(-vel.normalized.y, vel.normalized.x, 0.0f);
            double tmp = rb.rotation.eulerAngles.z;
            if (tmp > 180) tmp -= 360;

            angle = -Mathf.Atan(rb.velocity.normalized.y / rb.velocity.normalized.x) * 180 / Mathf.PI + tmp;

            if (-15.0f <= angle && angle <= 50)
            {
                lift = 0.000933d + 0.00023314d * angle - 0.00000008201d * angle * angle -
                    0.0000001233d * angle * angle * angle + 0.00000000169d * angle * angle * angle * angle;
                drag = 0.001822d + 0.000096017d * angle + 0.00000222578d * angle * angle -
                    0.00000018944d * angle * angle * angle + 0.00000000352d * angle * angle * angle * angle;
            }


            if (takeoff)
            {
                if (jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Take-off") &&
                    jumperModel.animator.IsInTransition(0))
                {
                    takeoff = false;
                   // Debug.Log("Total samples: " + totalSamples + ", good samples: " + goodSamples);
                }

                if (OnInrun && goodSamples < totalSamples)
                {
                    var jumpDirection = rb.velocity.normalized;
                    jumpDirection = new Vector3(-jumpDirection.y, jumpDirection.x, 0);
                    rb.AddForce(jumpSpeed * jumpDirection / totalSamples / Time.fixedDeltaTime, ForceMode.Acceleration);
                    goodSamples++;
                }
            }

            if (State == 1)
            {
                rb.AddForce(-vel.normalized * (inrunDrag * vel.sqrMagnitude));
            }

            if (State == 2 && !takeoff)
            {



                if (torqueCoef < 0.5)
                {
                    torqueCoef += 0.01f;
                }

                windThrustDelayCounter += 1;
                //Debug.Log(windThrustDelayCounter);
                    rb.AddForce(-vel.normalized * ((float)drag * vel.sqrMagnitude * forceScale));
                    rb.AddForce(liftVec * ((float)lift * vel.sqrMagnitude * forceScale));
                    var torque = new Vector3(0.0f, 0.0f,
                        (90 - (float)angle) * Time.fixedDeltaTime * torqueCoef);

                    //Debug.Log("Torque: " + torque.z + " angle: " + angle + " 90 - angle: " + (90 - (float)angle));


                    rb.AddRelativeTorque(torque, ForceMode.Acceleration);


                if(windThrustDelayCounter > 80)
                    WindThrustDeterminer = Random.Range(0, 100);
                    if (WindThrustDeterminer > 96)
                    {

                        rb.AddRelativeTorque(0, 0, 15, ForceMode.Acceleration); ;
                        WindThrustDeterminerTimesUsed++;
                    }
                    // Debug.Log(WindThrustDeterminer);
                   
                
            }

            if (State == 5)
            {
                var brakeVec = Vector3.left;
                var distToEnd = judgesController.hill.U.x + 100 - rb.position.x;

                rb.AddForce(brakeVec * (rb.mass * rb.velocity.x) / 2);
            }
        }

        public void Gate()
        {
            if (State != 0) return;
            State = 1;
            OnStartEvent.Invoke();
            rb.isKinematic = false;
        }

        public void Jump()
        {
            takeoff = true;
            State = 2;
        }

        public void Land()
        {
            var anglesZ = rb.transform.rotation.eulerAngles.z;
            anglesZ = (anglesZ + 180) % 360 - 180;
            if (Landed)
            {
                rb.AddTorque(0, 0, -anglesZ * 5, ForceMode.Acceleration);
            }

            State = 3;
            landing = 1;
            jumperModel.animator.SetFloat(Landing, 1);
            if (button0 && button1)
            {
                jumperModel.animator.SetFloat(Landing, 0);
                landing = 0;
            }

            if (landing == 0)
            {
                judgesController.PointDeduction(1, 1m);
                landing = -1;
            }
        }

        public void Crash()
        {
            if (State == 3)
            {
                judgesController.PointDeduction(1, 3);
            }
            else
            {
                judgesController.PointDeduction(1, 5);
                judgesController.PointDeduction(0, 5);
            }

            //Na plecy i na brzuch
            //State = ;
            jumperModel.animator.SetBool(JumperCrash, true);
            rSkiClone.SetActive(true);
            lSkiClone.SetActive(true);
            jumperModel.skiRight.SetActive(false);
            jumperModel.skiLeft.SetActive(false);
            lSkiClone.GetComponent<Rigidbody>().velocity = rb.velocity * 0.9f;
            lSkiClone.GetComponent<Transform>().position = jumperModel.skiLeft.GetComponent<Transform>().position;
            lSkiClone.GetComponent<Transform>().rotation = jumperModel.skiLeft.GetComponent<Transform>().rotation;

            rSkiClone.GetComponent<Rigidbody>().velocity = rb.velocity * 0.9f;
            rSkiClone.GetComponent<Transform>().position = jumperModel.skiRight.GetComponent<Transform>().position;
            rSkiClone.GetComponent<Transform>().rotation = jumperModel.skiRight.GetComponent<Transform>().rotation;
            judgesController.PointDeduction(2, 7);
            if (!judged)
            {
                judgesController.Judge();
                judged = true;
            }
        }

        public void Brake()
        {
            //ToDo
            if (State != 5)
            {
                if (!judged)
                {
                    judgesController.Judge();
                    judged = true;
                }
            }

            State = 5;
        }
    }
}