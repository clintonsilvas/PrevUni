using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Security.Cryptography.X509Certificates;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public Cursos Cursos { get; set; } = new();
        public void OnGet(string nome, int quantidadeAlunos)
        {
            Cursos = new Cursos()
            {
                curso = nome,
                alunos = quantidadeAlunos

            };
        }
    }
}
