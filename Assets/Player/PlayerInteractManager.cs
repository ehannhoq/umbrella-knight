using UnityEngine;

public class PlayerInteractManager : MonoBehaviour
{
    [SerializeField] float _interactDistance;

    private Camera _cam;
    private int _outlineLayer;
    private int _defaultLayer;

    private InteractableMonobehavior lookingAt;
    private InteractableMonobehavior lastLookingAt;

    void Start()
    {
        _cam = Camera.main;
        _outlineLayer = LayerMask.NameToLayer("Outlined Object");
        _defaultLayer = LayerMask.NameToLayer("Default");
    }

    void Update()
    {
        lastLookingAt = lookingAt;
        lookingAt = null;

        if (Physics.Raycast(
            _cam.transform.position,
            _cam.transform.forward,
            out RaycastHit hit,
            _interactDistance,
            Util.nonColliderMasks
        ))
        {
            InteractableMonobehavior interactable = FindInteractable(hit.transform);

            if (interactable != null)
                if (interactable.active)
                    lookingAt = interactable;
        }

        HandleOutlines();
    }


    private InteractableMonobehavior FindInteractable(Transform t)
    {
        if (t.TryGetComponent(out InteractableMonobehavior interactableMonobehavior))
            return interactableMonobehavior;

        if (t.parent == null)
            return null;

        return FindInteractable(t.parent);
    }

    private void HandleOutlines()
    {
        if (lastLookingAt != null && lastLookingAt != lookingAt)
            SetLayerRecursive(lastLookingAt.gameObject, _defaultLayer);

        if (lookingAt != null && lastLookingAt != lookingAt)
            SetLayerRecursive(lookingAt.gameObject, _outlineLayer);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    public void OnInteract()
    {
        if (lookingAt != null)
            lookingAt.Interact();
    }
}


public abstract class InteractableMonobehavior : MonoBehaviour
{
    public bool active = true;
    public abstract void Interact();
}