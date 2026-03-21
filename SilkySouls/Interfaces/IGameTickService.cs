// 

using System;

namespace SilkySouls.Interfaces;

public interface IGameTickService
{
    public void Subscribe(Action callback);
    public void Unsubscribe(Action callback);
}