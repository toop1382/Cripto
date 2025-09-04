using UnityEngine;
using VContainer;

namespace Cripto.Game.Gameplay.Character
{
    /*
     * TopDownCharacterController
     * - Attach to your character GameObject.
     * - Optional components supported:
     *      - Rigidbody (3D) for physics-based MovePosition
     *      - Rigidbody2D for 2D MovePosition
     *      - CharacterController for SimpleMove
     *   If none exist, falls back to transform.Translate.
     * - Animator (optional):
     *      Float "Speed" (movement magnitude)
     *      Bool  "IsMoving"
     *      Float "MoveX" and "MoveY" (for 2D blend trees)
     * - Joystick (optional): assign SimpleJoystick. If null, keyboard (WASD/Arrows) are used as fallback.
     */
    [DisallowMultipleComponent]
    public class TopDownCharacterController : MonoBehaviour
    {
        public bool keyboardFallback = true; // Editor/Standalone fallback when no joystick

        [Header("Movement")] public float moveSpeed = 3.5f; // units/sec

        [Tooltip("If true, character faces movement direction by rotating on Y (3D)")]
        public bool faceMoveDirection = true;

        [Tooltip("If true, uses 2D facing (flip X) when a SpriteRenderer exists")]
        public bool flipSpriteWithX = true;

        [Header("Animation")] public Animator animator; // Optional
        public string speedParam = "Speed";
        

        private Rigidbody _rb3D;
        private CharacterController _cc;
        private SpriteRenderer _sprite;

        // Animator hashes (if parameters exist)
        private int _speedHash = -1;

        [Inject] private Joystick joystick;

        void Awake()
        {
            _rb3D = GetComponent<Rigidbody>();
            _cc = GetComponent<CharacterController>();
            _sprite = GetComponentInChildren<SpriteRenderer>();

            if (animator == null) animator = GetComponentInChildren<Animator>();
            CacheAnimatorHashes();
        }

        void CacheAnimatorHashes()
        {
            if (animator == null) return;
            // We don't have a way to query if a param exists cheaply at runtime, so we compute hashes.
            // Setting nonexistent hashes is harmless; Animator ignores them. To be safe, allow disabling by empty name.
            if (!string.IsNullOrEmpty(speedParam)) _speedHash = Animator.StringToHash(speedParam);

        }

        void Update()
        {
            Vector2 input = ReadInput();
            Vector3 move = new Vector3(input.x, 0f, input.y); // 3D/top-down default

            // Movement
            float dt = Time.deltaTime;

            if (_cc != null)
            {
                _cc.SimpleMove(move * moveSpeed);
            }
            else
            {
                transform.position += move * (moveSpeed * dt);
            }

            // Facing
            if (faceMoveDirection)
            {
                if (move.sqrMagnitude > 0.0001f)
                {
                    // Rotate on Y axis for 3D top-down
                    Vector3 look = new Vector3(move.x, 0f, move.z);
                    transform.forward = look;
                }
            }

            if (flipSpriteWithX && _sprite != null)
            {
                if (Mathf.Abs(input.x) > 0.01f)
                {
                    _sprite.flipX = input.x < 0f;
                }
            }

            // Animation
            if (animator != null)
            {
                float speed = Mathf.Clamp01(input.magnitude);
                if (_speedHash != -1) animator.SetFloat(_speedHash, speed);
            }
        }

        Vector2 ReadInput()
        {
            Vector2 v = Vector2.zero;

            if (v.sqrMagnitude < 0.0001f && keyboardFallback)
            {
                float x = 0f, y = 0f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
                v = new Vector2(x, y);
                if (v.sqrMagnitude > 1f) v.Normalize();
            }
            else
            {
                v = joystick.Direction;
            }

            return v;
        }
    }
}