using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;

        // AGORA CORRETO: IHttpClientFactory → client nomeado "PokeApi"
        public PokemonApiService(IHttpClientFactory factory, IMemoryCache cache)
        {
            _http = factory.CreateClient("PokeApi");
            _cache = cache;
        }

        // Função auxiliar para cache
        private async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory)
        {
            if (_cache.TryGetValue(key, out T value))
                return value;

            value = await factory();
            _cache.Set(key, value, ttl);
            return value;
        }

        // ────────────────────────────────────────────
        // Buscar TODOS os pokemons (3000+) COM CACHE
        // ────────────────────────────────────────────
        public async Task<List<PokemonListaResponse>> BuscarTodos()
        {
            return await GetOrCreateAsync(
                "todos_pokemons_cache",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var document = await _http.GetFromJsonAsync<JsonDocument>("pokemon?limit=3000");
                    var results = document.RootElement.GetProperty("results").EnumerateArray();

                    var lista = new List<PokemonListaResponse>();

                    foreach (var item in results)
                    {
                        var name = item.GetProperty("name").GetString()!;
                        var url = item.GetProperty("url").GetString()!;
                        int id = int.Parse(url.TrimEnd('/').Split('/').Last());

                        var detalhe = await BuscarPokemonDetalhe(id);
                        if (detalhe == null) continue;

                        var finalName = char.ToUpper(name[0]) + name[1..];

                        lista.Add(new PokemonListaResponse
                        {
                            Numero = id,
                            Nome = finalName,
                            Tipos = detalhe.Types.Select(t => t.Type.Name).ToList(),
                            Sprite = detalhe.Sprites
                        });
                    }

                    return lista;
                }
            );
        }

        // Cache do detalhe do Pokémon
        private async Task<PokemonApiDetail?> BuscarPokemonDetalhe(int id)
        {
            return await GetOrCreateAsync(
                $"pokemon_detail_{id}",
                TimeSpan.FromHours(24),
                async () =>
                    await _http.GetFromJsonAsync<PokemonApiDetail>($"pokemon/{id}")
            );
        }

        // ────────────────────────────────────────────
        // Buscar Pokémon individual (COM CACHE)
        // ────────────────────────────────────────────
        public async Task<Pokemon?> BuscarPokemon(string nomeOuNumero)
        {
            return await GetOrCreateAsync(
                $"pokemon_completo_{nomeOuNumero}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    try
                    {
                        var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>(
                            $"pokemon/{nomeOuNumero.ToLower()}"
                        );

                        if (detalhe == null)
                            return null;

                        var evolucoes = await BuscarEvolucoes(detalhe.Id);
                        var habilidades = await BuscarHabilidades(detalhe);
                        var (descricao, eggGroups) = await BuscarDescricaoEEggGroups(detalhe.Id);

                        var baseName = detalhe.Name.Split('-')[0];
                        var finalName = char.ToUpper(baseName[0]) + baseName[1..];

                        string cryUrl =
                            $"https://raw.githubusercontent.com/PokeAPI/cries/main/cries/pokemon/latest/{detalhe.Id}.ogg";

                        return new Pokemon
                        {
                            Numero = detalhe.Id,
                            Nome = finalName,
                            CryUrl = cryUrl,
                            Altura = detalhe.Height,
                            Peso = detalhe.Weight,
                            Habilidades = habilidades,
                            Descricao = descricao,
                            EggGroups = eggGroups,
                            Tipos = detalhe.Types.Select(t => t.Type.Name).ToList(),
                            Evolucoes = evolucoes,
                            Sprites = detalhe.Sprites,
                            Stats = detalhe.Stats.Select(s => new PokemonStatResponse
                            {
                                Nome = s.Stat.Name,
                                Valor = s.StatusBase
                            }).ToList()
                        };
                    }
                    catch
                    {
                        return null;
                    }
                }
            );
        }

        // ────────────────────────────────────────────
        // EVOLUÇÕES (COM CACHE)
        // ────────────────────────────────────────────
        public async Task<List<PokemonEvolucaoDTO>> BuscarEvolucoes(int id)
        {
            return await GetOrCreateAsync(
                $"evolucoes_{id}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var species = await _http.GetFromJsonAsync<PokemonSpecies>($"pokemon-species/{id}");
                    if (species == null || string.IsNullOrWhiteSpace(species.Evolution_Chain.Url))
                        return new List<PokemonEvolucaoDTO>();

                    var evoChainId = species.Evolution_Chain.Url.TrimEnd('/').Split('/').Last();

                    var chain = await _http.GetFromJsonAsync<EvolutionChainResponse>(
                        $"evolution-chain/{evoChainId}"
                    );

                    if (chain == null) return new List<PokemonEvolucaoDTO>();

                    var lista = new List<PokemonEvolucaoDTO>();
                    await ExtrairEvolucoes(chain.Chain, lista);

                    return lista;
                }
            );
        }

        private async Task ExtrairEvolucoes(ChainLink link, List<PokemonEvolucaoDTO> lista)
        {
            string speciesUrl = link.Species.Url;
            int numero = int.Parse(speciesUrl.TrimEnd('/').Split('/').Last());
            int? minLevel = link.Evolution_Details.FirstOrDefault()?.Min_Level;

            var detalhe = await BuscarPokemonDetalhe(numero);

            lista.Add(new PokemonEvolucaoDTO
            {
                Numero = numero,
                Nome = Capitalizar(link.Species.Name),
                NivelParaEvoluir = minLevel,
                Sprite = detalhe?.Sprites ?? new PokemonSprite(),
                Links = new { self = $"/api/v2/pokemon/{numero}" }
            });

            foreach (var e in link.Evolves_To)
                await ExtrairEvolucoes(e, lista);
        }

        private string Capitalizar(string nome)
        {
            return char.ToUpper(nome[0]) + nome[1..];
        }

        // ────────────────────────────────────────────
        // TIPO (DAMAGE RELATIONS) COM CACHE
        // ────────────────────────────────────────────
        private async Task<TypeDetailResponse> BuscarTipo(string tipo)
        {
            return await GetOrCreateAsync(
                $"tipo_{tipo}",
                TimeSpan.FromHours(24),
                async () => await _http.GetFromJsonAsync<TypeDetailResponse>($"type/{tipo}")
            );
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
                var typeData = await BuscarTipo(tipo);

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
                Fraquezas = all.Where(x => x.Value > 1).ToDictionary(x => x.Key, x => x.Value),
                Resistencias = all.Where(x => x.Value < 1 && x.Value > 0).ToDictionary(x => x.Key, x => x.Value),
                Imunidades = all.Where(x => x.Value == 0).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        // ────────────────────────────────────────────
        // DESCRIÇÃO + EGG GROUPS (SPECIES) COM CACHE
        // ────────────────────────────────────────────
        private async Task<(string descricao, List<string> eggs)> BuscarDescricaoEEggGroups(int id)
        {
            return await GetOrCreateAsync(
                $"species_{id}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var species = await _http.GetFromJsonAsync<PokemonSpeciesApi>($"pokemon-species/{id}");
                    if (species == null) return ("", new());

                    var entry = species.FlavorTextEntries.FirstOrDefault(x => x.Language.Name == "en");
                    string descricao = entry?.FlavorText?.Replace("\n", " ")?.Replace("\f", " ") ?? "";

                    var eggs = species.EggGroups.Select(e => e.Name).ToList();

                    return (descricao, eggs);
                }
            );
        }

        // ────────────────────────────────────────────
        // HABILIDADES (COM CACHE)
        // ────────────────────────────────────────────
        private async Task<List<PokemonHabilidadeResponse>> BuscarHabilidades(PokemonApiDetail detalhe)
        {
            var lista = new List<PokemonHabilidadeResponse>();

            foreach (var ability in detalhe.Abilities)
            {
                var data = await GetOrCreateAsync(
                    $"ability_{ability.Ability.Name}",
                    TimeSpan.FromHours(24),
                    async () =>
                        await _http.GetFromJsonAsync<AbilityDetailResponse>(ability.Ability.Url)
                );

                var desc = data?.EffectEntries
                    .FirstOrDefault(e => e.Language.Name == "en")
                    ?.ShortEffect ?? "";

                lista.Add(new PokemonHabilidadeResponse
                {
                    Nome = ability.Ability.Name,
                    Descricao = desc
                });
            }

            return lista;
        }

        // ────────────────────────────────────────────
        // MOVIMENTOS (COM CACHE)
        // ────────────────────────────────────────────
        public async Task<PokemonMovesResponse?> BuscarTodosMovimentos(string nomeOuNumero)
        {
            return await GetOrCreateAsync(
                $"moves_{nomeOuNumero}",
                TimeSpan.FromHours(24),
                async () =>
                {
                    var detalhe = await _http.GetFromJsonAsync<PokemonApiDetail>($"pokemon/{nomeOuNumero.ToLower()}");
                    if (detalhe == null) return null;

                    var resposta = new PokemonMovesResponse();

                    foreach (var moveSlot in detalhe.Moves)
                    {
                        var moveName = moveSlot.Move.Name;

                        var moveDetail = await GetOrCreateAsync(
                            $"move_detail_{moveName}",
                            TimeSpan.FromHours(24),
                            async () =>
                                await _http.GetFromJsonAsync<MoveDetailResponse>($"move/{moveName}")
                        );

                        foreach (var version in moveSlot.Version_Group_Details)
                        {
                            var metodo = version.MoveLearnMethod.Name;
                            var level = version.Level;

                            switch (metodo)
                            {
                                case "level-up":
                                    var efeito = moveDetail.EffectEntries.FirstOrDefault(e => e.Language.Name == "en");

                                    resposta.LevelUp.Add(new PokemonLevelUpMove
                                    {
                                        Nome = moveName,
                                        Level = level,
                                        Tipo = moveDetail.type.Name,
                                        Categoria = moveDetail.DamageClass.Name,
                                        Poder = moveDetail.power,
                                        Accuracy = moveDetail.accuracy,
                                        PP = moveDetail.pp,
                                        Efeito = efeito?.Effect ?? "",
                                        EfeitoCurto = efeito?.ShortEffect ?? "",
                                        ChanceEfeito = moveDetail.EffectChance
                                    });
                                    break;

                                case "machine":
                                    resposta.Machine.Add(MapearMove(moveName, moveDetail));
                                    break;

                                case "tutor":
                                    resposta.Tutor.Add(MapearMove(moveName, moveDetail));
                                    break;

                                case "egg":
                                    resposta.Egg.Add(MapearMove(moveName, moveDetail));
                                    break;

                                default:
                                    resposta.Outros.Add(moveName);
                                    break;
                            }
                        }
                    }

                    resposta.LevelUp = resposta.LevelUp
                        .GroupBy(x => new { x.Nome, x.Level })
                        .Select(g => g.First())
                        .OrderBy(x => x.Level)
                        .ToList();

                    resposta.Machine = resposta.Machine.Distinct().ToList();
                    resposta.Tutor = resposta.Tutor.Distinct().ToList();
                    resposta.Egg = resposta.Egg.Distinct().ToList();
                    resposta.Outros = resposta.Outros.Distinct().ToList();

                    return resposta;
                }
            );
        }

        private PokemonMoveDetail MapearMove(string nome, MoveDetailResponse move)
        {
            var efeito = move.EffectEntries.FirstOrDefault(e => e.Language.Name == "en");

            return new PokemonMoveDetail
            {
                Nome = nome,
                Tipo = move.type.Name,
                Categoria = move.DamageClass.Name,
                Poder = move.power,
                Accuracy = move.accuracy,
                PP = move.pp,
                Efeito = efeito?.Effect ?? "",
                EfeitoCurto = efeito?.ShortEffect ?? "",
                ChanceEfeito = move.EffectChance
            };
        }
    }
}
