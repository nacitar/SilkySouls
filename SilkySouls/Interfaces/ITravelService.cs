// 

using System.Numerics;

namespace SilkySouls.Interfaces;

public interface ITravelService
{
    void Warp(int bonfireId);
    void CustomWarp(Vector3 coords, float angle, int bonfireId);
    void UnlockBonfireWarps();
}