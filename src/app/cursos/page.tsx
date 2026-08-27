import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";
import CursosClient from "./CursosClient";

export default async function Dashboard() {
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  return <CursosClient usuarioNome={sessao.nome} />;
}