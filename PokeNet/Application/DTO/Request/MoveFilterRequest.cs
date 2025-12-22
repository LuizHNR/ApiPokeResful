using PokeNet.Application.DTO.External;
using PokeNet.Application.DTO.Response;

namespace PokeNet.Application.DTO.Request
{

    public class MoveFilterRequest
    {
        /// <summary>
        /// level-up, machine, egg, tutor
        /// </summary>
        public List<string> Methods { get; set; } = new();

        /// <summary>
        /// Opcional: filtrar por tipo (fire, water...)
        /// </summary>
        public List<string> Types { get; set; } = new();

    }
}
