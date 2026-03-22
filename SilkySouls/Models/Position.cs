// 

using System.Numerics;

namespace SilkySouls.Models;

public class Position(int blockId, Vector3 coords, float angle)
{
    public uint BlockId;
    public Vector3 Coords;
    public float Angle;
}