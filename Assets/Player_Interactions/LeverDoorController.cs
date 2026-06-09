using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class LeverDoorController : MonoBehaviour, IInteractable
{
    [Header("Door")]
    public Actor_Animation DoorAnimation;
    public bool ToggleDoor = false;
    public bool OneShot = true;

    [Header("Lever Motion")]
    public Transform LeverHandle;
    public Vector3 PulledLocalEulerOffset = new Vector3(-60f, 0f, 0f);
    public float MoveDuration = 0.25f;

    [Header("Events")]
    public UnityEvent OnLeverPulled;
    public UnityEvent OnLeverReset;

    private Quaternion initialRotation;
    private Coroutine moveRoutine;
    private bool isPulled;
    private bool hasBeenUsed;

    private void Awake()
    {
        if (LeverHandle == null)
        {
            LeverHandle = transform;
        }

        initialRotation = LeverHandle.localRotation;
    }

    public void OnClick()
    {
        OnClick(gameObject);
    }

    public void OnClick(GameObject sender)
    {
        if (OneShot && hasBeenUsed)
        {
            return;
        }

        if (ToggleDoor && isPulled)
        {
            CloseDoor();
            SetLeverPulled(false);
            OnLeverReset?.Invoke();
            return;
        }

        OpenDoor();
        SetLeverPulled(true);
        hasBeenUsed = true;
        OnLeverPulled?.Invoke();
    }

    public void OpenDoor()
    {
        if (DoorAnimation != null)
        {
            DoorAnimation.Act_Event_SetSpeed(1f);
        }
    }

    public void CloseDoor()
    {
        if (DoorAnimation != null)
        {
            DoorAnimation.Act_Event_SetSpeed(-1f);
        }
    }

    private void SetLeverPulled(bool pulled)
    {
        isPulled = pulled;

        Quaternion targetRotation = initialRotation;
        if (pulled)
        {
            targetRotation *= Quaternion.Euler(PulledLocalEulerOffset);
        }

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
        }

        moveRoutine = StartCoroutine(MoveLever(targetRotation));
    }

    private IEnumerator MoveLever(Quaternion targetRotation)
    {
        Quaternion startRotation = LeverHandle.localRotation;
        float elapsed = 0f;

        while (elapsed < MoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / MoveDuration);
            LeverHandle.localRotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        LeverHandle.localRotation = targetRotation;
        moveRoutine = null;
    }

    public void OnEnter() { }
    public void OnExit() { }
    public void OnStay() { }

    public void OnEnter(GameObject sender) { }
    public void OnExit(GameObject sender) { }
    public void OnStay(GameObject sender) { }
}
