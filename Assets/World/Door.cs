using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorState
    {
        Open,
        Closed
    }

    [SerializeField] GameObject leftDoor;
    [SerializeField] GameObject rightDoor;
    
    public DoorState state;
    public bool locked;

    void Start()
    {
        state = DoorState.Closed;
        locked = false;
    }

    public void ToggleState()
    {
        if (state == DoorState.Open) state = DoorState.Closed;
        if (state == DoorState.Closed) state = DoorState.Open;
    }

    void Animate()
    {
        
    }
}
