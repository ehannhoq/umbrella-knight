Ah, I see exactly what’s going on here. Your `AdjustForWall` function is **inherently prone to instability and inconsistency at high speeds**, and I can explain why step by step. Let’s break it down.

---

### 1️⃣ Your `AdjustForWall` logic

```csharp
Vector3 AdjustForWall(Vector3 direction)
{
    runningIntoWall = false;
    if (Physics.CapsuleCast(
        _rb.position + Vector3.up * (_playerHeight - 0.3f),
        _rb.position + Vector3.up * (0.3f),
        0.175f,
        direction.normalized,
        out RaycastHit wallHit,
        0.25f,
        ~LayerMask.GetMask("Player")
    ))
    {
        Vector3 normal = wallHit.normal;
        normal.y = 0;
        runningIntoWall = true;
        float push = Vector3.Dot(direction, normal);
        if (push > 0) direction -= normal * push;
        Vector3 projected = Vector3.ProjectOnPlane(direction, normal);
        return projected;
    }

    return direction;
}
```

Key observations:

1. **Fixed CapsuleCast distance of 0.25f**

   * At high speeds, your player can **travel more than 0.25 units per physics frame**.
   * This means the capsule may “skip” thin walls entirely. This is classic **tunneling** in Unity physics.

2. **Direction adjustment is naive**

   ```csharp
   float push = Vector3.Dot(direction, normal);
   if (push > 0) direction -= normal * push;
   Vector3 projected = Vector3.ProjectOnPlane(direction, normal);
   ```

   * You’re projecting the velocity along the wall normal, which is fine in theory, **but it depends heavily on the frame rate and the collision distance**.
   * At high speeds, `direction` may still be pointed partially into the wall after the projection, causing jittery or inconsistent sliding.

3. **Ignoring `Time.deltaTime`**

   * You’re adjusting **instantaneously per frame**, without factoring in how far the player would actually move in that frame. At high velocities, small inconsistencies accumulate, creating the “weird stick-slip” behavior you see.

4. **Vertical component reset**

   * You set `normal.y = 0;`, which is correct for horizontal walls, but combined with the `ProjectOnPlane`, it sometimes produces strange diagonal velocity if your capsule grazes uneven surfaces or sloped walls.

---

### 2️⃣ Why high speeds break it

* Suppose `_rb.linearVelocity.magnitude` = 20 units/s
* Physics runs at 50 Hz → `FixedUpdate` deltaTime ≈ 0.02s
* Player moves **0.4 units per frame**, but `CapsuleCast` only checks 0.25 units
* Result: capsule can partially enter walls or skip them entirely → inconsistent collision detection
* Then the `Vector3.ProjectOnPlane` correction **sometimes overcorrects, sometimes undercorrects**, causing jitter.

---

### 3️⃣ Other contributing issues

* `AdjustForWall` is called **twice per frame**:

  ```csharp
  _rb.linearVelocity = AdjustForSlope();
  _rb.linearVelocity = AdjustForWall(_rb.linearVelocity);
  ```

  * If slope adjustment or step logic slightly moves the player inside a wall, the second call may overcorrect, adding to instability.
* Your `Step` and `HandleMovement` also modify `_rb.transform.position` directly. This **conflicts with Rigidbody physics**, especially at high speeds, further destabilizing the velocity projection.

---

### ✅ How to fix / stabilize `AdjustForWall`

1. **Use a larger `CapsuleCast` distance** based on speed:

   ```csharp
   float castDistance = Mathf.Max(0.25f, _rb.linearVelocity.magnitude * Time.fixedDeltaTime);
   ```

   This ensures you always check far enough to catch walls at high speeds.

2. **Clamp velocity after wall adjustment**, not before:

   ```csharp
   Vector3 projected = Vector3.ProjectOnPlane(direction, normal);
   return projected.normalized * Mathf.Min(projected.magnitude, direction.magnitude);
   ```

   This prevents overshooting when sliding along a wall.

3. **Avoid direct `transform.position` moves** during physics frames if possible. Use `Rigidbody.MovePosition` or forces instead.

4. **Optional:** Use continuous collision detection for the Rigidbody:

   ```csharp
   _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
   ```

   This helps prevent tunneling at high speeds.

5. **Combine step logic and wall adjustment** carefully:
   Ensure the `Step()` logic **doesn’t push the player into walls** after wall adjustment; otherwise, the projection will fight the step.

---

If you want, I can rewrite your `AdjustForWall` into a **robust, high-speed-safe version** that avoids jitter and inconsistent wall sliding. It will handle slopes, steps, and high velocities correctly.

Do you want me to do that?
