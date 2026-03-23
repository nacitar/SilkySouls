using System;
using SilkySouls.Enums;

namespace SilkySouls.Interfaces;

public interface IStateService
{
    public bool IsLoaded();
    public bool IsFading();
    void Publish(State eventType);
    void Subscribe(State eventType, Action handler);
    void Unsubscribe(State eventType, Action handler);
}