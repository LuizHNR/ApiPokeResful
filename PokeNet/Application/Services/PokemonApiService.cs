using Microsoft.Extensions.Caching.Memory;
using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Request;
using PokeNet.Application.DTO.Response;
using PokeNet.Domain.Entities;
using System.Net.Http.Json;

namespace PokeNet.Application.Services
{
    public class PokemonApiService
    {
        private readonly HttpClient _http;
        private readonly IMemoryCache _cache;

        public PokemonApiService(IHttpClientFactory factory, IMemoryCache cache)
        {
            _http = factory.CreateClient("PokeApi");
            _cache = cache;
        }

        // DTO INTERNO PARA LISTAGEM
        private class PokemonListItem
        {
            public NamedAPIResource Item { get; set; } = null!;
            public int Id { get; set; }
        }

        // ────────────────────────────────────────────
        // CACHE AUXILIAR
        // ────────────────────────────────────────────
        private async Task<T> GetOrCreateAsync<T>(
            string key,
            TimeSpan ttl,
            Func<Task<T>> factory
        )
        {
            if (_cache.TryGetValue(key, out T value))
                return value;

            value = await factory();
            _cache.Set(key, value, ttl);
            return value;
        }

        // ────────────────────────────────────────────
        // INTERVALOS DE GERAÇÃO
        // ────────────────────────────────────────────
        private static readonly Dictionary<int, (int Min, int Max)> GenRanges = new()
        {
            {1, (1, 151)},
            {2, (152, 251)},
            {3, (252, 386)},
            {4, (387, 493)},
            {5, (494, 649)},
            {6, (650, 721)},
            {7, (722, 809)},
            {8, (810, 905)},
            {9, (906, 1025)}
        };

        // ────────────────────────────────────────────
        // LISTAGEM COM FILTROS
        // ────────────────────────────────────────────
        public async Task<PagedResponse<PokemonListaResponse>> BuscarTodos(
            PokemonFilterRequest filter
        )
        {
            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize is < 1 or > 100 ? 50 : filter.PageSize;

            var list = await GetOrCreateAsync(
                "pokemon_list_all",
                TimeSpan.FromHours(6),
                async () =>
                    await _http.GetFromJsonAsync<PokeApiListResponse>(
                        "pokemon?offset=0&limit=5000"
                    )
            );

            var filtrados = list.Results
                .Select(p => new PokemonListItem
                {
                    Item = p,
                    Id = int.Parse(p.Url.TrimEnd('/').Split('/').Last())
                })
                .ToList();

            // BUSCA POR NOME OU NÚMERO
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim().ToLower();
                filtrados = filtrados.Where(x =>
                    x.Item.Name.Contains(s) ||
                    x.Id.ToString().Contains(s)
                ).ToList();
            }

            // FILTRO POR GERAÇÕES
            if (filter.Generations.Any())
            {
                filtrados = filtrados.Where(x =>
                    filter.Generations.Any(gen =>
                        GenRanges.ContainsKey(gen) &&
                        x.Id >= GenRanges[gen].Min &&
                        x.Id <= GenRanges[gen].Max
                    )
                ).ToList();
            }

            // FILTRO POR TIPOS
            if (filter.Types.Any())
            {
                var temp = new List<PokemonListItem>();

                foreach (var p in filtrados)
                {
                    var detalhe = await BuscarPokemonDetalhe(p.Id);
                    if (detalhe == null) continue;

                    if (detalhe.Types.Any(t =>
                        filter.Types.Contains(t.Type.Name)
                    ))
                    {
                        temp.Add(p);
                    }
                }

                filtrados = temp;
            }

            // ORDENAÇÃO
            filtrados = filter.Order switch
            {
                "name_asc" => filtrados.OrderBy(x => x.Item.Name).ToList(),
                "name_desc" => filtrados.OrderByDescending(x => x.Item.Name).ToList(),
                "id_desc" => filtrados.OrderByDescending(x => x.Id).ToList(),
                _ => filtrados.OrderBy(x => x.Id).ToList()
            };

            int totalFiltrado = filtrados.Count;

            // PAGINAÇÃO
            var paged = filtrados
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<PokemonListaResponse>();

            foreach (var p in paged)
            {
                var detalhe = await BuscarPokemonDetalhe(p.Id);
                if (detalhe == null) continue;

                items.Add(new PokemonListaResponse
                {
                    Numero = p.Id,
                    Nome = Capitalizar(p.Item.Name),
                    Tipos = detalhe.Types.Select(t => t.Type.Name).ToList(),
                    Sprite = detalhe.Sprites
                });
            }

            return new PagedResponse<PokemonListaResponse>
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalFiltrado,
                TotalPages = (int)Math.Ceiling(totalFiltrado / (double)pageSize),
                Items = items
            };
        }

        // ────────────────────────────────────────────
        // DETALHE (CACHE)
        // ────────────────────────────────────────────
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