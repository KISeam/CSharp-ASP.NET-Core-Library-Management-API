using LibraryAPI.Domain.Common;
using LibraryAPI.Domain.Enums;

namespace LibraryAPI.Application.DTOs.Books;

public class BookQueryParameters : QueryParameters
{
    public Genre? Genre { get; set; }

    public int? AuthorId { get; set; }

    public bool OnlyAvailable { get; set; } = false;
}
