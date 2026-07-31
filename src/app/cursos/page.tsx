import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";
import { CursoService } from "@/services/curso.service";
import CursosClient from "./CursosClient";

export default async function Cursos() {
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  const cursoService = new CursoService();
  const cursos = await cursoService.listarPorInstituicao(sessao.instituicaoId);

  return <CursosClient usuarioNome={sessao.nome} cursos={cursos} />;
}
