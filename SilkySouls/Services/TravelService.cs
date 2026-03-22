// 

using System.Numerics;
using SilkySouls.Interfaces;
using SilkySouls.Memory;

namespace SilkySouls.Services;

public class TravelService(IMemoryService memoryService, HookManager hookManager) : ITravelService
{
    public void Warp(int bonfireId)
    {
        
    }

    public void CustomWarp(Vector3 coords, float angle, int bonfireId)
    {
        throw new System.NotImplementedException();
    }

    public void UnlockBonfireWarps()
    {
        throw new System.NotImplementedException();
    }
}