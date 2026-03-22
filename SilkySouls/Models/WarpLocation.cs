using System.Numerics;

namespace SilkySouls.Models
{
    public class WarpLocation
    {
        public int Id { get; set; }
        public Vector3? Coords { get; set; }
        public float Angle { get; set; }
        public string Name { get; set; }
        public string MainArea { get; set; }
        
        public bool HasCoordinates => Coords.HasValue;
    }
}