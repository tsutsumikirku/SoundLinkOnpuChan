using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NewCamera : MonoBehaviour, IBeginDragHandler, IDragHandler, UnityEngine.EventSystems.IEndDragHandler
{
    [SerializeField] private float cameraMoveTime = 5f;
    [SerializeField] private float cameraResetTime = 0.2f;
    [SerializeField] private Transform[] sequencePositions;
    [SerializeField] private Transform playerTransform;
    private Transform goalTransform;
    [SerializeField] private UIButton cameraResetButton;
    [SerializeField] private Vector2 offset;    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float maxHight = 100f;
    [SerializeField] private float minHight = -10f;
    [SerializeField] private float minFOV = 20f;
    [SerializeField] private float maxFOV = 60f;
    [SerializeField] [Range(0f,1f)] private float minMoveSensitivity = 0.2f;
    [SerializeField] private float inertiaDamping = 5f; // larger = quicker stop
    [SerializeField] private float inertiaMinSpeed = 0.01f; // threshold to stop
    Tween tween = null;
    Vector3 inertiaVelocity = Vector3.zero;
    Vector2 lastDragDelta = Vector2.zero;
    bool isPlayerTracking = true;
    Camera mainCamera;
    Transform cameraPosition;
    float initialPlayerX;
    float goalX;
    float boundMinX;
    float boundMaxX;
    float boundMinY;
    float boundMaxY;
    [SerializeField]float defaultFOV;
    void Awake()
    {
        Init().Forget();    
    }
    async UniTask Init()
    {
        cameraResetButton.OnClick = CameraReset;
        mainCamera = GameObject.FindWithTag("MeinCamera").GetComponent<Camera>();
        playerTransform = GameObject.FindWithTag("Player").transform;
        goalTransform = GameObject.FindWithTag("Goal").transform;
        cameraPosition = mainCamera.transform;
        defaultFOV = mainCamera.fieldOfView;
        initialPlayerX = playerTransform.position.x;
        goalX = goalTransform.position.x;
        var initialPos = GetFollowTargetPosition();
        initialPos.z = cameraPosition.position.z;
        cameraPosition.position = initialPos;
        UpdateMovementBounds();
        foreach (var target in sequencePositions)
        {
            var seqPos = target.position;
            seqPos.z = cameraPosition.position.z;
            // clamp sequence target to current movement bounds
            seqPos.x = Mathf.Clamp(seqPos.x, boundMinX, boundMaxX);
            seqPos.y = Mathf.Clamp(seqPos.y, boundMinY, boundMaxY);
            tween = cameraPosition.DOMove(seqPos, cameraMoveTime);
            await tween.AsyncWaitForCompletion();
        }
        if(!isPlayerTracking)return;
        var followPos = GetFollowTargetPosition();
        followPos.z = cameraPosition.position.z;
        UpdateMovementBounds();
        followPos.x = Mathf.Clamp(followPos.x, boundMinX, boundMaxX);
        followPos.y = Mathf.Clamp(followPos.y, boundMinY, boundMaxY);
        tween = cameraPosition.DOMove(followPos, cameraMoveTime);
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        tween?.Kill();
        isPlayerTracking = false;
        cameraResetButton.gameObject.SetActive(true);
        inertiaVelocity = Vector3.zero;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if(Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;
            float difference = currentMagnitude - prevMagnitude;
            mainCamera.fieldOfView -= difference * zoomSpeed * Time.deltaTime;
            if(mainCamera.fieldOfView < minFOV)
                mainCamera.fieldOfView = minFOV;
            if(mainCamera.fieldOfView > maxFOV)
                mainCamera.fieldOfView = maxFOV;
            // FOV changed -> recalc bounds before applying clamps
            UpdateMovementBounds();
        }
        // Scale move sensitivity based on zoom level: when zoomed in (small FOV) reduce sensitivity
        float zoomNorm = Mathf.InverseLerp(minFOV, maxFOV, mainCamera.fieldOfView); // 0 when minFOV(zoomed in), 1 when maxFOV(zoomed out)
        float sensitivityMul = Mathf.Lerp(minMoveSensitivity, 1f, zoomNorm);
        var delta = new Vector3(-eventData.delta.x, -eventData.delta.y, 0) * Time.deltaTime * moveSpeed * sensitivityMul;
        // record last drag delta for inertia calculation on release
        lastDragDelta = eventData.delta;
        cameraPosition.position += new Vector3(delta.x, delta.y, 0);
        if(cameraPosition.position.y > boundMaxY)
            cameraPosition.position = new Vector3(cameraPosition.position.x, boundMaxY, cameraPosition.position.z);
        if(cameraPosition.position.y < boundMinY)
            cameraPosition.position = new Vector3(cameraPosition.position.x, boundMinY, cameraPosition.position.z);
        if(cameraPosition.position.x < boundMinX)
            cameraPosition.position = new Vector3(boundMinX, cameraPosition.position.y, cameraPosition.position.z);
        if(cameraPosition.position.x > boundMaxX)
            cameraPosition.position = new Vector3(boundMaxX, cameraPosition.position.y, cameraPosition.position.z);
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        // start inertia based on last drag delta
        float zoomNorm = Mathf.InverseLerp(minFOV, maxFOV, mainCamera.fieldOfView);
        float sensitivityMul = Mathf.Lerp(minMoveSensitivity, 1f, zoomNorm);
        // convert the lastDelta into world units per second approximation
        inertiaVelocity = new Vector3(-lastDragDelta.x, -lastDragDelta.y, 0) * moveSpeed * sensitivityMul;
    }
    private void CameraReset()
    {
        cameraResetButton.gameObject.SetActive(false);
        if (isPlayerTracking) return;
        isPlayerTracking = true;
    }

    // Recalculate movement bounds based on current camera FOV and aspect
    void UpdateMovementBounds()
    {
        if (mainCamera == null || playerTransform == null || cameraPosition == null) return;
        // Use distance from camera to player as focal plane for size calculation
        float focalDistance = Mathf.Abs(playerTransform.position.z - cameraPosition.position.z);
        float halfHeight = focalDistance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * mainCamera.aspect;

        float leftMaxX = Mathf.Min(initialPlayerX, goalX);
        float rightMaxX = Mathf.Max(initialPlayerX, goalX);
        boundMinX = leftMaxX;
        boundMaxX = rightMaxX;

        boundMinY = minHight + halfHeight;
        boundMaxY = maxHight - halfHeight;

        // If bounds inverted, collapse to center
        if (boundMinX > boundMaxX)
        {
            float mid = (boundMinX + boundMaxX) * 0.5f;
            boundMinX = boundMaxX = mid;
        }
        if (boundMinY > boundMaxY)
        {
            float mid = (boundMinY + boundMaxY) * 0.5f;
            boundMinY = boundMaxY = mid;
        }
    }
    Vector3 GetFollowTargetPosition()
    {
        if (playerTransform == null || goalTransform == null)
            return cameraPosition != null ? cameraPosition.position : Vector3.zero;
        var midpoint = (playerTransform.position + goalTransform.position) * 0.5f;
        return midpoint + (Vector3)offset;
    }
    void Update()
    {
        if (isPlayerTracking)
        {
            UpdateMovementBounds();
            // trueだった場合はカメラリセット
            cameraResetButton.gameObject.SetActive(false);
            var target3 = GetFollowTargetPosition();
            target3.z = cameraPosition.position.z;
            // clamp target to movement bounds
            target3.x = Mathf.Clamp(target3.x, boundMinX, boundMaxX);
            target3.y = Mathf.Clamp(target3.y, boundMinY, boundMaxY);
            // カメラの位置のリセットをcameraResetTime秒かけて行う
            float t = Time.deltaTime / Mathf.Max(0.00001f, cameraResetTime);
            cameraPosition.position = Vector3.Lerp(cameraPosition.position, target3, t);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, defaultFOV, t);
        }
        else
        {
            // apply inertia when user released drag
            if (inertiaVelocity.sqrMagnitude > inertiaMinSpeed * inertiaMinSpeed)
            {
                // move by inertia (velocity is approx pixels * moveSpeed * sensitivity)
                Vector3 delta = inertiaVelocity * Time.deltaTime;
                cameraPosition.position += new Vector3(delta.x, delta.y, 0);
                UpdateMovementBounds();
                // clamp to bounds
                cameraPosition.position = new Vector3(
                    Mathf.Clamp(cameraPosition.position.x, boundMinX, boundMaxX),
                    Mathf.Clamp(cameraPosition.position.y, boundMinY, boundMaxY),
                    cameraPosition.position.z
                );
                // decay
                inertiaVelocity = Vector3.Lerp(inertiaVelocity, Vector3.zero, inertiaDamping * Time.deltaTime);
                if (inertiaVelocity.sqrMagnitude <= inertiaMinSpeed * inertiaMinSpeed)
                    inertiaVelocity = Vector3.zero;
            }
        }
    }
}
