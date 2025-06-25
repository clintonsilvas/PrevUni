﻿using System.Text.Json.Serialization;



namespace Front.Models
{
    public class Acoes
    {
        public string nome_acao { get; set; }
        public string action { get; set; }
        public string target { get; set; }
        public string component { get; set; }


        public List<Acoes> ListarAcoes()
        {
            return new List<Acoes>
    {
        new Acoes { nome_acao = "Atividade", action = "graded", target = "user", component = "core" },
        new Acoes { nome_acao = "Fórum", action = "uploaded", target = "assessable", component = "mod_forum" },
        new Acoes { nome_acao = "Quizz", action = "submitted", target = "attempt", component = "mod_quiz" },

        //new Acoes { nome_acao = "Aval Final?", actionv = "updated", target = "course_module_completion", component = "core" },
         
        //new Acoes { nome_acao = "Visualizar Disciplina", action = "viewed", target = "course", component = "core" },
        //new Acoes { nome_acao = "Visualizar Tela com todos os Emblemas", action = "viewed", target = "badge_listing", component = "core" },
        //new Acoes { nome_acao = "Visualizar Tela com todos os Alunos do Curso", action = "viewed", target = "user_list", component = "core" },
        //new Acoes { nome_acao = "Visualizar perfil de um Aluno", action = "viewed", target = "user_profile", component = "core" },
        //new Acoes { nome_acao = "Visualizar Tela de Notas", action = "viewed", target = "grade_report", component = "gradereport_grader" },
        //new Acoes { nome_acao = "Visualizar Atividade", action = "viewed", target = "course_module", component = "mod_lti" },
        //new Acoes { nome_acao = "Visualizar Fórum", action = "viewed", target = "course_module", component = "mod_forum" },
        //new Acoes { nome_acao = "Visualizou Respostas do Fórum", action = "viewed", target = "discussion", component = "mod_forum" },

        //new Acoes { nome_acao = "Quizz", action = "started", target = "attempt", component = "mod_quiz" },  
        //new Acoes { nome_acao = "Clicou em Recurso (PDF, vídeo...)", action = "viewed", target = "course_module", component = "mod_resource" },
        //new Acoes { nome_acao = "Clicou em Link de Unidade", action = "viewed", target = "course_module", component = "mod_url" },
        //new Acoes { nome_acao = "Criou Resposta no Fórum", action = "created", target = "discussion", component = "mod_forum" },
        //new Acoes { nome_acao = "Criou Assinatura de Discussão", action = "created", target = "discussion_subscription", component = "mod_forum" },
        //new Acoes { nome_acao = "Enviou Resposta no Fórum", action = "uploaded", target = "assessable", component = "mod_forum" },
        //new Acoes { nome_acao = "Postou em Discussão do Fórum", action = "created", target = "post", component = "mod_forum" }
    };
        }



        public List<Acoes> ListarAcoes_paraIA()
        {
            return new List<Acoes>
    {
        new Acoes { nome_acao = "Atividade", action = "graded", target = "user", component = "core" },
        new Acoes { nome_acao = "Fórum", action = "uploaded", target = "assessable", component = "mod_forum" },
        new Acoes { nome_acao = "Quizz", action = "submitted", target = "attempt", component = "mod_quiz" },

        //new Acoes { nome_acao = "Visualizou Disciplina", action = "viewed", target = "course", component = "core" },
        //new Acoes { nome_acao = "Visualizou Tela com todos os Emblemas", action = "viewed", target = "badge_listing", component = "core" },
        //new Acoes { nome_acao = "Visualizou Tela com todos os Alunos do Curso", action = "viewed", target = "user_list", component = "core" },
        //new Acoes { nome_acao = "Visualizou perfil de um Aluno", action = "viewed", target = "user_profile", component = "core" },
        //new Acoes { nome_acao = "Visualizou Tela de Notas", action = "viewed", target = "grade_report", component = "gradereport_grader" },
        new Acoes { nome_acao = "Visualizou Atividade", action = "viewed", target = "course_module", component = "mod_lti" },
        new Acoes { nome_acao = "Visualizou Fórum", action = "viewed", target = "course_module", component = "mod_forum" },
        new Acoes { nome_acao = "Visualizou Respostas do Fórum", action = "viewed", target = "discussion", component = "mod_forum" },

        new Acoes { nome_acao = "Acessou Recurso (PDF, vídeo...)", action = "viewed", target = "course_module", component = "mod_resource" },
        new Acoes { nome_acao = "Acessou em Link de Unidade", action = "viewed", target = "course_module", component = "mod_url" },
        //new Acoes { nome_acao = "Criou Resposta no Fórum", action = "created", target = "discussion", component = "mod_forum" },
        //new Acoes { nome_acao = "Enviou Resposta no Fórum", action = "uploaded", target = "assessable", component = "mod_forum" },
        //new Acoes { nome_acao = "Postou em Discussão do Fórum", action = "created", target = "post", component = "mod_forum" }
    };
        }

    }


    public class AlunoEngajamento
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = "";

        [JsonPropertyName("nome")]
        public string Name { get; set; } = "";

        [JsonPropertyName("engajamento")]
        public double Engajamento { get; set; }
    }



    public class AcaoResumo
    {
        public string Acao { get; set; } = "";
        public int Quantidade { get; set; }
    }
}
