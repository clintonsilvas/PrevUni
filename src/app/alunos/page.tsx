import { redirect } from "next/navigation";

import { SessaoService } from "@/services/sessao.service";
import { AlunoService } from "@/services/aluno.service";
import Header from "@/components/header/header.component";
import Footer from "@/components/Footer/Footer";
import AlunosClient from "./AlunosClient";

export default async function Alunos() {
  const sessaoService = new SessaoService();
  const alunoService = new AlunoService();

  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  const alunos = await alunoService.listarPorSessao(sessao);

  return (
    <main>
      <Header usuarioNome={sessao.nome} />
      <p> você é {sessao.tipo}</p>

      <AlunosClient alunos={alunos} />

      <Footer />
    </main>
  );
}