using System;
using System.Diagnostics;
using System.Collections;
using OpenSkiJumping.Competition;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Competition.Runtime;
using OpenSkiJumping.Data;
using OpenSkiJumping.Jumping;
using OpenSkiJumping.Scripts2025;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using OpenSkiJumping.UI;
using UnityEngine.Animations.Rigging;



namespace OpenSkiJumping.New
{
    public class JumperController2 : MonoBehaviour
    {
        [Space][Header("Flight")] public double angle;
        [SerializeField] private AudioSource audioSource;
        public AudioSource src;
        public AudioClip skisSound, landingSound;
        [SerializeField] Rig rig;
        [SerializeField] private float brakeForce;
        [SerializeField] private float inrunDrag = 0.0011f;
        private int windThrustDelayCounter = 0;
        [SerializeField] private int crashAngle;
        private int determineCrashAtAngle = 0;
        [SerializeField] private SkiJumperDataController skiJumperDataController;
        [SerializeField] private HillsRuntime hillsRepository;
        [SerializeField] private int WindThrustDelayCap = 80;
        public CompetitionRunner competitionRunner;
        private float torqueCoef = 0f;
        [SerializeField] private int WindThrustDeterminer;
        [SerializeField] private int WindThrustDeterminerTimesUsed;
        [SerializeField] private float fixedUpdateTorqueReference;
        public GameObject continueButton;
        private float forceScaleModifier;
        private float windModifier;

        //                                                                                          BUG Z ROTACJĄ LEŻY W WINDTHRUSTDETERMINERTIMESUSED.

        bool button0, button1;
        public bool willFall;
        private float landingVertDistance;
        public bool desperateLanding;
        private bool deductedforlanding;
        public float dirChange;
        public double drag = 0.001d;
        public float forceChange;
        private int goodSamples;
        public double angleDebug;

        private bool judged;

        public JudgesController judgesController;
        public WindGatePanel windGatePanel;
        public GameplayExtension gameplayExtension;
        public RuntimeJumpData jumpData;
        public float jumperAngle;

        public JumperModel jumperModel;


        [Space][Header("Parameters")] public float jumpSpeed;
        [SerializeField] private float forceScale;
        public float startForceScale;

        private int landing;
        private float initiateStruggleLanding = 0;
        private float struggleToCrash = 0;
        public double lift = 0.001d;
        [SerializeField] private GameConfigRuntime gameConfig;
        [SerializeField] private LayerMask landingLayerMask;

        public UnityEvent OnStartEvent;
        public UnityEvent BugOccured;
        public UnityEvent CPUJumpPerformed;
        public UnityEvent OnRoundCompleted;
        private Rigidbody rb;
        public FloatVariable rotCoef;
        public Collider landingAreaCollider;
        public GameObject rSkiClone, lSkiClone;

        public float sensCoef = 0.01f;
        public float smoothCoef = 0.01f;


        private bool takeoff;
        public int totalSamples;

        public int skillForPresentHill;
        private float hillSize;


        // public float mouseSensitivity = 2f; 
        [Space][Header("Wind")] public Vector2 windDir;
        public float windForce;
        private static readonly int JumperCrash = Animator.StringToHash("JumperCrash");
        private static readonly int JumperState = Animator.StringToHash("JumperState");
        private static readonly int DownForce = Animator.StringToHash("DownForce");
        private static readonly int JumperAngle = Animator.StringToHash("JumperAngle");
        private static readonly int Landing = Animator.StringToHash("Landing");
        private static readonly int InitiateStruggleLanding = Animator.StringToHash("InitiateStruggleLanding");
        private static readonly int StruggleToCrash = Animator.StringToHash("StruggleToCrash");
        private static readonly int Flight = Animator.StringToHash("Flight");
        private static readonly int LeftRot = Animator.StringToHash("LeftRot");
        private static readonly int RightRot = Animator.StringToHash("RightRot");

        private float tiltTimer = 0f;
        private float nextTiltDelay = 0f;
        private bool tilting = false;


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
                // UnityEngine.Debug.Log("Teraz pokazuję jumpData.distance od OnTriggerEnter: " + jumpData.Distance);


                ReducePointsForShorterJump();

                ProcessLanding();




                if (!jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Pre-landing"))
                {
                    Crash();
                }

                Landed = true;
                State = 4;
                //UnityEngine.Debug.Log("A teraz Landing jest state 4");

            }
        }




        private void ReducePointsForShorterJump()
        {
            decimal distanceAwayFromHS = 0;
            decimal pointsToDeduct = 0;

            if ((float)jumpData.Distance < hillSize)
            {
                distanceAwayFromHS = ((decimal)hillSize - jumpData.Distance) / (decimal)hillSize;
                pointsToDeduct = Math.Round(distanceAwayFromHS * 25, MidpointRounding.AwayFromZero) / 2;
                judgesController.PointDeduction(1, pointsToDeduct);
            }
            /*if ((float)jumpData.Distance < hillSize * 0.95)
            {
                judgesController.PointDeduction(1, 0.5m);
               // UnityEngine.Debug.Log("Redukcja punktów za krótki skok 0.95");
            }
            if ((float)jumpData.Distance < hillSize * 0.85)
            {
                judgesController.PointDeduction(1, 0.5m);
               // UnityEngine.Debug.Log("Redukcja punktów za krótki skok 0.85");
            }
            if ((float)jumpData.Distance < hillSize * 0.75)
            {
                judgesController.PointDeduction(1, 1);
                //UnityEngine.Debug.Log("Redukcja punktów za krótki skok 0.75");
            }*/
        }

        private void ProcessLanding()
        {

            if ((float)jumpData.Distance >= hillSize)
            {
                windGatePanel.JumpOverHSPerformed();
                float ofHS = ((float)jumpData.Distance / hillSize);
                //UnityEngine.Debug.Log("ofHS: " + ofHS);

                if (landing == 1)//jumper attempted telemark
                {
                    if (ofHS > 1.06f)
                    {
                        Crash();
                    }
                    else
                    {
                        float saveFromStruggle = Random.Range(1, 1.03f);
                        float saveFromFall = Random.Range(1.03f, 1.06f);
                        // UnityEngine.Debug.Log("saveFromStruggle: " + saveFromStruggle + " saveFromFall: " + saveFromFall);

                        if (ofHS > saveFromStruggle)
                        {
                            jumperModel.animator.SetFloat(InitiateStruggleLanding, 1);
                            initiateStruggleLanding = 1;
                            if (saveFromFall > ofHS)
                            {
                                judgesController.PointDeduction(1, 2);
                                struggleToCrash = 0;
                            }
                            else
                            {
                                struggleToCrash = 1;
                                jumperModel.animator.SetFloat(StruggleToCrash, 1);
                            }
                        }
                        /*  if(ofHS > saveFromFall && initiateStruggleLanding == 1)
                          {
                              struggleToCrash = 1;
                              judgesController.PointDeduction(1, 2);
                          }*/
                    }

                    /* jumperModel.animator.SetFloat(InitiateStruggleLanding, 1);
                     initiateStruggleLanding = 1;
                     //UnityEngine.Debug.Log("Teraz dałem InitStruggleLanding na 1");
                     struggleToCrash = 1;
                     judgesController.PointDeduction(1, 2);*/
                }



                if (landing == -1)//if the jumper attempted to land on two legs
                {
                    if (ofHS > 1.08f)
                    {
                        Crash();
                    }
                    else
                    {

                        float saveFromStruggle = Random.Range(1, 1.05f);
                        float saveFromFall = Random.Range(1.05f, 1.08f);
                        UnityEngine.Debug.Log("saveFromStruggle: " + saveFromStruggle + " saveFromFall: " + saveFromFall);

                        if (ofHS > saveFromStruggle)
                        {
                            jumperModel.animator.SetFloat(InitiateStruggleLanding, 2);
                            initiateStruggleLanding = 1;
                            if (saveFromFall > ofHS)
                            {
                                judgesController.PointDeduction(1, 2);
                                struggleToCrash = 0;
                            }
                            else
                            {
                                struggleToCrash = 2;
                                jumperModel.animator.SetFloat(StruggleToCrash, 2);
                            }
                        }


                    }




                }

                /*

                if (landing == -1)//if the jumper attempted to land on twolegs
                {
                    if (ofHS > 1.1f)
                    {
                        Crash();
                    }
                    else
                    {
                        float saveFromFalling = Random.Range(1, 1.1f);
                        UnityEngine.Debug.Log("saveFromFalling: " + saveFromFalling);


                    }
                    jumperModel.animator.SetFloat(InitiateStruggleLanding, 2);
                    initiateStruggleLanding = 2;
                    // UnityEngine.Debug.Log("Teraz dałem InitStruggleLanding na 2");
                    judgesController.PointDeduction(1, 3);

                }

                */





                /*   if (landing == 1)//jumper attempted telemark
                   {

                       jumperModel.animator.SetFloat(InitiateStruggleLanding, 1);
                       initiateStruggleLanding = 1;
                       //UnityEngine.Debug.Log("Teraz dałem InitStruggleLanding na 1");
                       struggleToCrash = 1;
                       judgesController.PointDeduction(1, 2);
                   }
                   if (landing == -1)//if the jumper attempted to land on twolegs
                   {
                       jumperModel.animator.SetFloat(InitiateStruggleLanding, 2);
                       initiateStruggleLanding = 2;
                      // UnityEngine.Debug.Log("Teraz dałem InitStruggleLanding na 2");
                       judgesController.PointDeduction(1, 3);
                   }*/
            }
            else if (landing != -1)
            {
                //Telemark finish//
                jumperModel.animator.SetFloat(Landing, 2f);
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
                // UnityEngine.Debug.Log("Teraz jestem na OnTriggerExit i LandingArea");
                OnOutrun = false;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (State >= 2 && !jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Take-off") &&
                other.collider.CompareTag("Inrun"))
            {
                BugOccured.Invoke();
                //Crash();
            }

            if (!Landed && other.collider.CompareTag("LandingArea"))
            {
                // Debug.Log("Landed: " + other.impulse.magnitude / Time.fixedDeltaTime);
                // if (other.relativeVelocity.magnitude > 4)
                // {
                //     Crash();
                //     Landed = true;
                // }
                src.clip = landingSound;
                src.PlayOneShot(src.clip);

                LandingAngleHandler();

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

        private void LandingAngleHandler()
        {
            //UnityEngine.Debug.Log("determineCrashatAngle: " + determineCrashAtAngle);
            if (determineCrashAtAngle == 0)
            {
                if (angle < crashAngle)
                {
                    determineCrashAtAngle = 1;
                }
                else
                {
                    determineCrashAtAngle = 2;
                }
            }
            //  UnityEngine.Debug.Log("Angle on landing: " + angle);
            if (angle > 30 && determineCrashAtAngle == 1)//usunięcie slide bugu
            {


                rb.AddRelativeTorque(0, 0, (int)(angle * -0.5), ForceMode.Acceleration);

            }

            if (determineCrashAtAngle == 2)
            {
                judgesController.OnDistanceMeasurement(
    (jumperModel.distCollider1.transform.position +
     jumperModel.distCollider2.transform.position) / 2.0f);

                Crash();
                Landed = true;
            }
        }

        private void Awake()
        {
            State = 0;
            Landed = false;
            startForceScale = forceScale;


            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;


            ResetValues();

        }



        public void AdjustForceScaleToHillSize()
        {
            float modifier = 1;
            float hS = hillSize = competitionRunner.GetHS();
            if (hS > 130)
            {
                modifier += (hS - 130) / 10;
                startForceScale += (hS - 130) * (0.00013f * modifier);
            }
            if (hS < 130)
            {
                modifier += (130 - hS) / 10;
                startForceScale -= (130 - hS) * (0.00062f * modifier);
            }
            if (hS > 250)
            {
                startForceScale -= (hS - 250) * (0.00007f * modifier);
            }
        }


        public void OffsetGateChange()

        {
            forceScale -= forceScaleModifier;
        }

        public void ResetValues()
        {
            State = 0;
            jumperModel.animator.SetBool(JumperCrash, false);
            jumperModel.animator.SetInteger(JumperState, State);
            jumperModel.animator.SetFloat(DownForce, 0f);
            jumperModel.animator.SetFloat(InitiateStruggleLanding, 0f);
            jumperModel.animator.SetFloat(StruggleToCrash, 0f);
            initiateStruggleLanding = 0;
            struggleToCrash = 0;
            willFall = false;
            Landed = false;
            rb.isKinematic = true;
            jumperModel.GetComponent<Transform>().localPosition = new Vector3();
            jumperAngle = 1;
            // inrunDrag = 0.0011f;
            button0 = button1 = false;
            rSkiClone.SetActive(false);
            lSkiClone.SetActive(false);
            jumperModel.skiRight.SetActive(true);
            jumperModel.skiLeft.SetActive(true);
            deductedforlanding = false;
            judged = false;
            takeoff = false;
            desperateLanding = false;
            goodSamples = 0;
            determineCrashAtAngle = 0;
            windThrustDelayCounter = 0;
            torqueCoef = 0f;
            forceScale += forceScaleModifier;
            WindThrustDeterminerTimesUsed = 0;
            WindThrustDeterminer = 0;
            rig.weight = 0;
            landingVertDistance = 0;
            windModifier = 1;
            tilting = false;
        }

        private bool shouldStart;
        private bool shouldNextJumper;
        [SerializeField] private int state;

        private void Update()
        {



            if (skiJumperDataController.GetControl() == 1 && competitionRunner.permitCPUJumps == true)
            {
                CPUJumpPerformed.Invoke();
                // UnityEngine.Debug.Log("Od JumperController2 Update CPUJumpPerformed.Invoke();");
            }


            if (state == 2 && !takeoff && Random.value < (float)((0.0001)*gameplayExtension.turbulenceChance))
            {
                tiltTimer += Time.deltaTime;
                
                AnimatorStateInfo stateInfo = jumperModel.animator.GetCurrentAnimatorStateInfo(0);
                bool inFlight = stateInfo.shortNameHash == Flight;
                bool inTransition = jumperModel.animator.IsInTransition(0);

                if (!tilting && inFlight && !inTransition && tiltTimer >= nextTiltDelay)
                {
                    tilting = true;
                    tiltTimer = 0f;

                  //  bool startTilt = Random.value < 0.5f;
                    
                    float returnTime = Random.Range(0.3f, 0.5f); // How long to stay tilted
                    /*
                    jumperModel.animator.SetFloat("TiltReturnTime", returnTime);
                    */

                    if (Random.value < 0.5f)
                    {
                        if (Random.value < 0.5f)
                        {
                            jumperModel.animator.SetTrigger("TiltLeft");
                            jumperModel.animator.SetBool("Return", true);
                            UnityEngine.Debug.Log("Turbulence Tiltleft");
                        }
                        else
                        {
                            jumperModel.animator.SetTrigger("TiltRight");
                            jumperModel.animator.SetBool("Return", true);
                            UnityEngine.Debug.Log("Turbulence TiltRight");
                        }
                    }
                    else
                    {
                        if (Random.value < 0.5f) 
                        {
                            jumperModel.animator.SetTrigger("LEscape");
                            jumperModel.animator.SetBool("Return", true);
                            UnityEngine.Debug.Log("Turbulence LEscape");
                        }
                        else
                        {
                            jumperModel.animator.SetTrigger("REscape");
                            jumperModel.animator.SetBool("Return", true);
                            UnityEngine.Debug.Log("Turbulence REscape");
                        }

                    }



                        StartCoroutine(ResetTiltingFlag(returnTime));

                  //  nextTiltDelay = Random.Range(0.1f, 0.2f);
                }
            }





            if (Input.GetKeyDown(KeyCode.F))
            {
                //jumperModel.animator.SetLayerWeight(jumperModel.animator.GetLayerIndex("LeftRot"), 1f);
                jumperModel.animator.SetTrigger("TiltLeft");
                UnityEngine.Debug.Log("jumperModel.animator.SetTrigger(\"TiltLeft\");");
            }
            if (Input.GetKeyDown(KeyCode.G))
            {
                // jumperModel.animator.SetLayerWeight(jumperModel.animator.GetLayerIndex("LeftRot"), 0f);
                jumperModel.animator.SetTrigger("TiltRight");
                UnityEngine.Debug.Log("jumperModel.animator.SetTrigger(\"TiltRight\");");
            }

            if (OnInrun || OnOutrun)
            {
                audioSource.mute = false;
                audioSource.pitch = Mathf.Sqrt(rb.velocity.magnitude / 20.0f);
            }
            else
            {
                audioSource.mute = true;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                UnityEngine.Debug.Log("Klikam S.");
                CPUJumpPerformed.Invoke();
            }
            /*
            if (Input.GetKeyDown(KeyCode.D))
            {
                jumperModel.footRight.transform.localPosition = new Vector3(0,0,0.2f) ;
                jumperModel.skiRight.transform.localPosition = new Vector3(0, 0, 0.2f);
            }*/

            jumperModel.animator.SetInteger(JumperState, State);
            //UnityEngine.Debug.Log( "State: " + state + " struggleToCrash: " + struggleToCrash);
            if (State == 1 && Input.GetMouseButtonDown(0))
            {
                Jump();
            }
            else if ((State == 2 || State == 3 || State == 4) &&
                     (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {

                tilting = false;
                StopAllCoroutines();

                button0 |= Input.GetMouseButtonDown(0);
                button1 |= Input.GetMouseButtonDown(1);
                rig.weight = 0f;

                landingVertDistance = RayCast();

                if (landingVertDistance < 1.5f && landingVertDistance != 0)
                {
                    UnityEngine.Debug.Log("Adding force because RayCast. DesperateLanding. LandingVertDistance:" + landingVertDistance);
                    rb.AddRelativeTorque(0, 0, (int)(angle * -10), ForceMode.Acceleration);
                    rb.AddForce(transform.up * 100f);
                    desperateLanding = true;
                }


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

            //pomysł taki, żeby się wycrashował po ciężkim lądowaniu
            if (struggleToCrash != 0)
            {
                if (jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("StruggleToCrash"))
                {
                    Crash();
                }
            }
        }

        private IEnumerator ReturnToFlight(float delay, float blendBackTime)
        {
            yield return new WaitForSeconds(delay);

            jumperModel.animator.CrossFade(Flight, blendBackTime, 0);
            tilting = false;
        }
        public float RayCast()
        {
            Ray ray = new Ray(jumperModel.skiLeft.transform.position, Vector3.down);
            RaycastHit hit;
            float verticalDistance = 0;

            if (Physics.Raycast(ray, out hit, 100f, landingLayerMask))
            {
                verticalDistance = hit.distance;
                UnityEngine.Debug.Log("Vertical distance to landing area: " + verticalDistance);
            }
            else
            {
                UnityEngine.Debug.Log("No landing area hit");
            }

            return verticalDistance;
        }

        public void CheckCurrentJumperControl()
        {
            UnityEngine.Debug.Log($"Po kliknieciu miedzy rundami Current jumper control: {skiJumperDataController.GetControl()}");
        }

        public void ResumeCPUJumps()
        {
            if (skiJumperDataController.GetControl() == 1 && competitionRunner.permitCPUJumps == true)
            {

                //Kod ponizej przeniesiony z Update()

                CPUJumpPerformed.Invoke();
                UnityEngine.Debug.Log("Od JumperController2 ResumeCPUJumps() CPUJumpPerformed.Invoke();");
                /*if (competitionRunner.jumperCounterReached)
                {
                    competitionRunner.jumperCounter = 0;
                    OnRoundCompleted.Invoke();
                }
                else
                {
                    CPUJumpPerformed.Invoke();
                }*/
            }
        }

        IEnumerator ResetTiltingFlag(float delay)
        {
            yield return new WaitForSeconds(delay);
            tilting = false;
        }
        private float GetWindMultiplier()
        {
            float minRearWindMultiplier = 1.8f;
            UnityEngine.Debug.Log("GetWindMultiplier()");
            if (windForce > 0)
            {
                UnityEngine.Debug.Log("GetWindMultiplier() windForce > 0");
                return (0.8f + windForce * (hillSize * 0.0005f));
            }
            else if (windForce < 0 && windForce > -2f)
            {
                // Clamp windForce to the range [0, -2]
                float clampedWind = Mathf.Clamp(windForce, -2f, 0f);

                // Compute t such that t is 0 when windForce is 0 and 1 when windForce is -2
                float t = Mathf.InverseLerp(0f, -2f, clampedWind);

                // Interpolate the base multiplier (here using 2.8f as your starting value) 
                // and add the dynamic hillSize modifier:
                float multiplier = Mathf.Lerp(3f, minRearWindMultiplier, t) + (-windForce) * (hillSize * GetHillMultiplier(hillSize));
                UnityEngine.Debug.Log("GetWindMultiplier() windForce < 0 && windForce > -2f");
                return multiplier;
            }
            else if (windForce <= -2f)
            {
                UnityEngine.Debug.Log("GetWindMultiplier() windForce <= -2f");
                return (minRearWindMultiplier + (-windForce) * (hillSize * GetHillMultiplier(hillSize)));
            }
            else
            {
                UnityEngine.Debug.Log("GetWindMultiplier() else");
                return 1f;
            }
        }

        private float GetHillMultiplier(float hillSize)
        {
            // For hillSize <= 90, use 0.0001
            if (hillSize <= 90f)
            {
                return 0.000f;
            }
            // For hillSize between 90 and 130, interpolate between 0.0001 and 0.0003
            else if (hillSize <= 150f)
            {
                return Mathf.Lerp(0.0000f, 0.0006f, (hillSize - 90f) / (150f - 90f));
            }
            // For hillSize between 130 and 250, interpolate between 0.0003 and 0.006
            else if (hillSize <= 250f)
            {
                return Mathf.Lerp(0.0006f, 0.018f, (hillSize - 130f) / (250f - 130f));
            }
            else
            {
                // For sizes above 250, clamp to 0.006 or extrapolate if desired
                return 0.018f;
            }
        }

        private void FixedUpdate()
        {
            //Test windforce

            var vel = new Vector3();



            vel = rb.velocity + rb.velocity.normalized * windForce * windModifier;


            //var vel = rb.velocity + rb.velocity.normalized * windForce;
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

            if (angle < -120 && angle > -130)
            {

                BugOccured.Invoke();
                // angleDebug = angle;
            }

            if (takeoff)
            {
                if (jumperModel.animator.GetCurrentAnimatorStateInfo(0).IsName("Take-off") &&
                    jumperModel.animator.IsInTransition(0))
                {
                    takeoff = false;
                    UnityEngine.Debug.Log("Total samples: " + totalSamples + ", good samples: " + goodSamples);
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

                // Torquecoef to zmienna która determinuje ile skoczek dostanie rotacji w tył w każdej klatce. Wartość początkowa to 0 po to, żeby skoczka zaraz na progu tak nie odchylało do tyłu,
                // Potem wartość ta rośnie trochę każdą klatką.

                if (torqueCoef < 0.5)
                {
                    torqueCoef += 0.005f;
                }


                windThrustDelayCounter += 1;

                //Debug.Log(windThrustDelayCounter);
                rb.AddForce(-vel.normalized * ((float)drag * vel.sqrMagnitude * forceScale));
                rb.AddForce(liftVec * ((float)lift * vel.sqrMagnitude * forceScale));
                var torque = new Vector3(0.0f, 0.0f,
                    (90 - (float)angle) * Time.fixedDeltaTime * torqueCoef);
                fixedUpdateTorqueReference = (90 - ((float)angle)) * Time.fixedDeltaTime * torqueCoef;

                //UnityEngine.Debug.Log("Torque: " + torque.z + " angle: " + angle + " 90 - angle: " + (90 - (float)angle));


                rb.AddRelativeTorque(torque, ForceMode.Acceleration);


                if (windThrustDelayCounter > WindThrustDelayCap)
                    //   UnityEngine.Debug.Log("Wind thrust delay counter: " + windThrustDelayCounter + "torqueCoef" + torqueCoef);
                    WindThrustDeterminer = Random.Range(0, 100);
                if (WindThrustDeterminer > 96)
                {

                    rb.AddRelativeTorque(0, 0, 15, ForceMode.Acceleration); ;
                    WindThrustDeterminerTimesUsed++;
                }
                //Debug.Log(WindThrustDeterminer);


            }

            if (State == 5)
            {
                var brakeVec = Vector3.left;
                var distToEnd = judgesController.hill.U.x + 100 - rb.position.x;

                rb.AddForce(brakeVec * (rb.mass * rb.velocity.x) / 2);
            }
        }

        public void AssignNewJumpersSkill()
        {
            hillSize = competitionRunner.GetHS();
            skillForPresentHill = skiJumperDataController.GetSkill(hillSize);
            jumpData.JumperSkill = skillForPresentHill;

        }

        public void Gate()
        {

            if (State != 0) return;
            forceScale = startForceScale;
            // inrunDrag += gameplayExtension.inrunDragModifier(skillForPresentHill);
            forceScaleModifier = gameplayExtension.forceScaleModifier(skillForPresentHill);
            forceScale -= forceScaleModifier;
            State = 1;
            rig.weight = 0f;
            windModifier = 1f;
            windModifier = GetWindMultiplier();
            OnStartEvent.Invoke();
            UnityEngine.Debug.Log("windModifier: " + windModifier + " skillforpresenthill: " + skillForPresentHill + " force at gate: " + forceScale);
            //UnityEngine.Debug.Log("jumpdata.Gate: " + jumpData.Gate + "jumpdata.Wind " + jumpData.Wind);
            rb.isKinematic = false;

        }

        public void Jump()
        {
            takeoff = true;
            State = 2;
            if (windGatePanel.windSlider.value > 0)
            {

                rig.weight = (windGatePanel.windSlider.value / 3f);
                if (rig.weight > 1)
                {
                    rig.weight = 1;
                }
                UnityEngine.Debug.Log($"rig.weight during flight: {rig.weight}");
            }

        }

        public void Land()
        {

            // UnityEngine.Debug.Log("A teraz executowana jest funkcja Land()");
            var anglesZ = rb.transform.rotation.eulerAngles.z;
            anglesZ = (anglesZ + 180) % 360 - 180;
            if (Landed)
            {
                rb.AddTorque(0, 0, -anglesZ * 5, ForceMode.Acceleration);
            }

            State = 3;


            landing = 1;

            if (desperateLanding)
            {
                judgesController.PointDeduction(1, 1);
            }

            jumperModel.animator.SetFloat(Landing, 1);
            if (button0 && button1)
            {
                jumperModel.animator.SetFloat(Landing, 0);
                landing = 0;
            }

            if (landing == 0)
            {
                judgesController.PointDeduction(1, 1);
                landing = -1;
            }

        }

        public void Crash()
        {
            if (State == 3)
            {
                judgesController.PointDeduction(1, 5);
            }
            else
            {
                judgesController.PointDeduction(1, 5);
                judgesController.PointDeduction(0, 5);
            }


            /*
                         if (State == 3)
            {
                judgesController.PointDeduction(1, 3);
            }
            else
            {
                judgesController.PointDeduction(1, 5);
                judgesController.PointDeduction(0, 5);
            }
*/

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

            /*if (struggleToCrash != 0)
            {
                lSkiClone.GetComponent<Rigidbody>().velocity = rb.velocity * 5f;
                lSkiClone.GetComponent<Transform>().position += new Vector3(4, 4);          
            }*/

            if (struggleToCrash == 0)
            {
                judgesController.PointDeduction(2, 4);
            }

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
