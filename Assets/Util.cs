using System;
using System.Collections;
using UnityEngine;

public static class Util
{
    public static LayerMask nonColliderMasks => ~(
        LayerMask.GetMask("Player") |
        LayerMask.GetMask("PlayerCollider") |
        LayerMask.GetMask("PlayerHitbox") |
        LayerMask.GetMask("Enemy") |
        LayerMask.GetMask("EnemyCollider") |
        LayerMask.GetMask("EnemyHitbox") |
        LayerMask.GetMask("IgnoreCollisions")
    );
    
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