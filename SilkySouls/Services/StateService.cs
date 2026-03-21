using System;
using System.Collections.Generic;
using SilkySouls.Enums;
using SilkySouls.Interfaces;
using SilkySouls.memory;

namespace SilkySouls.Services;

public class StateService(IMemoryService memoryService) : IStateService
{
    private readonly Dictionary<State, List<Action>> _eventHandlers = new();

    public bool IsLoaded()
    {
        var loadingCheckPtr = memoryService.FollowPointers(
            memoryService.Read<nint>(Offsets.MenuMan.Base),
            [(int)Offsets.MenuMan.MenuManData.LoadedFlag
        ], false);
        return memoryService.Read<int>(loadingCheckPtr) == 1;
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