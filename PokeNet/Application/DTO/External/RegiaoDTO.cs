namespace PokeNet.Application.DTO.External
{

    public class RegionLocationCompleteDTO
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }


    public class RegionListResponse
    {
        public int Count { get; set; }
        public List<RegionListItem> Results { get; set; } = new();
    }

    public class RegionListItem
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class RegionDetailResponse
    {
        public string Name { get; set; } = "";
        public List<LocationName> Locations { get; set; } = new();
        public MainGeneration Generation { get; set; } = new();
        public List<RegionLocationCompleteDTO> LocationsComplete { get; set; } = new();
    }

    public class LocationName
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }

    public class MainGeneration
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }




    public class LocationDetailResponse
    {
        public int Id { get; set; }
        public List<LocalizedName> Names { get; set; } = new();
        public List<AreaRef> Areas { get; set; } = new();
    }

    public class LocalizedName
    {
        public string Name { get; set; } = "";
        public LanguageRef Language { get; set; } = new();
    }

    public class LanguageRef
    {
        public string Name { get; set; } = "";
    }


    public class LocationAreaDetailResponse
    {
        public int Id { get; set; }
        public List<FlavorText> Flavor_Text_Entries { get; set; } = new();
    }

    public class FlavorText
    {
        public string Flavor_Text { get; set; } = "";
        public LanguageRef Language { get; set; } = new();
    }


    public class AreaRef
    {
        public string Name { get; set; } = "";
        public string Url { get; set; } = "";
    }


}
