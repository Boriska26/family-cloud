using FamilyCloud.Domain;

namespace FamilyCloud.Application.Services.FileStorage
{
    public record class FileSaveOptions(Guid Id, string Extension, PhotoVariant Variant);
}