using System;
using System.Collections.Generic;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using static SilkySouls.memory.Offsets;

namespace SilkySouls.Services;

public class StateService(IMemoryService memoryService) : IStateService
{
    private readonly Dictionary<State, List<Action>> _eventHandlers = new();

    public bool IsLoaded()
    {
        var worldChrMan = memoryService.Read<nint>(WorldChrMan.Base);
        return memoryService.Read<nint>(worldChrMan + WorldChrMan.PlayerIns) != 0;
    }

    public bool IsFading()
    {
        var menuMan = memoryService.Read<nint>(MenuMan.Base);
        return memoryService.Read<int>(menuMan + MenuMan.IsFadeActive) == 1;
    }

    public void Publish(State eventType)
    {
        if (_eventHandlers.ContainsKey(eventType))
        {
            foreach (var handler in _eventHandlers[eventType])
                handler.Invoke();
        }
    }

    public void Subscribe(State eventType, Action handler)
    {
        if (!_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType] = new List<Action>();

        _eventHandlers[eventType].Add(handler);
    }

    public void Unsubscribe(State eventType, Action handler)
    {
        if (_eventHandlers.ContainsKey(eventType))
            _eventHandlers[eventType].Remove(handler);
    }
}