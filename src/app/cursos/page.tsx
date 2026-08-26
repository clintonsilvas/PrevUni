import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";
import { CursoService } from "@/services/curso.service";
import CursosClient from "./CursosClient";

export default async function Dashboard() {
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  const cursoService = new CursoService();
  const cursos = await cursoService.listarPorSessao(sessao);

  return <CursosClient usuarioNome={sessao.nome} cursos={cursos} />;
}