using CSMP.Tools;

namespace CSMP.Model
{
    public class Map
    {
        public Cuboid Limits => Cuboid.From(Components
            .Select(component => component.Cuboid)
            .WhereNotNull());

        public List<Component> Components { get; } = new();
    }

    public class Component
    {
        public Material Material { get; set; }
        public Cuboid Cuboid { get; set; }
    }
}
