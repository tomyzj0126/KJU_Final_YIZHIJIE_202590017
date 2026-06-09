using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageTrap : MonoBehaviour
{
    [Header("Damage")]
    public float DamageAmount = 10f;
    public bool DamageRepeatedly = true;
    public float DamageInterval = 1f;

    [Header("Target")]
    public bool RequirePlayerTag = false;
    public string PlayerTag = "Player";

    private Coroutine damageRoutine;
    private PlayerHealth currentTarget;

    private void Reset()
    {
        Collider trapCollider = GetComponent<Collider>();
        trapCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        if (DamageInterval < 0.05f)
        {
            DamageInterval = 0.05f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = GetPlayerHealth(other);
        if (playerHealth == null)
        {
            return;
        }

        currentTarget = playerHealth;
        currentTarget.TakeDamage(DamageAmount);

        if (DamageRepeatedly && damageRoutine == null)
        {
            damageRoutine = StartCoroutine(DamageWhileStanding());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth playerHealth = GetPlayerHealth(other);
        if (playerHealth == null || playerHealth != currentTarget)
        {
            return;
        }

        currentTarget = null;

        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }
    }

    private IEnumerator DamageWhileStanding()
    {
        while (currentTarget != null)
        {
            yield return new WaitForSeconds(DamageInterval);

            if (currentTarget != null)
            {
                currentTarget.TakeDamage(DamageAmount);
            }
        }

        damageRoutine = null;
    }

    private PlayerHealth GetPlayerHealth(Collider other)
    {
        if (RequirePlayerTag && !other.CompareTag(PlayerTag))
        {
            return null;
        }

        return other.GetComponentInParent<PlayerHealth>();
    }
}
