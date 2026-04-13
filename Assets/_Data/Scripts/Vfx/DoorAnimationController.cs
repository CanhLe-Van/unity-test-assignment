using System;
using System.Collections;
using UnityEngine;

public class DoorAnimationController : MonoBehaviour
{
    public static DoorAnimationController ins;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [SerializeField] private string openParam = "isOpen";
    [SerializeField] private float animDuration = 1f;

    private Coroutine currentRoutine;

    public bool IsOpen { get; private set; }


    private void Awake()
    {
        if (ins != null && ins != this)
        {
            Destroy(gameObject);
            return;
        }

        ins = this;
    }
    public void Open(Action onComplete = null)
    {
        Play(true, onComplete);
    }

    public void Close(Action onComplete = null)
    {
        Play(false, onComplete);
    }

    public void Toggle(Action onComplete = null)
    {
        Play(!IsOpen, onComplete);
    }

    private void Play(bool open, Action onComplete)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(PlayRoutine(open, onComplete));
    }

    private IEnumerator PlayRoutine(bool open, Action onComplete)
    {
        IsOpen = open;

        animator.SetBool(openParam, open);

        yield return new WaitForSeconds(animDuration);

        onComplete?.Invoke();
    }
}