"use client";
import { useEffect, useMemo, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import AlunoCard from "@/components/alunoCard/alunoCard.component"; 
import Header from "@/components/header/header.component";
import Footer from "@/components/Footer/Footer";
import Button from "@/components/button/button.component";
import ExportMenu from "@/components/exportMenu/exportMenu.component";
import {
  LupaIcon,
  SearchButtonIcon,
  ChevronDownIcon,
} from "@/components/icons/icons";
import "./alunos.styles.css";

type Ordenacao = "az" | "mais-disciplinas";

const OPCOES_ORDENACAO: { valor: Ordenacao; label: string }[] = [
  { valor: "az", label: "(A-Z)" },
  { valor: "mais-disciplinas", label: "Quantidade de Disciplinas" },
];

interface AlunoCompleto {
  aluno: {
    id: string;
    nome: string;
    email: string;
  };
  curso: {
    id: string;
    nome: string;
  } | null;
  coordenador: unknown | null;
  disciplinas: {
    id: string;
    nome: string;
  }[];
  matriculas: unknown[];
}

interface AlunosClientProps {
  usuarioNome: string;
  tipoUsuario?: "coordenador" | "professor" | "admin";
}

export default function AlunosClient({ usuarioNome, tipoUsuario }: AlunosClientProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const cursoIdParam = searchParams.get("cursoId");

  const [alunos, setAlunos] = useState<AlunoCompleto[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [busca, setBusca] = useState("");
  const [ordenacao, setOrdenacao] = useState<Ordenacao>("az");

  // Busca via API padronizada
  useEffect(() => {
    async function buscarAlunos() {
      setCarregando(true);
      setErro(null);

      try {
        const resposta = await fetch("/api/alunos");

        if (resposta.status === 401) {
          router.push("/login");
          return;
        }

        const json = await resposta.json();

        if (!resposta.ok || !json.success) {
          throw new Error(json.message ?? "Não foi possível carregar os alunos.");
        }

        setAlunos(json.data);
      } catch {
        setErro("Não foi possível carregar os alunos. Tente novamente em instantes.");
      } finally {
        setCarregando(false);
      }
    }

    buscarAlunos();
  }, [router]);

  // Identifica o nome do curso filtrado pelo ID na URL
  const nomeCursoFiltrado = useMemo(() => {
    if (!cursoIdParam) return null;
    const alunoComCurso = alunos.find((item) => item.curso?.id === cursoIdParam);
    return alunoComCurso?.curso?.nome ?? "Curso Selecionado";
  }, [alunos, cursoIdParam]);

  // Filtro por texto e por cursoId da URL
  const alunosFiltrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();

    const resultado = alunos.filter(({ aluno, curso }) => {
      // Filtro da URL por cursoId
      if (cursoIdParam && curso?.id !== cursoIdParam) {
        return false;
      }

      // Filtro do campo de busca
      if (!termo) return true;
      return (
        aluno.nome.toLowerCase().includes(termo) ||
        aluno.email.toLowerCase().includes(termo) ||
        curso?.nome.toLowerCase().includes(termo)
      );
    });

    return [...resultado].sort((a, b) => {
      if (ordenacao === "mais-disciplinas") {
        return b.disciplinas.length - a.disciplinas.length;
      }
      return a.aluno.nome.localeCompare(b.aluno.nome, "pt-BR");
    });
  }, [alunos, busca, ordenacao, cursoIdParam]);

  const limparFiltroCurso = () => {
    router.push("/alunos");
  };

  const colunasExport = useMemo(
    () => [
      { chave: "nome", label: "Aluno" },
      { chave: "email", label: "Email" },      
    ],
    []
  );

  const dadosExport = useMemo(
    () =>
      alunosFiltrados.map((aluno) => ({
        nome: aluno.aluno.nome,   
        email: aluno.aluno.email,   
      })),
    [alunosFiltrados]
  );

  return (
    <>
      <Header usuarioNome={usuarioNome} tipoUsuario={tipoUsuario} />

      <main className="alunos-page">
        <div className="alunos-title-row">
          <h1 className="alunos-title">Alunos</h1>
          <ExportMenu
            nomeArquivo="alunos"
            colunas={colunasExport}
            dados={dadosExport}
          />
        </div>

        {/* Banner do Filtro de Curso vindo da URL */}
        {cursoIdParam && (
          <div className="alunos-filtro-aviso" style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "16px" }}>
            <span>Mostrando alunos do curso: <strong>{nomeCursoFiltrado}</strong></span>
            <Button
              style="secondary"
              content="Limpar filtro"
              onClickButton={limparFiltroCurso}
            />
          </div>
        )}

        {/* Busca */}
        <div className="alunos-search">
          <div className="alunos-search-input">
            <LupaIcon size={20} />
            <input
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="O que você procura?"
            />
          </div>

          <div className="alunos-search-button">
            <Button
              style="primary"
              content="Pesquisar"
              onClickButton={() => {}}
            />
          </div>

          <button
            type="button"
            className="alunos-search-submit-mobile"
            aria-label="Pesquisar"
          >
            <SearchButtonIcon size={40} />
          </button>
        </div>

        {/* Toolbar */}
        <div className="alunos-toolbar">
          <span className="alunos-count">
            {alunosFiltrados.length}{" "}
            {alunosFiltrados.length === 1 ? "Aluno" : "Alunos"}
          </span>

          {/* Desktop */}
          <div className="alunos-sort-desktop">
            <span>Ordenar por:</span>
            {OPCOES_ORDENACAO.map((opcao) => (
              <button
                key={opcao.valor}
                onClick={() => setOrdenacao(opcao.valor)}
                className={
                  ordenacao === opcao.valor
                    ? "alunos-sort-pill alunos-sort-pill-active"
                    : "alunos-sort-pill"
                }
              >
                {opcao.label}
              </button>
            ))}
          </div>

          {/* Mobile */}
          <label className="alunos-sort-mobile">
            <span>Ordenar por:</span>
            <div className="alunos-sort-mobile-select">
              <select
                value={ordenacao}
                onChange={(e) => setOrdenacao(e.target.value as Ordenacao)}
              >
                {OPCOES_ORDENACAO.map((opcao) => (
                  <option key={opcao.valor} value={opcao.valor}>
                    {opcao.label}
                  </option>
                ))}
              </select>
              <ChevronDownIcon size={14} />
            </div>
          </label>
        </div>

        {/* Estados de Carregamento e Lista */}
        {carregando && <p className="alunos-status">Carregando alunos...</p>}

        {!carregando && erro && (
          <p className="alunos-status alunos-status-erro">{erro}</p>
        )}

        {!carregando && !erro && (
          <>
            <div className="alunos-grid">
              {alunosFiltrados.map((item) => (
                <AlunoCard
                  key={item.aluno.id}
                  aluno={item.aluno}
                  curso={item.curso}
                  disciplinas={item.disciplinas}
                />
              ))}
            </div>

            {alunosFiltrados.length === 0 && (
              <p className="alunos-empty">
                Nenhum aluno encontrado para essa busca.
              </p>
            )}
          </>
        )}
      </main>

      <Footer />
    </>
  );
}