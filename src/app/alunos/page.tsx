import { redirect } from "next/navigation";
import { SessaoService } from "@/services/sessao.service";
import AlunosClient from "./AlunosClient";

export default async function Alunos() {
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  return (
    <AlunosClient usuarioNome={sessao.nome}  tipoUsuario={sessao.tipo}
    />
  );
}