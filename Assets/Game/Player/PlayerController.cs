using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour {
    [SerializeField] private float movementSpeed;
    [SerializeField] private float minTargetDistance;
    [Header("Perspective")]
    [SerializeField] private float baseYPosition;
    [SerializeField] private float roomPerspectiveYModifier;
    [SerializeField] private float roomPerspectiveScaleModifier;
    [SerializeField] private Vector2 scaleRange;
    [Header("Imports")]
    [SerializeField] private Transform appearanceTransform;
    [SerializeField] private Rigidbody2D physicalBody;
    [SerializeField] private Transform legsPosition;

    private MovementType currentMovementType;
    private bool isInputLocked = false;
    private Vector2 targetPosition;
    private UnityAction arrivalCallback;
    //Pathing
    private NavMeshPath path;
    private int pathIndex;

    public void MoveTo(Vector2 _targetPosition, UnityAction callback) {
        HandleSettingTarget(_targetPosition, callback);
        isInputLocked = true;
    }

    public void TeleportTo(Vector2 position) {
        transform.position = (Vector3)position - legsPosition.localPosition;
    }

    public Vector2 GetPosition() {
        return legsPosition.position;
    }

    private void Update() {
        HandleInput();
        HandleMovement();
        UpdatePerspective();
    }

    private void HandleInput() {
        if (!isInputLocked && !DialogueSystem.inst.isDialogueOngoing && Input.GetMouseButtonDown(0) && CanWalkToMouse()) {
            HandleSettingTarget(ScreenUtils.WorldMouse(), null);
        }
        isInputLocked = false;
    }

    private void HandleSettingTarget(Vector2 _targetPosition, UnityAction callback) {
        arrivalCallback = callback;

        path = new NavMeshPath();
        bool isSuccessful = NavMesh.CalculatePath(GetPosition(), (Vector3)_targetPosition - legsPosition.localPosition,NavMesh.AllAreas,path);
        currentMovementType = isSuccessful ? MovementType.Mouse : MovementType.None;
        pathIndex = 1;
    }

    private void HandleMovement() {
        if (currentMovementType == MovementType.None) {
            physicalBody.velocity = Vector2.zero;
            return;
        }
        //Move to next point in path
        targetPosition = path.corners[pathIndex];

        Vector2 direction = targetPosition - (Vector2)transform.position;
        //Modify direction to account for perspective
        direction.y /= roomPerspectiveYModifier;
        physicalBody.velocity = movementSpeed * direction.normalized * new Vector2(1, roomPerspectiveYModifier);
        if (Vector2.Distance(targetPosition, transform.position) < minTargetDistance) {
            //If point is last
            if (pathIndex >= path.corners.Length - 1) {
                arrivalCallback?.Invoke();
                currentMovementType = MovementType.None;
            } else {
                //Goto next point
                pathIndex++;
            }
        }
    }

    private void UpdatePerspective() {
        float dy = baseYPosition - transform.position.y;
        float scale = Mathf.Clamp(1 + dy * roomPerspectiveScaleModifier, scaleRange.x, scaleRange.y);
        appearanceTransform.localScale = Vector3.one * scale;
    }

    //TODO: Move to interaction class, when it'll be present
    private bool CanWalkToMouse() {
        Collider2D collider = Physics2D.OverlapCircle(ScreenUtils.WorldMouse(),0.1f);
        return collider == null || collider.isTrigger;
    }

    private enum MovementType {
        None,
        Mouse
    }
}
