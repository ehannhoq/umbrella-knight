using System.Collections;
using UnityEngine;

public class Chest : InteractableMonobehavior
{
    [SerializeField] float _slerpT;
    [SerializeField] ChestLoot chestLoot;

    private Transform _lid; 
    public bool opened;

    
    void Start()
    {
        _lid = transform.GetChild(0);        
    }

    public override void Interact()
    {
        if (opened) return;
        opened = true;
        active = false;
        StartCoroutine(LidOpenAnimation());
    }

    IEnumerator LidOpenAnimation()
    {
        while (_lid.localRotation != Quaternion.identity)
        {
            _lid.localRotation = Quaternion.Slerp(_lid.localRotation, Quaternion.identity, _slerpT);
            yield return new WaitForEndOfFrame();
        }

        _lid.localRotation = Quaternion.identity;
    }
}
