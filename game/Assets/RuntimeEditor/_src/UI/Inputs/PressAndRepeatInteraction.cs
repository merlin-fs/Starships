using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public class PressAndRepeatInteraction : IInputInteraction
{
    /// <summary> Make PressAndRepeatInteraction available in the Input Action Asset Editor window
    /// The method is run by [InitializeOnLoad] </summary>
    static PressAndRepeatInteraction()
    {
        //TODO: проверить почему не роботает
        //InputSystem.RegisterInteraction<PressAndRepeatInteraction>();
    }

    private static readonly float defaultButtonPressPoint = 0.5f;

    [Tooltip("Perform immediately when the Control is actuated.")]
    public bool fireImmediately = true;

    [Tooltip("Seconds to wait after the button is first pressed down before the perform event fires again.")]
    public float initialPause = 0.5f;

    [Tooltip("Seconds to repeatedly wait for the perform event to be fired again. This pause is repeated until the control is no longer actuated.")]
    public float repeatedPause = 0.2f;

    public float pressPoint = 0.5f;

    private float InitialPauseOrDefault => initialPause > 0.0 ? initialPause : InputSystem.settings.defaultHoldTime;
    private float RepeatedPauseOrDefault => repeatedPause > 0.0 ? repeatedPause : InputSystem.settings.defaultHoldTime;
    private float PressPointOrDefault => pressPoint > 0.0 ? pressPoint : defaultButtonPressPoint;

    private float ignoreInputUntilTime;

    /// <inheritdoc />
    public void Process(ref InputInteractionContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Waiting:
            case InputActionPhase.Canceled:
                if (context.ControlIsActuated(PressPointOrDefault)
                    && Time.time >= ignoreInputUntilTime - 0.1f)
                {
                    context.Started();
                    if (fireImmediately)
                    {
                        context.PerformedAndStayStarted();
                    }
                    ignoreInputUntilTime = Time.time + InitialPauseOrDefault;

                    // Check input again when the time elapsed or input changed.
                    context.SetTimeout(InitialPauseOrDefault);
                }
                break;

            case InputActionPhase.Started:
                if (!context.ControlIsActuated())
                {
                    Cancel(ref context);
                }
                else if (Time.time >= ignoreInputUntilTime - 0.1f)
                {
                    // Perform action but stay in the started phase, because we want to fire again after durationOrDefault 
                    context.PerformedAndStayStarted();
                    ignoreInputUntilTime = Time.time + RepeatedPauseOrDefault;

                    // Check input again when the time elapsed or input changed.
                    context.SetTimeout(RepeatedPauseOrDefault);
                }
                break;

            case InputActionPhase.Performed:
                if (!context.ControlIsActuated(PressPointOrDefault))
                {
                    Cancel(ref context);
                }
                break;
            default:
                if (!context.ControlIsActuated(PressPointOrDefault))
                {
                    Cancel(ref context);
                }
                break;
        }
    }

    private void Cancel(ref InputInteractionContext context)
    {
        ignoreInputUntilTime = 0;
        context.Canceled();
    }

    /// <inheritdoc />
    public void Reset()
    {
        // Method needed to implement interface
    }
}