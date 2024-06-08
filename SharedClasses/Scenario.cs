
namespace ScenarioCreatorShared
{
    public class Scenario
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Scenario( int id, string name )
        {
            Id = id;
            Name = name;
        }
    }
}
