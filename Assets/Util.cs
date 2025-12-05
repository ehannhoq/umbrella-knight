using System;
using System.Collections;
using UnityEngine;

public static class Util
{
    public static LayerMask nonColliderMasks => ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Collision") | LayerMask.GetMask("NPCHitbox") | LayerMask.GetMask("PlayerHitbox") | LayerMask.GetMask("UmbrellaCollider") | LayerMask.GetMask("HurtCollider"));
    public static IEnumerator DelayedActionSeconds(float seconds, Action code)
    {
        yield return new WaitForSeconds(seconds);
        code.Invoke();
    }

    public static IEnumerator DelayedActionEndOfFrame(Action code)
    {
        yield return new WaitForEndOfFrame();
        code.Invoke();
    }
}