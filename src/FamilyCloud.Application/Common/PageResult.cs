namespace FamilyCloud.Application.Common
{
    public record PageResult<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize
        );
}