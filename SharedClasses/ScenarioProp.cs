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
        public _AttachedMetadata AttachedMetadata  { get; set; }

        public ScenarioProp( 
            int id, 
            int scenarioId,
            string model,
            Vector3 position,
            Vector3 rotation,
            int attachedToPedId,
            _AttachedMetadata attachedMetadata
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

    public class _AttachedMetadata 
    {
        public Vector3 Offset { get; set; }
        public Vector3 Rotation { get; set; }
        public bool HasCollision { get; set; }
        
        public _AttachedMetadata(
            Vector3 offset = new Vector3(),
            Vector3 rotation = new Vector3(),
            bool hasCollision = false
        )
        {
            Offset = offset;
            Rotation = rotation;
            HasCollision = hasCollision;
        }
    }
}
