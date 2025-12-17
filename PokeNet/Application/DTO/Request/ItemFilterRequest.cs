using PokeNet.Application.DTO.Response;

namespace PokeNet.Application.DTO.Request
{

    public class ItemFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string? Search { get; set; }

    }

    public class ListItemFilter
    {
        public ItemListResult Item { get; set; } = null!;
        public int Id { get; set; }
    }


}
