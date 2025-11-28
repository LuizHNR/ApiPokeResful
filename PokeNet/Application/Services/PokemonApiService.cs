using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;
using System.Net.Http.Json;
using System.Text.Json;

namespace PokeNet.Application.Services
{
    public class PokemonApiService
    {
        private readonly HttpClient _http;

        public PokemonApiService(HttpClient http)
        {
            _http = http;
        }




        //----------------------------
        // Buscar todos pokemons
        //----------------------------
        public async Task<List<PokemonListaResponse>> BuscarTodos(int page, int pageSize)
        {
            int offset = (page - 1) * pageSize;

            var document = await _http.GetFromJsonAsync<JsonDocument>(
                $"pokemon?limit={pageSize}&offset={offset}"
            );

            var results = document.RootElement
                .GetProperty("results")
                .EnumerateArray();

            var lista = new List<PokemonListaResponse>();

            foreach (var item in results)
            {
                var name = item.GetProperty("name").GetString()!;
                var url = item.GetProperty("url").GetString()!;
                int id = int.Parse(url.TrimEnd('/').Split('/').Last());

                var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>($"pokemon/{id}");

                if (detalhe == null)
                    continue;

                var finalName = char.ToUpper(name[0]) + name[1..];

                lista.Add(new PokemonListaResponse
                {
                    Numero = id,
                    Nome = finalName,
                    Tipos = detalhe.Types
                        .Select(t => t.Type.Name)
                        .ToList()
                });
            }

            return lista;
        }



        //----------------------------
        // Buscar pokemons
        //----------------------------
        public async Task<Pokemon?> BuscarPokemon(string nomeOuNumero)
        {
            try
            {
                var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>(
                    $"pokemon/{nomeOuNumero.ToLower()}"
                );

                if (detalhe == null)
                    return null;

                var evolucoes = await BuscarEvolucoes(detalhe.Id);

                var baseName = detalhe.Name.Split('-')[0];
                var finalName = char.ToUpper(baseName[0]) + baseName[1..];

                string cryUrl = $"https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/{detalhe.Id}.ogg";

                return new Pokemon
                {
                    Numero = detalhe.Id,
                    Nome = finalName,
                    CryUrl = cryUrl,
                    Altura = detalhe.Height,
                    Peso = detalhe.Weight,
                    Habilidades = detalhe.Abilities.Select(a => a.Ability.Name).ToList(),
                    Tipos = detalhe.Types.Select(t => t.Type.Name).ToList(),
                    Evolucoes = evolucoes,
                    Sprites = detalhe.Sprites
                };
            }
            catch
            {
                return null;
            }
        }




        //----------------------------
        // Buscar evoluções
        //----------------------------
        public async Task<List<PokemonEvolucaoDTO>> BuscarEvolucoes(int id)
        {
            var species = await _http.GetFromJsonAsync<PokemonSpecies>($"pokemon-species/{id}");
            if (species == null || string.IsNullOrWhiteSpace(species.Evolution_Chain.Url))
                return new List<PokemonEvolucaoDTO>();

            var evoChainId = species.Evolution_Chain.Url
                .TrimEnd('/')
                .Split('/')
                .Last();

            var chain = await _http.GetFromJsonAsync<EvolutionChainResponse>(
                $"evolution-chain/{evoChainId}"
            );

            if (chain == null)
                return new List<PokemonEvolucaoDTO>();

            var lista = new List<PokemonEvolucaoDTO>();
            ExtrairEvolucoes(chain.Chain, lista);
            return lista;
        }


        private void ExtrairEvolucoes(ChainLink link, List<PokemonEvolucaoDTO> lista)
        {
            // extrair número da pokedex a partir da URL da species
            string speciesUrl = link.Species.Url;
            int numero = int.Parse(speciesUrl.TrimEnd('/').Split('/').Last());

            // pegar nível para evoluir (se existir)
            int? minLevel = link.Evolution_Details.FirstOrDefault()?.Min_Level;

            lista.Add(new PokemonEvolucaoDTO
            {
                Numero = numero,
                Nome = Capitalizar(link.Species.Name),
                NivelParaEvoluir = minLevel,
                Links = new
                {
                    self = $"/api/v2/pokemon/{numero}"
                }
            });

            foreach (var e in link.Evolves_To)
                ExtrairEvolucoes(e, lista);
        }

        private string Capitalizar(string nome)
        {
            return char.ToUpper(nome[0]) + nome[1..];
        }



        private async Task<Dictionary<string, double>> ObterMultiplicadoresAsync(List<string> tiposPokemon)
        {
            var multipliers = new Dictionary<string, double>
            {
                ["normal"] = 1,
                ["fire"] = 1,
                ["water"] = 1,
                ["electric"] = 1,
                ["grass"] = 1,
                ["ice"] = 1,
                ["fighting"] = 1,
                ["poison"] = 1,
                ["ground"] = 1,
                ["flying"] = 1,
                ["psychic"] = 1,
                ["bug"] = 1,
                ["rock"] = 1,
                ["ghost"] = 1,
                ["dragon"] = 1,
                ["dark"] = 1,
                ["steel"] = 1,
                ["fairy"] = 1
            };

            foreach (var tipo in tiposPokemon)
            {
                var typeData = await _http.GetFromJsonAsync<TypeDetailResponse>($"type/{tipo}");

                foreach (var d in typeData.Damage_Relations.Double_Damage_From)
                    multipliers[d.Name] *= 2;

                foreach (var h in typeData.Damage_Relations.Half_Damage_From)
                    multipliers[h.Name] *= 0.5;

                foreach (var n in typeData.Damage_Relations.No_Damage_From)
                    multipliers[n.Name] = 0;
            }

            return multipliers;
        }


        public async Task<PokemonMultipliersResponse> ObterFraquezasEVantagens(List<string> tipos)
        {
            var all = await ObterMultiplicadoresAsync(tipos);

            return new PokemonMultipliersResponse
            {
                Fraquezas = all
                    .Where(x => x.Value > 1)
                    .ToDictionary(x => x.Key, x => x.Value),

                Resistencias = all
                    .Where(x => x.Value < 1 && x.Value > 0)
                    .ToDictionary(x => x.Key, x => x.Value),

                Imunidades = all
                    .Where(x => x.Value == 0)
                    .ToDictionary(x => x.Key, x => x.Value)
            };
        }


    }
}
