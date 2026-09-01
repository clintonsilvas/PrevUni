"use client";

import Footer from "@/components/Footer/Footer";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import Header from "@/components/header/header.component";
import CourseCard from "@/components/courseCard/courseCard.component";
import Button from "@/components/button/button.component";
import ExportMenu from "@/components/exportMenu/exportMenu.component";
import {
  LupaIcon,
  SearchButtonIcon,
  ChevronDownIcon,
} from "@/components/icons/icons";
import { CursoResumo } from "@/services/curso.service";
import "./cursos.style.css";

interface CursosClientProps {
  usuarioNome: string;
  tipoUsuario?: "coordenador" | "professor" | "admin";
}

type Ordenacao = "az" | "mais-alunos" | "mais-disciplinas";

const OPCOES_ORDENACAO: { valor: Ordenacao; label: string }[] = [
  { valor: "az", label: "(A-Z)" },
  { valor: "mais-alunos", label: "Quantidade de Alunos" },
  { valor: "mais-disciplinas", label: "Quantidade de Disciplinas" },
];

function CursosClient({ usuarioNome, tipoUsuario }: CursosClientProps) {
  const router = useRouter();

  const [cursos, setCursos] = useState<CursoResumo[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [busca, setBusca] = useState("");
  const [ordenacao, setOrdenacao] = useState<Ordenacao>("az");

  useEffect(() => {
    async function buscarCursos() {
      setCarregando(true);
      setErro(null);

      try {
        const resposta = await fetch("/api/cursos");

        if (resposta.status === 401) {
          router.push("/login");
          return;
        }

        const json = await resposta.json();

        if (!resposta.ok || !json.success) {
          throw new Error(
            json.message ?? "Não foi possível carregar os cursos."
          );
        }

        setCursos(json.data);
      } catch {
        setErro(
          "Não foi possível carregar os cursos. Tente novamente em instantes."
        );
      } finally {
        setCarregando(false);
      }
    }

    buscarCursos();
  }, [router]);

  const cursosFiltrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();

    const resultado = cursos.filter((curso) => {
      if (!termo) return true;
      return (
        curso.nome.toLowerCase().includes(termo) ||
        curso.coordenadorNome.toLowerCase().includes(termo)
      );
    });

    return [...resultado].sort((a, b) => {
      if (ordenacao === "mais-alunos") {
        return b.quantidadeAlunos - a.quantidadeAlunos;
      }
      if (ordenacao === "mais-disciplinas") {
        return b.quantidadeDisciplinas - a.quantidadeDisciplinas;
      }
      return a.nome.localeCompare(b.nome, "pt-BR");
    });
  }, [busca, cursos, ordenacao]);

  const colunasExport = useMemo(
    () => [
      { chave: "nome", label: "Curso" },
      { chave: "coordenadorNome", label: "Coordenador" },
      { chave: "quantidadeDisciplinas", label: "Disciplinas" },
      { chave: "quantidadeAlunos", label: "Alunos" },
      { chave: "quantidadeAlunosAtivos", label: "Alunos Ativos" },
      { chave: "engajamento", label: "Engajamento (%)" },
    ],
    []
  );

  const dadosExport = useMemo(
    () =>
      cursosFiltrados.map((curso) => ({
        nome: curso.nome,
        coordenadorNome: curso.coordenadorNome,
        quantidadeDisciplinas: curso.quantidadeDisciplinas,
        quantidadeAlunos: curso.quantidadeAlunos,
        quantidadeAlunosAtivos: curso.quantidadeAlunosAtivos,
        engajamento: curso.engajamento,
      })),
    [cursosFiltrados]
  );

  return (
    <>
      <Header usuarioNome={usuarioNome} tipoUsuario={tipoUsuario} />

      <main className="cursos-page">
        <div className="cursos-title-row">
          <h1 className="cursos-title">Cursos</h1>

          <ExportMenu
            nomeArquivo="cursos"
            colunas={colunasExport}
            dados={dadosExport}
          />
        </div>

        <div className="cursos-search">
          <div className="cursos-search-input">
            <LupaIcon size={20} />
            <input
              value={busca}
              onChange={(e) => setBusca(e.target.value)}
              placeholder="O que você procura?"
            />
          </div>

          <div className="cursos-search-button">
            <Button
              style="primary"
              content="Pesquisar"
              onClickButton={() => { }}
            />
          </div>

          <button
            type="button"
            className="cursos-search-submit-mobile"
            aria-label="Pesquisar"
          >
            <SearchButtonIcon size={40} />
          </button>
        </div>

        <div className="cursos-toolbar">
          <span className="cursos-count">
            {cursosFiltrados.length}{" "}
            {cursosFiltrados.length === 1 ? "Curso" : "Cursos"}
          </span>

          <div className="cursos-sort-desktop">
            <span>Ordenar por:</span>
            {OPCOES_ORDENACAO.map((opcao) => (
              <button
                key={opcao.valor}
                onClick={() => setOrdenacao(opcao.valor)}
                className={
                  ordenacao === opcao.valor
                    ? "cursos-sort-pill cursos-sort-pill-active"
                    : "cursos-sort-pill"
                }
              >
                {opcao.label}
              </button>
            ))}
          </div>

          <label className="cursos-sort-mobile">
            <span>Ordenar por:</span>
            <div className="cursos-sort-mobile-select">
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

        {carregando && <p className="cursos-status">Carregando cursos...</p>}

        {!carregando && erro && (
          <p className="cursos-status cursos-status-erro">{erro}</p>
        )}

        {!carregando && !erro && (
          <>
            <div className="cursos-grid">
              {cursosFiltrados.map((curso) => (
                <CourseCard key={curso.id} curso={curso} />
              ))}
            </div>

            {cursosFiltrados.length === 0 && (
              <p className="cursos-empty">
                Nenhum curso encontrado para essa busca.
              </p>
            )}
          </>
        )}
      </main>

      <Footer></Footer>
    </>
  );
}

export default CursosClient;