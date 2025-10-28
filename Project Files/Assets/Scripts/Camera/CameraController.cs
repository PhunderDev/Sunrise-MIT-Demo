using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraController : MonoBehaviour
{

    public struct Effect
    {
        public EffectTypes Type;
        public float Strength;
        public float Duration;
        public float Speed;

        public Effect(EffectTypes type, float strength, float duration, float speed)
        {
            Type = type;
            Strength = strength;
            Duration = duration;
            Speed = speed;
        }
    }
    public enum EffectTypes
    {
        Shake = 0
    }
    public enum EffectsEnum
    {
        CameraShake_Weak = 0,
        CameraShake_Mid = 1,
        CameraShake_Strong = 2,
    }


    public Effect[] Effects = new Effect[3] {
        new Effect(EffectTypes.Shake, 0.4f, 0.5f, 2f),
        new Effect(EffectTypes.Shake, 0.5f, 0.5f, 4f),
        new Effect(EffectTypes.Shake, 0.6f, 0.75f, 8f),
    };


    // Singleton Definition
    public static CameraController Instance;

    // Adjustable
    public Transform CameraTransformTarget;
    public Vector3 CameraTarget;
    public Vector2 CameraOffset;
    public float CameraLookAheadOffset;
    public float LockedTargetCameraLookAheadOffset;
    public float DampingStrength;

    // Required To Work
    private Vector3 CameraPosition;
    private Vector2 CurrentOffset;

    // Shake Related
    private Vector3 ShakeOffset = Vector3.zero;
    private Coroutine shakeCoroutine;

    // Lookahead
    public float LookAheadDampingStrength;
    private int CurrentDirection = 1;
    private float XSpeedCoefficient = 0f;
    private Vector3 LookAheadOffset = Vector3.zero;

    // Restrictions
    [SerializeField]
    private bool AllowCollisions;
    private Bounds CurrentCollisionSize = new Bounds();
    private BoxCollider2D CameraCollider;
    private List<BoxCollider2D> Colliders = new List<BoxCollider2D>();
    private BoxCollider2D CurrentlyObeyedCollider;
    private Vector4 CurrentlyDisabledDirections = Vector4.zero; // Top Right Bottom Left, just like in css


    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;

        CameraCollider = GetComponent<BoxCollider2D>();

        CameraPosition = transform.position;
        UpdateCameraColliderSize();
    }

    private void OnValidate()
    {
        CameraOffset = new Vector2(Mathf.Clamp(CameraOffset.x, 0f, 100f), Mathf.Clamp(CameraOffset.y, -100f, 100f));
        if(!AllowCollisions) CurrentlyDisabledDirections = Vector4.zero;
    }


    #region Basic Camera Movement
    public void ChangeDirection(int NewDirection)
    {
        CurrentDirection = NewDirection;
        //AddEffect(EffectsEnum.CameraShake_Weak);
    }

    public void FollowTransform(Transform NewTranformToFollow)
    {
        CameraTransformTarget = NewTranformToFollow;
    }

    public void FollowLocation(Vector3 NewLocationToFollow)
    {
        CameraTransformTarget = null;
        CameraTarget = NewLocationToFollow;
    }

    #endregion

    public void SetSpeedCoefficient(float Speed, float MaxSpeed)
    {
        XSpeedCoefficient = Mathf.Clamp(Speed / MaxSpeed, -1f, 1f);

    }

    #region Effects
    public void AddEffect(EffectsEnum Effect)
    {
        Effect eff = Effects[(int)Effect];
        switch (eff.Type)
        {
            case EffectTypes.Shake:
                CameraShake(eff.Duration, eff.Strength, eff.Speed);
                break;

        }
    }

    public void CameraShake(float duration, float amplitude, float speed)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            ShakeOffset = Vector3.zero;
        }
        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, amplitude, speed));
    }

    private IEnumerator ShakeCoroutine(float duration, float amplitude, float speed)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Mathf.PerlinNoise(Time.time * speed, 0f) * 2 - 1;
            float y = Mathf.PerlinNoise(0f, Time.time * speed) * 2 - 1;
            ShakeOffset = new Vector3(x, y, 0) * amplitude;
            yield return null;
        }
        ShakeOffset = Vector3.zero;
    }
    
    #endregion

    void LateUpdate()
    {
        // If following a transform => Set the target position to the targeted transform's position
        if(CameraTransformTarget != null) CameraTarget = CameraTransformTarget.position;
        // If not following a tranform but also no target point is set => dindunuffin
        if (CameraTarget == null) return;


        // Get the wanted position and lerp to it
        Vector3 desiredPosition = new Vector3(CameraTarget.x + CameraOffset.x, CameraTarget.y + CameraOffset.y, transform.position.z);
        CameraPosition = Vector3.Lerp(CameraPosition, desiredPosition, Time.deltaTime * DampingStrength);
        //if (CameraTransformTarget == null) CameraPosition.y = desiredPosition.y;

        float LookAheadDampingOffset = CameraLookAheadOffset;
        if (CameraTransformTarget == null) LookAheadDampingOffset = LockedTargetCameraLookAheadOffset;


        LookAheadOffset = Vector3.Lerp(LookAheadOffset, new Vector3(LookAheadDampingOffset * XSpeedCoefficient, 0f, 0f), Time.deltaTime * LookAheadDampingStrength);

        // Check For Collisions
        if (AllowCollisions)
        {
            if (CameraPosition.x > transform.position.x && CurrentlyDisabledDirections.y == 1) CameraPosition.x = transform.position.x;
            if (CameraPosition.x < transform.position.x && CurrentlyDisabledDirections.w == 1) CameraPosition.x = transform.position.x;
            if (CameraPosition.y > transform.position.y && CurrentlyDisabledDirections.x == 1) CameraPosition.y = transform.position.y;
            if (CameraPosition.y < transform.position.y && CurrentlyDisabledDirections.z == 1) CameraPosition.y = transform.position.y;
        }

        // Offset the position by the shake modifier
        transform.position = CameraPosition + LookAheadOffset + ShakeOffset;
    }

    private void Update()
    {
        //UpdateCameraColliderSize();
        if(AllowCollisions) CheckCollisions();
    }

    private void CheckCollisions()
    {
        CurrentlyDisabledDirections = Vector4.zero;
        if (Colliders.Count <= 0) return;

        bool ExceedsRightLimit = true;
        bool ExceedsLeftLimit = true;
        bool ExceedsTopLimit = true;
        bool ExceedsBottomLimit = true;
        foreach (BoxCollider2D col in Colliders)
        {
            if(ExceedsRightLimit) ExceedsRightLimit = col.bounds.max.x < transform.position.x + (CurrentCollisionSize.size.x / 2f);
            if (ExceedsLeftLimit) ExceedsLeftLimit = col.bounds.min.x > transform.position.x - (CurrentCollisionSize.size.x / 2f);
            if (ExceedsTopLimit) ExceedsTopLimit = col.bounds.max.y < transform.position.y + (CurrentCollisionSize.size.y / 2f);
            if (ExceedsBottomLimit) ExceedsBottomLimit = col.bounds.min.y > transform.position.y - (CurrentCollisionSize.size.y / 2f);
        }

        if (ExceedsTopLimit) CurrentlyDisabledDirections.x = 1;
        if (ExceedsRightLimit) CurrentlyDisabledDirections.y = 1;
        if (ExceedsBottomLimit) CurrentlyDisabledDirections.z = 1;
        if (ExceedsLeftLimit) CurrentlyDisabledDirections.w = 1;

    }

    private bool IsInsideOfARectangle(Vector2 Pos1, Vector2 Pos2, Vector2 PosToCheck)
    {
        bool ContainsX = Pos1.x <= PosToCheck.x && Pos2.x >= PosToCheck.x;
        bool ContainsY = Pos1.y >= PosToCheck.y && Pos2.y <= PosToCheck.y;
        return ContainsX && ContainsY;
    }

    private void UpdateCameraColliderSize()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found!");
            return;
        }

        // Get screen bounds in world space
        float screenHeight = 2f * Camera.main.orthographicSize;
        float screenWidth = screenHeight * Camera.main.aspect;

        // Set collider size to match screen dimensions
        CurrentCollisionSize.size = new Vector2(screenWidth, screenHeight);
        CameraCollider.size = CurrentCollisionSize.size;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Colliders.Add(collision.gameObject.GetComponent<BoxCollider2D>());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        for(int i = 0; i < Colliders.Count; i++)
        {
            if (Colliders[i] == collision.gameObject.GetComponent<BoxCollider2D>())
            {
                Colliders.RemoveAt(i);
                break;
            }
        }
    }

    private void OnDrawGizmos()
    {

        if (Colliders.Count <= 0) return;

        Gizmos.color = Color.red;
        if (CurrentlyDisabledDirections.x == 1) Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y + (CurrentCollisionSize.size.y / 2f), 0f), new Vector2(CurrentCollisionSize.size.x, 1f));
        if (CurrentlyDisabledDirections.z == 1) Gizmos.DrawCube(new Vector3(transform.position.x, transform.position.y - (CurrentCollisionSize.size.y / 2f), 0f), new Vector2(CurrentCollisionSize.size.x, 1f));
        if (CurrentlyDisabledDirections.y == 1) Gizmos.DrawCube(new Vector3(transform.position.x + (CurrentCollisionSize.size.x / 2f), transform.position.y, 0f), new Vector2(1f, CurrentCollisionSize.size.y));
        if (CurrentlyDisabledDirections.w == 1) Gizmos.DrawCube(new Vector3(transform.position.x - (CurrentCollisionSize.size.x / 2f), transform.position.y, 0f), new Vector2(1f, CurrentCollisionSize.size.y));
    }




}