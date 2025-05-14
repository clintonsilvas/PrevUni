using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Backend.Models
{
    public class AlunoLogs
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string user_id { get; set; }

        public List<LogUsuario> logs { get; set; }
    }
}
