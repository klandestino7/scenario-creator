using CitizenFX.Core;

namespace ScenarioCreatorShared
{
    public class ScenarioPed
    {
        public int Id  { get; set; }
        public int ScenarioId  { get; set; }
        public string Model  { get; set; }
        public Vector3 Position  { get; set; }
        public Vector3 Rotation  { get; set; }
        public int OutfitVariation  { get; set; }
        public bool IsFreezed  { get; set; }
        public bool IsInvincible  { get; set; }
        public string Scenario  { get; set; }
        public string Anim  { get; set; }
        public string Dict  { get; set; }
        public int Flags  { get; set; }
        public string Relationship  { get; set; }
        public string WeaponModel  { get; set; }

        public ScenarioPed( 
            int id, 
            int scenarioId,
            string model,
            Vector3 position,
            Vector3 rotation,
            int outfitVariation,
            bool isFreezed,
            bool isInvincible,
            string scenario,
            string anim,
            string dict,
            int flags,
            string relationship,
            string weaponModel
        )
        {
            Id = id;
            ScenarioId = scenarioId;
            Model = model;
            Position = position;
            Rotation = rotation;
            OutfitVariation = outfitVariation;
            IsFreezed = isFreezed;
            IsInvincible = isInvincible;
            Scenario = scenario;
            Anim = anim;
            Dict = dict;
            Flags = flags;
            Relationship = relationship;
            WeaponModel = weaponModel;
        }
    }
}
