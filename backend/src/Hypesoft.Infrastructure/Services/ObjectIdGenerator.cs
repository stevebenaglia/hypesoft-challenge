using Hypesoft.Domain.Services;
using MongoDB.Bson;

namespace Hypesoft.Infrastructure.Services;

public sealed class ObjectIdGenerator : IIdGenerator
{
    public string NewId() => ObjectId.GenerateNewId().ToString();
}
