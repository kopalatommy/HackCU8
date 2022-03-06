using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnnaturalSelection.Audio;
using UnnaturalSelection.Environment;

namespace UnnaturalSelection.Character
{
    public enum MotionState
    {
        Idle,
        Walking,
        Running,
        Crouched,
        Climbing,
        Flying
    }

    [RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))]
    public class MovementController : MonoBehaviour
    {
        [Header("Character Settings")]
        [SerializeField, Range(1.5f, 2.1f)]
        [Tooltip("The character’s Capsule Collider height in meters.")]
        private float characterHeight = 1.8f;

        [SerializeField, Range(0.6f, 1.4f)]
        [Tooltip("The character’s Capsule Collider diameter in meters.")]
        private float characterWidth = 0.8f;

        [SerializeField, Range(50, 100)]
        [Tooltip("The character’s mass. The movement is based on forces so the mass will affect how fast the character moves.")]
        private float characterMass = 80;

        [SerializeField]
        [Tooltip("Enables jumping functionality for the character.")]
        private bool canJump = true;

        [SerializeField]
        [Tooltip("Enables running functionality for the character.")]
        private bool canRun = true;

        [SerializeField]
        [Tooltip("Enables crouching functionality for the character.")]
        private bool canCrouch = true;

        [SerializeField, Range(0.9f, 1.3f)]
        [Tooltip("The character’s Capsule Collider height while crouching, in meters.")]
        private float crouchingHeight = 1.25f;

        [SerializeField, Range(0.001f, 2f)]
        [Tooltip("Determines how fast the character can change between standing and crouching.")]
        private float crouchSpeedModifier = 0.5f;

        [SerializeField, Range(0.05f, 0.5f)]
        [Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value.")]
        private float maxStepHeight = 0.25f;

        [SerializeField, Range(1, 90)]
        [Tooltip("Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.")]
        private float slopeLimit = 50;

        [Header("Movement")]
        [SerializeField, Range(0.1f, 10)]
        [Tooltip("Defines how much force will be applied on the character when walking.")]
        private float walkingForce = 4.25f;

        [SerializeField, Range(0.1f, 5)]
        [Tooltip("Defines how much force will be applied on the character when walking crouching.")]
        private float crouchForce = 2f;

        [SerializeField]
        [Tooltip("Defines the Running Force by multiplying the indicated value by the Walking Force. (Running Force = Walking Force * Run Multiplier).")]
        private float runMultiplier = 2.25f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines how much the character will be affected by the movement forces while flying.")]
        private float airControlPercent = 0.5f;

        [SerializeField]
        [Tooltip("Determines the force applied on the character to perform a jump.")]
        private float jumpForce = 9f;

        [SerializeField]
        [Tooltip("A gravity acceleration multiplier to control jumping and falling velocities.")]
        private float gravityIntensity = 4f;

        [SerializeField]
        [Tooltip("Determines the delay to perform a jump, in seconds.")]
        private float jumpDelay;

        [SerializeField]
        [Tooltip("The minimum fall distance to calculate the resulting damage.")]
        private float heightThreshold = 4.0f;

        [SerializeField]
        [Tooltip("Multiplies the calculated fall damage by the indicated value to increase the total damage.")]
        private float damageMultiplier = 3.0f;

        private Vector3 fallingStartPos;
        private float fallDistance;

        [SerializeField]
        [Tooltip("Allow the character to climb ladders.")]
        private bool ladder = true;

        [SerializeField, Range(0.1f, 10)]
        [Tooltip("Defines how fast the character can climb ladders.")]
        private float climbingSpeed = 3.5f;

        private bool climbing;
        private Vector3 ladderTopEdge;

        [SerializeField]
        [Tooltip("Allows the character to slide")]
        private bool sliding = true;

        [SerializeField]
        [Tooltip("Defines how much force will be applied on the character when sliding.")]
        private float slidingThrust = 12f;

        [SerializeField]
        private Vector2 slidingDistance = new Vector2(4, 7);

        [SerializeField]
        [Range(1, 90)]
        [Tooltip("Limits the collider to only slide on slopes that are less steep (in degrees) than the indicated value.")]
        private float slidingSlopeLimit = 15;

        [SerializeField]
        [Tooltip("Defines how much time the character will need to stand up after sliding.")]
        private float delayToGetUp = 1f;

        [SerializeField]
        [Tooltip("Defines whether the character will stand-up after sliding or will stay crouching.")]
        private bool standAfterSliding = true;

        [SerializeField]
        [Tooltip("Override the camera’s vertical rotation limits  (pitch).")]
        private bool overrideCameraPitchLimit = true;

        [SerializeField]
        private Vector2 slidingCameraPitch = new Vector2(-40, 60);

        private float nextSlidingTime;
        private float desiredSlidingDistance;
        private Vector3 slidingStartPosition;
        private Vector3 slidingStartDirection;

        [SerializeField]
        [Tooltip("Defines whether the weight the character is carrying will affect their movement speed.")]
        private bool weightAffectSpeed = true;

        [SerializeField]
        [Tooltip("Defines whether the weight the character is carrying will affect their jump height.")]
        private bool weightAffectJump = true;

        [SerializeField]
        [Tooltip("Allow the character to vault scene objects.")]
        private bool vault = true;

        [SerializeField]
        [Tooltip("Defines the layers that can be evaluated when checking the dimensions of the obstacle.")]
        private LayerMask affectedLayers = 1;

        [SerializeField]
        [Tooltip("Allow the character to vault big obstacles like walls.")]
        private bool allowWallJumping = true;

        [SerializeField]
        [Tooltip("This curve is used to evaluate how fast the animation should be simulated according to the wave format.")]
        private AnimationCurve vaultAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        [SerializeField, Range(0.75f, 1.75f)]
        [Tooltip("Defines the maximum height an obstacle can have for the character to be able to vault it.")]
        private float maxObstacleHeight = 1.2f;

        [SerializeField]
        private Vector2 obstacleSlope = new Vector2(70, 120);

        [SerializeField]
        [Tooltip("Defines how long the character will need to vault an obstacle.")]
        private float vaultDuration = 0.55f;

        private Vector3 obstaclePosition;

        [SerializeField]
        [Tooltip("Allow the character to emit sounds when walking through the scene.")]
        private bool footsteps = true;

        [SerializeField]
        [Tooltip("Allows the footsteps to be automatically calculated based on the character's current speed.")]
        private bool automaticallyCalculateIntervals;

        [SerializeField]
        [Tooltip("Defines the interval between the footsteps when the character is walking.")]
        private float walkingBaseInterval = 0.475f;

        [SerializeField]
        [Tooltip("Defines the interval between the footsteps when the character is running.")]
        private float runningBaseInterval = 0.12f;

        [SerializeField]
        [Tooltip("Defines the interval between the footsteps when the character is crouched.")]
        private float crouchBaseInterval = 0.7f;

        [SerializeField]
        [Tooltip("Defines the interval between the footsteps when the character is is climbing a ladder.")]
        private float climbingInterval = 0.55f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the footsteps volume when walking.")]
        private float walkingVolume = 0.05f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the footsteps volume when walking crouching.")]
        private float crouchVolume = 0.02f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the footsteps volume when running.")]
        private float runningVolume = 0.15f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the jumping volume used by the character when performing a jump.")]
        private float jumpVolume = 0.1f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the landing volume used by the character when landing on the ground.")]
        private float landingVolume = 0.1f;

        [SerializeField]
        [Tooltip("Sound played when the character goes from standing to crouching.")]
        private AudioClip standingUpSound;

        [SerializeField]
        [Tooltip("Sound played when the character goes from crouching to standing.")]
        private AudioClip crouchingDownSound;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the crouching volume played when the character goes from crouch or stand up.")]
        private float crouchingVolume = 0.2f;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the sliding sound volume when the character is sliding.")]
        private float slidingVolume = 0.2f;

        private float nextStep;

        public event Action<bool> ladderEvent;

        public event Action<float> landingEvent;

        public event Action preJumpEvent;

        public event Action jumpEvent;

        public event Action vaultEvent;

        public event Action startSlidingEvent;

        public event Action gettingUpEvent;

        private float weight;

        [SerializeField]
        [Tooltip("Defines the maximum weight the character can carry.")]
        private float maxWeight = 50;

        [SerializeField, Range(0, 1)]
        [Tooltip("Defines the maximum speed loss when carrying the maximum weight.")]
        private float maxSpeedLoss = 0.7f;

        private Rigidbody _rigidBody;
        private CapsuleCollider capsuleCollider;

        private float groundRelativeAngle;
        private Vector3 groundContactNormal;
        private Vector3 groundContactPoint;

        private SurfaceIdentifier m_Surface;
        private int triangleIndex;

        private bool jump;
        private bool previouslyGrounded;
        private bool previouslyJumping;
        private bool previouslySliding;
        private bool isJumping;
        private bool slopedSurface;
        private bool isRunning;
        private AudioEmitter m_PlayerFootstepsSource;

        public bool IsGrounded
        {
            get;
            private set;
        }

        public bool IsControllable
        {
            get;
            set;
        }

        public float Weight
        {
            get => weight;
            set => weight = weightAffectSpeed ? Mathf.Clamp(value, 0, maxWeight) : 0;
        }

        public float Radius => capsuleCollider.radius;

        public MotionState State { get; private set; } = MotionState.Idle;

        public Vector3 Velocity => _rigidBody.velocity;

        public bool IsAiming
        {
            get;
            set;
        }

        public bool LowerBodyDamaged
        {
            get;
            set;
        }

        public bool TremorTrauma
        {
            get;
            set;
        }

        public bool IsSliding
        {
            get;
            private set;
        }

        public bool IsCrouched
        {
            get;
            private set;
        }

        public bool ReadyToVault
        {
            get;
            set;
        }

        public bool CanVault
        {
            get;
            private set;
        }

        public float CurrentTargetForce
        {
            get
            {
                if (IsCrouched)
                {
                    return crouchForce;
                }

                if (isRunning)
                {
                    return WalkingForce * runMultiplier;
                }

                return State == MotionState.Climbing ? climbingSpeed : WalkingForce;
            }
        }

        private float SlidingThrust => slidingThrust * WeightFactor;

        private float WalkingForce => walkingForce * WeightFactor;

        private float WeightFactor
        {
            get
            {
                if (LowerBodyDamaged)
                {
                    return Mathf.Clamp01(1 + maxSpeedLoss - ((1 - maxSpeedLoss) * maxWeight + maxWeight * maxSpeedLoss) / maxWeight);
                }

                float factor = Mathf.Clamp01(1 + maxSpeedLoss - ((1 - maxSpeedLoss) * Weight + maxWeight * maxSpeedLoss) / maxWeight);
                return factor;
            }
        }

        private float JumpForce
        {
            get
            {
                if (!weightAffectJump)
                    return jumpForce;

                return jumpForce * WeightFactor;
            }
        }

        public bool ShouldJump
        {
            get;
            set;
        }

        public bool ShouldCrouch
        {
            get;
            set;
        }

        public bool ShouldRun { get; set; }

        public Vector3 MoveDirection
        {
            get;
            set;
        }

        public Vector2 MoveInput
        {
            get;
            set;
        }

        public Camera characterCamera;
        public bool hasCamera = false;
        public CameraController cameraController = null;

        public void OnStart()
        {
            // Init components
            _rigidBody = GetComponent<Rigidbody>();
            _rigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidBody.mass = characterMass / 8;

            capsuleCollider = GetComponent<CapsuleCollider>();

            if(hasCamera)
                cameraController.Init(transform, characterCamera.transform);

            ReadyToVault = true;

            // Events
            jumpEvent += PlayJumpSound;
            landingEvent += PlayLandingSound;

            // Controllers
            IsControllable = true;

            // Instead of calling these methods once per frame, it's more efficient to update them at a lower frequency.
            InvokeRepeating(nameof(UpdateState), 0, 0.05f);
            InvokeRepeating(nameof(CheckGroundStatus), 0, 0.05f);
            //InvokeRepeating(nameof(CheckObstaclesAhead), 0, 0.05f);

            // AudioSources
            Transform root = transform.root;
            m_PlayerFootstepsSource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterFeet", root);
        }

        public void OnUpdate()
        {
            if(hasCamera)
                cameraController.UpdateRotation(IsAiming);

            if (IsControllable)
            {
                CheckObstaclesAhead();
                HandleInput();
            }

            if (IsGrounded || State == MotionState.Climbing)
            {
                ladderEvent?.Invoke(State == MotionState.Climbing);

                if (footsteps && State != MotionState.Flying)
                    FootStepCycle();

                if (IsSliding)
                {
                    // Stand up if there is anything preventing the character to slide.
                    if (State != MotionState.Running || Vector3.Distance(transform.position, slidingStartPosition) > desiredSlidingDistance
                        || Vector3.Dot(transform.forward, slidingStartDirection) <= 0 || groundRelativeAngle > slidingSlopeLimit)
                    {
                        IsSliding = false;
                        previouslySliding = false;
                        nextSlidingTime = Time.time + delayToGetUp;
                        desiredSlidingDistance = slidingDistance.x;
                        m_PlayerFootstepsSource.Stop();
                        isRunning = standAfterSliding && isRunning;
                        IsCrouched = !standAfterSliding || PreventStandingInLowHeadroom(transform.position);

                        gettingUpEvent?.Invoke();

                        if(hasCamera && overrideCameraPitchLimit)
                            cameraController.OverrideCameraPitchLimit(false, slidingCameraPitch.x, slidingCameraPitch.y);
                    }
                    else
                    {
                        if (!previouslySliding)
                        {
                            previouslySliding = true;

                            startSlidingEvent?.Invoke();
                        }

                        ScaleCapsuleForCrouching(IsSliding, 0.9f, crouchSpeedModifier * 2);

                        if (hasCamera && overrideCameraPitchLimit)
                            cameraController.OverrideCameraPitchLimit(IsSliding, slidingCameraPitch.x, slidingCameraPitch.y);
                    }
                }
                else
                {
                    // Calculate the sliding distance based on how much the character was running.
                    desiredSlidingDistance = Mathf.Max(Mathf.MoveTowards(desiredSlidingDistance, State == MotionState.Running ? slidingDistance.y * WeightFactor
                        : slidingDistance.x, Time.deltaTime * (State == MotionState.Running ? 2 : 3)), slidingDistance.x);

                    ScaleCapsuleForCrouching(IsCrouched, crouchingHeight, crouchSpeedModifier);
                }
            }

            if (IsSliding && State != MotionState.Running)
            {
                IsSliding = false;
                m_PlayerFootstepsSource.Stop();

                capsuleCollider.height = characterHeight;
                capsuleCollider.radius = characterWidth * 0.5f;
                capsuleCollider.center = Vector3.zero;

                if (hasCamera)
                {
                    characterCamera.transform.localPosition = new Vector3(0, characterHeight * 0.4f, 0);

                    if(overrideCameraPitchLimit)
                        cameraController.OverrideCameraPitchLimit(false, slidingCameraPitch.x, slidingCameraPitch.y);
                }
            }

            if (!IsGrounded && (previouslyGrounded || climbing))
            {
                // Set falling start position.
                fallingStartPos = transform.position;
                previouslyGrounded = false;
            }

            if (IsGrounded && !previouslyGrounded)
            {
                // Calculates the fall distance.
                fallDistance = fallingStartPos.y - transform.position.y;

                if (fallDistance > heightThreshold && !slopedSurface)
                {
                    landingEvent?.Invoke(Mathf.Round(damageMultiplier * -Physics.gravity.y * (fallDistance - heightThreshold)));
                }
                else if (fallDistance >= maxStepHeight + characterHeight * 0.5f || previouslyJumping)
                {
                    landingEvent?.Invoke(0);
                }

                fallDistance = 0;
                previouslyGrounded = true;
                slopedSurface = false;

                if(hasCamera && overrideCameraPitchLimit)
                    cameraController.OverrideCameraPitchLimit(false, slidingCameraPitch.x, slidingCameraPitch.y);
            }
        }

        /// <summary>
        /// Method used to verify the actions the player wants to execute.
        /// </summary>
        private void HandleInput()
        {
            if (!jump && !IsCrouched && !LowerBodyDamaged && !climbing && !IsSliding)
            {
                // Check if there is any obstacle ahead and try to vault, otherwise just jump upwards
                if (ShouldJump && ReadyToVault && CanVault && MoveInput.y > Mathf.Epsilon && (IsGrounded || allowWallJumping))
                {
                    Transform t = transform;
                    StartCoroutine(Vault(t.forward * (obstaclePosition.z + capsuleCollider.radius * 2) +
                                         t.up * (obstaclePosition.y + (capsuleCollider.height * 0.5f + maxStepHeight) - t.position.y)));

                    vaultEvent?.Invoke();
                    ShouldJump = false;
                    isJumping = false;
                }

                if (ShouldJump && !PreventStandingInLowHeadroom(transform.position) && canJump && IsGrounded)
                {
                    if (preJumpEvent != null && State == MotionState.Idle)
                        preJumpEvent.Invoke();

                    Invoke(nameof(PerformJump), State != MotionState.Idle ? 0 : jumpDelay);
                }
            }

            if (IsGrounded || State == MotionState.Climbing)
            {
                if (canRun)
                    CheckRunning();

                if (canCrouch)
                {
                    if (ShouldCrouch && !IsCrouched && !isRunning && State != MotionState.Climbing && !IsSliding)
                    {
                        ShouldCrouch = false;
                        IsCrouched = true;
                    }
                    else
                    {
                        IsCrouched &= !isRunning && !IsSliding && State != MotionState.Climbing && !ShouldCrouch && !ShouldJump || PreventStandingInLowHeadroom(transform.position);
                    }
                }

                if (!sliding)
                    return;

                if (ShouldCrouch && State == MotionState.Running && !IsSliding && !LowerBodyDamaged
                    && nextSlidingTime < Time.time && groundRelativeAngle < slidingSlopeLimit)
                {
                    IsSliding = true;

                    Transform t = transform;
                    slidingStartPosition = t.position;
                    slidingStartDirection = t.forward;
                }
            }
        }

        private void PerformJump()
        {
            if (!isJumping)
                jump = true;
        }

        /// <summary>
        /// Casts a ray forward trying to find any obstacle in front of the character, 
        /// if found validate its dimensions to evaluate whether the character can vault or not.
        /// </summary>
        private void CheckObstaclesAhead()
        {
            if (!ReadyToVault || !vault)
            {
                CanVault = false;
                obstaclePosition = Vector3.zero;
                return;
            }

            // ReSharper disable once InlineOutVariableDeclaration
            // C# 6 compatibility.
            RaycastHit hitInfo;

            float radius = capsuleCollider.radius;
            Transform t = transform;
            Vector3 origin = t.position + Vector3.up * (capsuleCollider.height * 0.05f);
            Vector3 direction = t.TransformDirection(Vector3.forward);

            if (Physics.SphereCast(origin, radius, direction, out hitInfo, radius * 2, affectedLayers, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.attachedRigidbody)
                    return;

                // Analyze the normal vector and the obstacle surface normal
                float vertical_angle = Vector3.Angle(t.up, hitInfo.normal);
                float horizontal_angle = Vector3.Angle(-t.forward, hitInfo.normal);

                CanVault = hitInfo.collider && horizontal_angle <= 30 && vertical_angle >= obstacleSlope.x && vertical_angle <= obstacleSlope.y && ClearPath() && !PreventStandingInLowHeadroom(t.position + t.forward * (2 * capsuleCollider.radius));

                if (!CanVault)
                    return;

                Vector3 pos = t.position + new Vector3(0, characterHeight * 2, 0) + t.forward * (radius * 2);
                Physics.Raycast(new Ray(pos, transform.TransformDirection(Vector3.down)), out RaycastHit obstacleInfo, characterHeight * 2, affectedLayers, QueryTriggerInteraction.Ignore);
                obstaclePosition = new Vector3(0, obstacleInfo.point.y > transform.position.y ? obstacleInfo.point.y + maxStepHeight : hitInfo.point.y + maxStepHeight, hitInfo.distance * 1.5f);
            }
            else
            {
                CanVault = false;
            }
        }

        /// <summary>
        /// Returns true if the target obstacle has valid dimensions, false otherwise.
        /// </summary>
        private bool ClearPath()
        {
            const float DistanceEpsilon = 0.01f;

            float radius = capsuleCollider.radius;
            Transform t = transform;
            Vector3 up = t.up;
            Vector3 verticalOffset = up * ((characterHeight - maxObstacleHeight + maxStepHeight) * (!IsGrounded ? 1.5f : 1));
            Vector3 forward = t.forward;
            Vector3 forwardOffset = forward * DistanceEpsilon;
            Vector3 position = t.position;

            Vector3 bottom = position + verticalOffset + forwardOffset + forward * (radius * 2);
            Vector3 top = position + verticalOffset + up * characterHeight + forwardOffset + forward * (radius * 2);

            bool capsuleTest = Physics.CheckCapsule(bottom, top, radius - DistanceEpsilon, affectedLayers, QueryTriggerInteraction.Ignore);
            bool lineTest = Physics.Linecast(position + forwardOffset, bottom, affectedLayers, QueryTriggerInteraction.Ignore);

            return !capsuleTest && !lineTest;
        }

        /// <summary>
        /// Moves the character to the target position, vaulting the obstacle.
        /// </summary>
        /// <param name="targetPosition">The character destination position.</param>
        private IEnumerator Vault(Vector3 targetPosition)
        {
            Vector3 position = transform.position;
            Vector3 initialPos = position;
            Vector3 destination = position + targetPosition;

            IsControllable = false;
            capsuleCollider.enabled = false;

            // Make the character move to the target position.
            for (float t = 0; t <= vaultDuration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(initialPos, destination, t / vaultDuration * vaultAnimationCurve.Evaluate(t / vaultDuration));
                yield return new WaitForFixedUpdate();
            }

            capsuleCollider.enabled = true;
            IsControllable = true;
        }

        /// <summary>
        /// Prevent the character from standing up.
        /// </summary>
        private bool PreventStandingInLowHeadroom(Vector3 position)
        {
            Ray ray = new Ray(position + capsuleCollider.center + new Vector3(0, capsuleCollider.height * 0.25f), transform.TransformDirection(Vector3.up));

            return Physics.SphereCast(ray, capsuleCollider.radius * 0.75f, out _, IsCrouched ? characterHeight * 0.6f : jumpForce * 0.12f,
                Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        private void FixedUpdate()
        {
            if (climbing)
            {
                ApplyClimbingVelocityChange(MoveInput);
            }
            else
            {
                ApplyInputVelocityChange(MoveInput);
            }

            ApplyGravityAndJumping(MoveInput);
        }

        private void ApplyClimbingVelocityChange(Vector2 input)
        {
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon))
            {
                // Calculates movement direction.
                Transform t = transform;
                Vector3 up = t.up;

                bool onLadderTop = ladderTopEdge.y - t.position.y < capsuleCollider.height;

                Vector3 desiredMove = (input.y * (onLadderTop ? up * Vector3.Dot(MoveDirection + up, up) + MoveDirection
                                           : MoveDirection) + transform.right * input.x).normalized;

                desiredMove.x *= climbingSpeed;
                desiredMove.z *= climbingSpeed;
                desiredMove.y = desiredMove.y * climbingSpeed * Mathf.Abs(Mathf.Sin(Time.time * Mathf.Deg2Rad * (700 - (climbingSpeed * 100))));

                if (_rigidBody.velocity.sqrMagnitude < (climbingSpeed * climbingSpeed))
                {
                    _rigidBody.AddForce(desiredMove, ForceMode.Impulse);
                }
            }

            if (!isJumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && _rigidBody.velocity.magnitude < 1f)
            {
                _rigidBody.Sleep();
            }
        }

        private void ApplyInputVelocityChange(Vector2 input)
        {
            if (IsSliding)
            {
                if (!(_rigidBody.velocity.sqrMagnitude < (slidingThrust * slidingThrust)))
                    return;

                Vector3 slidingDir = Vector3.ProjectOnPlane(transform.forward, groundContactNormal);
                _rigidBody.AddForce(slidingDir * Mathf.Lerp(SlidingThrust, crouchForce, Vector3.Distance(transform.position, slidingStartPosition) / desiredSlidingDistance), ForceMode.Impulse);
            }
            else
            {
                if (Mathf.Abs(input.x) - Mathf.Epsilon > 0 || Mathf.Abs(input.y) - Mathf.Epsilon > 0)
                {
                    // Calculates movement direction.
                    Transform t = transform;
                    Vector3 desiredMove = t.forward * input.y + t.right * input.x;
                    desiredMove = (desiredMove.sqrMagnitude > 1) ? Vector3.ProjectOnPlane(desiredMove, groundContactNormal).normalized : Vector3.ProjectOnPlane(desiredMove, groundContactNormal);

                    desiredMove.x *= (IsGrounded ? CurrentTargetForce : CurrentTargetForce * airControlPercent);
                    desiredMove.z *= (IsGrounded ? CurrentTargetForce : CurrentTargetForce * airControlPercent);
                    desiredMove.y = desiredMove.y * (IsGrounded ? CurrentTargetForce : CurrentTargetForce * airControlPercent);

                    if (_rigidBody.velocity.sqrMagnitude < (CurrentTargetForce * CurrentTargetForce))
                    {
                        _rigidBody.AddForce(desiredMove, ForceMode.Impulse);
                    }
                }
            }
        }

        private void ApplyGravityAndJumping(Vector2 input)
        {
            if (IsGrounded || climbing)
            {
                _rigidBody.drag = 5f;

                if (jump)
                {
                    _rigidBody.drag = 0f;
                    Vector3 velocity = _rigidBody.velocity;
                    velocity = new Vector3(velocity.x, 0f, velocity.z);
                    _rigidBody.velocity = velocity;

                    Transform t = transform;
                    if (State == MotionState.Running)
                    {
                        _rigidBody.AddForce(t.forward * (JumpForce * 2.5f) + t.up * (JumpForce * 10), ForceMode.Impulse);
                    }
                    else
                    {
                        _rigidBody.AddForce(t.up * (JumpForce * 10), ForceMode.Impulse);
                    }

                    isJumping = true;

                    if (!climbing)
                        jumpEvent?.Invoke();
                }

                if (!isJumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && _rigidBody.velocity.magnitude < 1f)
                {
                    _rigidBody.Sleep();
                }
            }
            else
            {
                _rigidBody.drag = 0f;

                if (_rigidBody.velocity.magnitude < Mathf.Abs(Physics.gravity.y * characterMass / 2))
                {
                    // Special thanks to Martin Hoffmann for pointing out the gravity acceleration issue.
                    _rigidBody.AddForce(Physics.gravity * gravityIntensity, ForceMode.Acceleration);
                }

                if (previouslyGrounded && !isJumping)
                {
                    StickToGroundHelper();
                }
            }
            jump = false;
        }

        /// <summary>
        /// Defines the capsule height based on the character state.
        /// </summary>
        /// <param name="isCrouched">Is the character crouched?</param>
        /// <param name="height">The capsule target height.</param>
        /// <param name="crouchingSpeed">Defines how fast the character can crouch.</param>
        private void ScaleCapsuleForCrouching(bool isCrouched, float height, float crouchingSpeed)
        {
            if (isCrouched)
            {
                if (Mathf.Abs(capsuleCollider.height - height) > Mathf.Epsilon)
                {
                    m_PlayerFootstepsSource.ForcePlay(crouchingDownSound, crouchingVolume);
                    nextStep = crouchingDownSound.length + Time.time;
                }

                capsuleCollider.height = height;
                capsuleCollider.radius = characterWidth * 0.5f;
                capsuleCollider.center = new Vector3(0, -(characterHeight - height) / 2, 0);

                if(hasCamera)
                {
                    characterCamera.transform.localPosition = Vector3.MoveTowards(characterCamera.transform.localPosition, new Vector3(0, height - 1, 0),
                    Time.deltaTime * 5 * crouchingSpeed);
                }
            }
            else
            {
                if (Mathf.Abs(capsuleCollider.height - characterHeight) > Mathf.Epsilon)
                {
                    m_PlayerFootstepsSource.ForcePlay(standingUpSound, crouchingVolume);
                    nextStep = standingUpSound.length + Time.time;
                }

                capsuleCollider.height = characterHeight;
                capsuleCollider.radius = characterWidth * 0.5f;
                capsuleCollider.center = Vector3.zero;

                if(hasCamera)
                {
                    characterCamera.transform.localPosition = Vector3.MoveTowards(characterCamera.transform.localPosition, new Vector3(0, characterHeight * 0.4f, 0),
                    Time.deltaTime * 5 * crouchingSpeed);
                }
            }
        }

        /// <summary>
        /// Update the character state by analyzing its properties, like speed and player input.
        /// </summary>
        private void UpdateState()
        {
            bool sleeping = IsSliding ? Velocity.sqrMagnitude < crouchForce * crouchForce * 0.9f && !slopedSurface
                : Velocity.sqrMagnitude < CurrentTargetForce * CurrentTargetForce * 0.1f;

            bool idle = MoveInput == Vector2.zero || sleeping;
            bool running = isRunning;

            if (IsGrounded)
            {
                if (!running && !IsCrouched && !idle && !IsSliding)
                {
                    State = MotionState.Walking;
                    return;
                }
                if ((running && !IsCrouched && !idle) || (IsSliding && !sleeping))
                {
                    State = MotionState.Running;
                    return;
                }
                if (IsCrouched && !idle && !IsSliding)
                {
                    State = MotionState.Crouched;
                    return;
                }
                State = MotionState.Idle;
            }
            else if (climbing)
            {
                State = MotionState.Climbing;
            }
            else
            {
                if (fallingStartPos.y - transform.position.y >= maxStepHeight + capsuleCollider.height)
                    State = MotionState.Flying;
            }
        }

        /// <summary>
        /// Positions the character to keep it in contact with the ground while standing in sloped surfaces.
        /// </summary>
        private void StickToGroundHelper()
        {
            if (!Physics.SphereCast(transform.position, capsuleCollider.radius * 0.9f, Vector3.down, out RaycastHit hitInfo,
                (1 - capsuleCollider.radius) + 0.1f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                return;

            if (Mathf.Abs(groundRelativeAngle) >= slopeLimit)
            {
                _rigidBody.velocity = Vector3.ProjectOnPlane(_rigidBody.velocity, hitInfo.normal);
            }
        }

        /// <summary>
        /// Defines if the character is running by checking the player inputs.
        /// </summary>
        private void CheckRunning()
        {
            if (!LowerBodyDamaged && MoveInput.y > 0 && !IsAiming && !climbing)
            {
                isRunning = ShouldRun;
            }
            else
            {
                isRunning = false;
            }
        }

        /// <summary>
        /// Simulates the footstep sounds according to the character's current speed.
        /// </summary>
        private void FootStepCycle()
        {
            if (IsSliding)
            {
                PlaySlidingSound();
            }
            else
            {
                if (!m_Surface)
                    return;

                switch (State)
                {
                    case MotionState.Idle:
                        if (Mathf.Abs(cameraController.CurrentYaw) > cameraController.CurrentSensitivity.x * 0.5f)
                        {
                            if (automaticallyCalculateIntervals)
                                OnFootStrike(crouchVolume, (6 - crouchForce) * 0.2f);
                            else
                                OnFootStrike(crouchVolume, crouchBaseInterval * (1 + (1 - WeightFactor)));
                        }
                        break;
                    case MotionState.Walking when automaticallyCalculateIntervals:
                        OnFootStrike(walkingVolume, (11 - CurrentTargetForce) * 0.07f);
                        break;
                    case MotionState.Walking:
                        OnFootStrike(walkingVolume, walkingBaseInterval * (1 + (1 - WeightFactor)));
                        break;
                    case MotionState.Crouched when automaticallyCalculateIntervals:
                        OnFootStrike(crouchVolume, (11 - CurrentTargetForce) * 0.075f);
                        break;
                    case MotionState.Crouched:
                        OnFootStrike(crouchVolume, crouchBaseInterval);
                        break;
                    case MotionState.Running when automaticallyCalculateIntervals:
                        OnFootStrike(runningVolume, 0.15f + walkingForce / CurrentTargetForce * 0.25f);
                        break;
                    case MotionState.Running:
                        OnFootStrike(runningVolume, runningBaseInterval * (1 + (1 - WeightFactor)) + walkingForce / CurrentTargetForce * (walkingBaseInterval - runningBaseInterval));
                        break;
                    case MotionState.Climbing when automaticallyCalculateIntervals:
                        OnFootStrike(walkingVolume, (11 - climbingSpeed) * 0.07f);
                        break;
                    case MotionState.Climbing:
                        OnFootStrike(walkingVolume, climbingInterval * (1 + (1 - WeightFactor)));
                        break;
                    case MotionState.Flying:
                        break;
                }
            }
        }

        /// <summary>
        /// Play the character's sliding sound.
        /// </summary>
        private void PlaySlidingSound()
        {
            if (!footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(groundContactPoint, triangleIndex);

            if (!surfaceType)
                return;

            AudioClip sliding = surfaceType.GetRandomSlidingSound();
            m_PlayerFootstepsSource.Play(sliding, slidingVolume);
        }

        /// <summary>
        /// Play the character's jump sound.
        /// </summary>
        private void PlayJumpSound()
        {
            if (!footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(groundContactPoint, triangleIndex);
            if (!surfaceType)
                return;

            AudioClip jump = surfaceType.GetRandomJumpSound();

            m_PlayerFootstepsSource.ForcePlay(jump, jumpVolume);
        }

        /// <summary>
        /// Play the character's landing sound based on the surface its standing on.
        /// </summary>
        private void PlayLandingSound(float fallDamage)
        {
            if (!footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(groundContactPoint, triangleIndex);
            if (!surfaceType)
                return;

            AudioClip land = surfaceType.GetRandomLandingSound();
            AudioManager.Instance.PlayClipAtPoint(land, groundContactPoint, 3, 8, landingVolume, 0);
        }

        private void OnFootStrike(float volume, float stepLength)
        {
            if (nextStep > Time.time)
                return;

            nextStep = stepLength + Time.time;
            SurfaceType surfaceType = m_Surface.GetSurfaceType(groundContactPoint, triangleIndex);

            if (State == MotionState.Climbing)
            {
                if (!(MoveInput == Vector2.zero || Velocity.sqrMagnitude < CurrentTargetForce * CurrentTargetForce * 0.05f))
                {
                    if (surfaceType)
                    {
                        AudioClip footStep = surfaceType.GetRandomWalkingFootsteps();
                        m_PlayerFootstepsSource.Play(footStep, volume);
                    }
                }
            }
            else
            {
                if (surfaceType)
                {
                    AudioClip footStep;
                    if (State == MotionState.Running)
                    {
                        footStep = surfaceType.GetRandomSprintingFootsteps();
                        //m_PlayerFootstepsSource.Play(footStep, volume);

                        // Better sound fidelity
                        AudioManager.Instance.PlayClipAtPoint(footStep, groundContactPoint, 3, 8, volume, 0);
                    }
                    else
                    {
                        footStep = surfaceType.GetRandomWalkingFootsteps();
                        m_PlayerFootstepsSource.Play(footStep, volume);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the angle between the character foot and the ground normal.
        /// </summary>
        /// <param name="ignoreEnvironment">Should ignore the environment around the character?</param>
        /// <returns>The current ground relative angle.</returns>
        private float CalculateGroundRelativeAngle(bool ignoreEnvironment)
        {
            Vector3 normal = Vector3.up;

            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, capsuleCollider.height + maxStepHeight, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                normal = hitInfo.normal;
            }

            if (ignoreEnvironment)
                return Vector3.Angle(normal, Vector3.up);

            Vector3 position = transform.position;
            Vector3 footPos = new Vector3(position.x, position.y - (characterHeight * 0.5f - maxStepHeight), position.z);
            if (Physics.Raycast(footPos, transform.TransformDirection(Vector3.forward), characterHeight * 2, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                return Vector3.Angle(normal, Vector3.up);
            }

            return -Vector3.Angle(normal, Vector3.up);

        }

        /// <summary>
        /// Defines if the character is grounded by casting a ray downwards to check whether the character 
        /// is touching a collider bellow its feet.
        /// </summary>
        private void CheckGroundStatus()
        {
            previouslyGrounded = climbing ? previouslyGrounded : IsGrounded;
            previouslyJumping = isJumping;
            slopedSurface = Mathf.Abs(groundRelativeAngle) > slopeLimit;
            float offset = (1 - capsuleCollider.radius) + (slopedSurface ? 0.05f : maxStepHeight);

            if (Physics.SphereCast(transform.position, capsuleCollider.radius * 0.9f, -transform.up, out RaycastHit hitInfo, offset, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                groundContactNormal = hitInfo.normal;
                groundContactPoint = hitInfo.point;
                groundRelativeAngle = CalculateGroundRelativeAngle(false);
                IsGrounded = Mathf.Abs(groundRelativeAngle) < slopeLimit;

                triangleIndex = hitInfo.triangleIndex;
                m_Surface = hitInfo.collider.GetSurface();
            }
            else
            {
                IsGrounded = false;
                IsCrouched = false;
                groundContactNormal = Vector3.up;
            }
            if (!previouslyGrounded && IsGrounded && isJumping)
            {
                isJumping = false;
            }
        }

        public void ClimbingLadder(bool climbing, Vector3 topEdge, SurfaceIdentifier surfaceIdentifier)
        {
            if (ladder)
            {
                ladderTopEdge = topEdge;
                this.climbing = climbing;

                if (State == MotionState.Climbing)
                    m_Surface = surfaceIdentifier;
            }
            else
            {
                this.climbing = false;
            }
        }
    }
}
