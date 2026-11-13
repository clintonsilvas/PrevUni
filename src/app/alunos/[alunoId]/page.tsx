import { DadosAluno } from "@/components/DadosAluno/DadosAluno";
import { HistoricoEvasao } from "@/components/HistoricoEvasao/HistoricoEvasao";

import Footer from "@/components/Footer/Footer";
import Header from "@/components/header/header.component";
import { AssistenteAluno } from "./AssistenteAluno";
import { SessaoService } from "@/services/sessao.service";
import { redirect } from "next/navigation";

import styles from "./page.module.css";

interface PageProps {
  params: Promise<{
    alunoId: string;
  }>;
}

export default async function AlunoPage({ params }: PageProps) {
  const { alunoId } = await params;
  const sessaoService = new SessaoService();
  const sessao = await sessaoService.obterSessao();

  if (!sessao) {
    redirect("/login");
  }

  return (
    <>
      <Header usuarioNome={sessao.nome} tipoUsuario={sessao.tipo} />
        <main className={styles.conteudo}>
          <div className={styles.dadosAluno}>
            <DadosAluno alunoId={alunoId} />
          </div>

          <div className={styles.historicoEvasao}>
            <HistoricoEvasao alunoId={alunoId} />
          </div>

          <AssistenteAluno alunoId={alunoId} />
        </main>
      <Footer></Footer>
    </>    
  );
}