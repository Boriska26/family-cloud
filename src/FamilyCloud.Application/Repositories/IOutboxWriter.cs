using FamilyCloud.Application.Photos.Events;

namespace FamilyCloud.Application.Repositories
{
    public interface IOutboxWriter
    {
        void Add(PhotoUploaded photoUploaded);
    }
}