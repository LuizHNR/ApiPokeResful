namespace PokeNet.Application.DTO.External
{
    public class TypeDetailResponse
    {
        public DamageRelations Damage_Relations { get; set; } = new();
    }

    public class DamageRelations
    {
        public List<TypeName> Double_Damage_From { get; set; } = new();
        public List<TypeName> Half_Damage_From { get; set; } = new();
        public List<TypeName> No_Damage_From { get; set; } = new();
    }

    public class TypeName
    {
        public string Name { get; set; } = "";
    }

}
