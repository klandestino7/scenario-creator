using CitizenFX.Core;

namespace ScenarioCreatorShared
{
    public class ScenarioProp
    {
        public int Id  { get; set; }
        public int ScenarioId  { get; set; }
        public string Model  { get; set; }
        public Vector3 Position  { get; set; }
        public Vector3 Rotation  { get; set; }
        public int AttachedToPedId  { get; set; }
        public string AttachedMetadata  { get; set; }

        public ScenarioProp( 
            int id, 
            int scenarioId,
            string model,
            Vector3 position,
            Vector3 rotation,
            int attachedToPedId,
            string attachedMetadata
        )
        {
            Id = id;
            ScenarioId = scenarioId;
            Model = model;
            Position = position;
            Rotation = rotation;
            AttachedToPedId = attachedToPedId;
            AttachedMetadata = attachedMetadata;
        }
    }
}
