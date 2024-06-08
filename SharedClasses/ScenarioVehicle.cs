using CitizenFX.Core;

namespace ScenarioCreatorShared
{
    public class ScenarioVehicle
    {
        public int Id  { get; set; }
        public int ScenarioId  { get; set; }
        public string Model  { get; set; }
        public Vector3 Position  { get; set; }
        public Vector3 Rotation  { get; set; }
        public string Props  { get; set; }
        public string Plate  { get; set; }
        public int PedDriver  { get; set; }
        public string PedDriverMetadata  { get; set; }
    
        public ScenarioVehicle(
            int id, 
            int scenarioId,
            string model,
            Vector3 position,
            Vector3 rotation,
            string props,
            string plate,
            int pedDriver,
            string pedDriverMetadata
        )
        {
            Id = id;
            ScenarioId = scenarioId;
            Model = model;
            Position = position;
            Rotation = rotation;
            Props = props;
            Plate = plate;
            PedDriver = pedDriver;
            PedDriverMetadata = pedDriverMetadata;
        }
    
    }
}
