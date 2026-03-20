using System;
using SilkySouls.Enums;

namespace SilkySouls.Interfaces;

public interface IStateService
{
    public bool IsLoaded();
    void Publish(State eventType);
    void Subscribe(State eventType, Action handler);
    void Unsubscribe(State eventType, Action handler);
}