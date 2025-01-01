using UnityEngine;
using System.Collections.Generic;

public class InputManager : Singleton<InputManager>
{
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    private List<IInputHandler> inputHandlers = new List<IInputHandler>();

    public void RegisterHandler(IInputHandler handler)
    {
        if (!inputHandlers.Contains(handler))
            inputHandlers.Add(handler);
    }

    public void UnregisterHandler(IInputHandler handler)
    {
        inputHandlers.Remove(handler);
    }

    private void Update()
    {
        foreach (var handler in inputHandlers)
        {
            handler.HandleInput();
        }
    }
}
