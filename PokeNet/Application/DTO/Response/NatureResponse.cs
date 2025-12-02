using PokeNet.Application.DTO.External;
using System.Text.Json.Serialization;

namespace PokeNet.Application.DTO.Response
{
    public class NatureResponse
    {
        public string Nome { get; set; } = "";
        public string Aumenta { get; set; } = "";
        public string Diminui { get; set; } = "";
    }


    public class NatureDetailResponse
    {
        public string Name { get; set; } = "";

        [JsonPropertyName("increased_stat")]
        public NamedAPIResource? Increased { get; set; }

        [JsonPropertyName("decreased_stat")]
        public NamedAPIResource? Decreased { get; set; }
    }



    public class NatureListResponse
    {
        public int Count { get; set; }
        public string Next { get; set; }
        public string Previous { get; set; }
        public List<NatureListResult> Results { get; set; } = new();
    }

    public class NatureListResult
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }

}
