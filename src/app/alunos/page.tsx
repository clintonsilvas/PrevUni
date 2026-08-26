import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";


export default async function Alunos() {
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  //get fetch api/aluno

  return <p>

    //components header
    //

    //  lista de alunos 
    //icone aluno 
    //nome aluno


    


  </p>  
}