
using CitizenFX.Core;

namespace ScenarioCreatorShared
{
    public class Scenario
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 DefaultPosition { get; set; }

        public Scenario( int id, string name, Vector3 defaultPosition )
        {
            Id = id;
            Name = name;
            DefaultPosition = defaultPosition;
        }
    }
}
