using System.Collections;
using UnityEngine;

public class Door : InteractableMonobehavior
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
        locked = true;
    }

    public void ToggleLocked()
    {
        locked = !locked;
        active = !locked;
    }

    public override void Interact()
    {
        active = false;
        StartCoroutine(OpenDoorAnimation(leftDoor, -1));
        StartCoroutine(OpenDoorAnimation(rightDoor, 1));
    }

    IEnumerator OpenDoorAnimation(GameObject door, float direction)
    {
        Quaternion rot = Quaternion.AngleAxis(135f * direction, Vector2.up);

        while (!door.transform.localRotation.Equals(rot))
        {
            door.transform.localRotation = Quaternion.Slerp(door.transform.localRotation, rot, 0.1f);
            yield return new WaitForEndOfFrame();
        }

        door.transform.localRotation = rot;
    }
}
