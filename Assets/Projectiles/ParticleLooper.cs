using System.Collections;
using UnityEngine;

public class ParticleLooper : MonoBehaviour
{
    public float loopOffset = 0.2f;

    void Start()
    {
        ParticleSystem[] systems = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in systems)
        {
            StartCoroutine(LoopParticles(ps));
        }
    }

    IEnumerator LoopParticles(ParticleSystem ps)
    {
        var main = ps.main;
        main.loop = false;

        while (true)
        {
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration * (1 - loopOffset));
        }
    }
}
