using System.Text.Json;
using FamilyCloud.Application.Photos.Events;
using FamilyCloud.Application.Repositories;
using FamilyCloud.Infrastructure.DataAccess;
using FamilyCloud.Infrastructure.DataAccess.Outbox;

namespace FamilyCloud.Infrastructure.Repositories
{
    public class OutboxWriter : IOutboxWriter
    {
        private readonly FamilyCloudDbContext _dbContext;

        public OutboxWriter(FamilyCloudDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Add(PhotoUploaded photoUploaded)
        {
            string eventType = nameof(PhotoUploaded);
            string payload = JsonSerializer.Serialize(photoUploaded);
            OutboxMessage outboxMessage = new OutboxMessage(eventType, payload);
            
            _dbContext.Add(outboxMessage);
        }
    }
}