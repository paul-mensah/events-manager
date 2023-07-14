using System.ComponentModel.DataAnnotations;

namespace EventsManager.API.Models.Filters;

public class BaseFilter
{
    [Range(1, 100)] 
    public int PageSize { get; set; } = 10;

    [Range(1, int.MaxValue)] 
    public int CurrentPage { get; set; } = 1;
    public string OrderBy { get; set; } = "desc";
}