using Microsoft.AspNetCore.Mvc;

namespace MoodleFakeApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DadosController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var dados = new
            {
                Users = new List<UserMoodle>
                {
                    new UserMoodle { UserId = "U001", Name = "Alice Santos", UserLastAccess = DateTime.Parse("2025-05-06 21:12:30") },
                    new UserMoodle { UserId = "U002", Name = "Bruno Lima", UserLastAccess = DateTime.Parse("2025-05-05 14:23:47") },
                    new UserMoodle { UserId = "U003", Name = "Carla Mendes", UserLastAccess = DateTime.Parse("2025-05-04 19:45:02") },
                    new UserMoodle { UserId = "U004", Name = "Daniel Costa", UserLastAccess = DateTime.Parse("2025-05-03 09:01:12") },
                    new UserMoodle { UserId = "U005", Name = "Eduarda Souza", UserLastAccess = DateTime.Parse("2025-05-02 17:33:58") },
                    new UserMoodle { UserId = "U006", Name = "Felipe Rocha", UserLastAccess = DateTime.Parse("2025-04-30 11:49:00") },
                    new UserMoodle { UserId = "U007", Name = "Gisele Nunes", UserLastAccess = DateTime.Parse("2025-05-06 22:10:15") },
                    new UserMoodle { UserId = "U008", Name = "Henrique Dias", UserLastAccess = DateTime.Parse("2025-05-01 08:20:40") },
                    new UserMoodle { UserId = "U009", Name = "Isabela Torres", UserLastAccess = DateTime.Parse("2025-04-29 16:07:11") },
                    new UserMoodle { UserId = "U010", Name = "João Pedro", UserLastAccess = DateTime.Parse("2025-05-07 07:50:00") }
                },

                Logs = new List<LogMoodle>
                {
                    new LogMoodle { UserId = "U001", Name = "Alice Santos", Date = DateTime.Parse("2025-04-25 10:00"), Action = "viewed", Target = "course", Component = "core", CourseFullname = "Curso de Python", UserLastAccess = DateTime.Parse("2025-05-06 21:12:30") },
                    new LogMoodle { UserId = "U001", Name = "Alice Santos", Date = DateTime.Parse("2025-04-26 15:00"), Action = "submitted", Target = "assignment", Component = "mod_assign", CourseFullname = "Curso de Python", UserLastAccess = DateTime.Parse("2025-05-06 21:12:30") },

                    new LogMoodle { UserId = "U002", Name = "Bruno Lima", Date = DateTime.Parse("2025-04-20 09:30"), Action = "viewed", Target = "quiz", Component = "mod_quiz", CourseFullname = "Curso de Excel", UserLastAccess = DateTime.Parse("2025-05-05 14:23:47") },
                    new LogMoodle { UserId = "U002", Name = "Bruno Lima", Date = DateTime.Parse("2025-04-21 11:00"), Action = "attempted", Target = "quiz", Component = "mod_quiz", CourseFullname = "Curso de Excel", UserLastAccess = DateTime.Parse("2025-05-05 14:23:47") },

                    new LogMoodle { UserId = "U003", Name = "Carla Mendes", Date = DateTime.Parse("2025-04-18 13:10"), Action = "viewed", Target = "page", Component = "mod_page", CourseFullname = "Curso de Gestão", UserLastAccess = DateTime.Parse("2025-05-04 19:45:02") },
                    new LogMoodle { UserId = "U003", Name = "Carla Mendes", Date = DateTime.Parse("2025-04-19 14:40"), Action = "downloaded", Target = "resource", Component = "mod_resource", CourseFullname = "Curso de Gestão", UserLastAccess = DateTime.Parse("2025-05-04 19:45:02") },

                    new LogMoodle { UserId = "U004", Name = "Daniel Costa", Date = DateTime.Parse("2025-04-22 18:30"), Action = "posted", Target = "forum", Component = "mod_forum", CourseFullname = "Curso de Java", UserLastAccess = DateTime.Parse("2025-05-03 09:01:12") },
                    new LogMoodle { UserId = "U004", Name = "Daniel Costa", Date = DateTime.Parse("2025-04-23 20:00"), Action = "replied", Target = "forum", Component = "mod_forum", CourseFullname = "Curso de Java", UserLastAccess = DateTime.Parse("2025-05-03 09:01:12") },

                    new LogMoodle { UserId = "U005", Name = "Eduarda Souza", Date = DateTime.Parse("2025-04-27 07:45"), Action = "viewed", Target = "course_module", Component = "mod_lti", CourseFullname = "Curso de Banco de Dados", UserLastAccess = DateTime.Parse("2025-05-02 17:33:58") },

                    new LogMoodle { UserId = "U006", Name = "Felipe Rocha", Date = DateTime.Parse("2025-04-15 16:00"), Action = "viewed", Target = "book", Component = "mod_book", CourseFullname = "Curso de Linux", UserLastAccess = DateTime.Parse("2025-04-30 11:49:00") },

                    new LogMoodle { UserId = "U007", Name = "Gisele Nunes", Date = DateTime.Parse("2025-05-01 13:30"), Action = "submitted", Target = "quiz", Component = "mod_quiz", CourseFullname = "Curso de Power BI", UserLastAccess = DateTime.Parse("2025-05-06 22:10:15") },

                    new LogMoodle { UserId = "U008", Name = "Henrique Dias", Date = DateTime.Parse("2025-04-10 08:20"), Action = "downloaded", Target = "resource", Component = "mod_resource", CourseFullname = "Curso de Redes", UserLastAccess = DateTime.Parse("2025-05-01 08:20:40") },

                    new LogMoodle { UserId = "U009", Name = "Isabela Torres", Date = DateTime.Parse("2025-04-17 09:00"), Action = "viewed", Target = "course", Component = "core", CourseFullname = "Curso de Marketing Digital", UserLastAccess = DateTime.Parse("2025-04-29 16:07:11") },

                    new LogMoodle { UserId = "U010", Name = "João Pedro", Date = DateTime.Parse("2025-05-06 06:30"), Action = "submitted", Target = "assignment", Component = "mod_assign", CourseFullname = "Curso de Design Gráfico", UserLastAccess = DateTime.Parse("2025-05-07 07:50:00") },
                    new LogMoodle { UserId = "U010", Name = "João Pedro", Date = DateTime.Parse("2025-05-07 07:00"), Action = "viewed", Target = "course_module", Component = "mod_lti", CourseFullname = "Curso de Design Gráfico", UserLastAccess = DateTime.Parse("2025-05-07 07:50:00") }
                }
            };
            return Ok(dados);
        }
    }

}
