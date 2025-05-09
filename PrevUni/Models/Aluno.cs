using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PrevUni.Models
{
    public class Aluno
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("idAluno")]
        public string IdAluno { get; set; }

        public string Nome { get; set; }
        public string Curso { get; set; }
        public string NomeCurso { get; set; }
        public string Disciplina { get; set; }
        public string CoordenadorCurso { get; set; }
        public string Professor { get; set; }

        public DateTime UltimoAcesso { get; set; }
        public FrequenciaAcesso FrequenciaAcesso { get; set; }
        public EntregasAtrasadas EntregasAtrasadas { get; set; }
        public Interacao Interacao { get; set; }
        public Desempenho Desempenho { get; set; }

        public AnaliseIA AnaliseIA { get; set; } // preenchido pela IA
    }

    public class FrequenciaAcesso
    {
        public int Total { get; set; }
        public int Ultimos7 { get; set; }
        public int Ultimos15 { get; set; }
        public int Ultimos30 { get; set; }
    }

    public class EntregasAtrasadas
    {
        public int ComAtraso { get; set; }
        public int Total { get; set; }
    }

    public class Interacao
    {
        public int MensagensEnviadas { get; set; }
        public int MensagensRecebidas { get; set; }
    }

    public class Desempenho
    {
        public List<double> Atividades { get; set; }
        public double Media { get; set; }
    }

    public class AnaliseIA
    {
        public bool RiscoEvasao { get; set; }
        public string Justificativa { get; set; }
    }
}
