"use client";
import { useEffect, useMemo, useState } from "react";
import Link from "next/link";
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
interface Evasao {
  id: string;
  alunoId: string;
  probabilidadeEvasaoPercent: number;
  nivelRisco: "baixo" | "medio" | "alto";
}

interface AlunosClientProps {
  usuarioNome: string;
  tipoUsuario?: "coordenador" | "professor" | "admin";
}

export default function AlunosClient({ usuarioNome, tipoUsuario }: AlunosClientProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const cursoIdParam = searchParams.get("cursoId");
  const disciplinaIdParam = searchParams.get("disciplinaId");

  const [alunos, setAlunos] = useState<AlunoCompleto[]>([]);
  const [evasoes, setEvasoes] = useState<Evasao[]>([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState<string | null>(null);
  const [busca, setBusca] = useState("");
  const [ordenacao, setOrdenacao] = useState<Ordenacao>("az");

  // Nome alternativo do curso vindo da API caso nenhum aluno seja encontrado
  const [nomeCursoApi, setNomeCursoApi] = useState<string | null>(null);

  // Nome alternativo da disciplina vindo da API caso nenhum aluno seja encontrado
  const [nomeDisciplinaApi, setNomeDisciplinaApi] = useState<string | null>(null);

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

  useEffect(() => {
    async function buscarEvasoes() {
      try {
        const url = cursoIdParam
          ? `/api/cursos/evasao?cursoId=${cursoIdParam}`
          : "/api/alunos/evasao";

        const resposta = await fetch(url);

        const json = await resposta.json();

        console.log("STATUS:", resposta.status);
        console.log("URL:", url);
        console.log("RESPOSTA EVASÃO:", json);

        if (!resposta.ok || !json.success) {
          throw new Error(json.message ?? "Erro ao buscar evasões");
        }

        setEvasoes(json.data);
      } catch (error) {
        console.error("Erro ao buscar evasões:", error);
        setEvasoes([]);
      }
    }

    buscarEvasoes();
  }, [cursoIdParam]);

  // Busca o nome do curso na API se houver cursoIdParam
  useEffect(() => {
    if (!cursoIdParam) return;

    async function buscarNomeCurso() {
      try {
        const res = await fetch("/api/cursos");
        const json = await res.json();
        if (json.success) {
          const curso = json.data.find((c: { id: string; nome: string }) => c.id === cursoIdParam);
          if (curso) setNomeCursoApi(curso.nome);
        }
      } catch {
        // Falha silenciosa
      }
    }

    buscarNomeCurso();
  }, [cursoIdParam]);

  // Busca o nome da disciplina na API se houver disciplinaIdParam
  useEffect(() => {
    if (!disciplinaIdParam) return;

    async function buscarNomeDisciplina() {
      try {
        const res = await fetch("/api/disciplinas");
        const json = await res.json();
        if (json.success) {
          const disc = json.data.find((d: { id: string; nome: string }) => d.id === disciplinaIdParam);
          if (disc) setNomeDisciplinaApi(disc.nome);
        }
      } catch {
        // Falha silenciosa
      }
    }

    buscarNomeDisciplina();
  }, [disciplinaIdParam]);

  // Identifica o nome do curso filtrado (prioriza os alunos, fallback para a API de cursos)
  const nomeCursoFiltrado = useMemo(() => {
    if (!cursoIdParam) return null;
    const alunoComCurso = alunos.find((item) => item.curso?.id === cursoIdParam);
    return alunoComCurso?.curso?.nome ?? nomeCursoApi ?? "Curso não encontrado";
  }, [alunos, cursoIdParam, nomeCursoApi]);

  // Identifica o nome da disciplina filtrada (prioriza os alunos, fallback para a API de disciplinas)
  const nomeDisciplinaFiltrada = useMemo(() => {
    if (!disciplinaIdParam) return null;
    for (const item of alunos) {
      const discEncontrada = item.disciplinas.find(
        (d) => d.id === disciplinaIdParam
      );
      if (discEncontrada) return discEncontrada.nome;
    }
    return nomeDisciplinaApi ?? "Disciplina não encontrada";
  }, [alunos, disciplinaIdParam, nomeDisciplinaApi]);

  // Filtro por texto, cursoId ou disciplinaId
  const alunosFiltrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();

    const resultado = alunos.filter(({ aluno, curso, disciplinas }) => {
      // Filtro por cursoId
      if (cursoIdParam && curso?.id !== cursoIdParam) {
        return false;
      }
      // Filtro por disciplinaId
      if (
        disciplinaIdParam &&
        !disciplinas.some((disc) => disc.id === disciplinaIdParam)
      ) {
        return false;
      }

      // Filtro de busca
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
  }, [alunos, busca, ordenacao, cursoIdParam, disciplinaIdParam]);

  const colunasExport = useMemo(
    () => [
      { chave: "nome", label: "Aluno" },
      { chave: "email", label: "Email" },
      { chave: "evasao", label: "Probabilidade de Evasão" },
      { chave: "risco", label: "Risco" },
    ],
    []
  );

  const dadosExport = useMemo(
    () =>
      alunosFiltrados.map((item) => {
        const evasao = evasoes.find((e) => e.alunoId === item.aluno.id);

        return {
          nome: item.aluno.nome,
          email: item.aluno.email,
          evasao: evasao
            ? `${evasao.probabilidadeEvasaoPercent.toFixed(2)}%`
            : "-",
          risco: evasao
            ? evasao.nivelRisco.charAt(0).toUpperCase() + evasao.nivelRisco.slice(1)
            : "-",
        };
      }),
    [alunosFiltrados, evasoes]
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

        {/* Banner de Filtro padronizado igual Disciplinas */}
        {(cursoIdParam || disciplinaIdParam) && (
          <div className="alunos-filtro-ativo">
            <span>
              Mostrando alunos {cursoIdParam ? "do curso:" : "da disciplina:"}{" "}
              <strong>
                {cursoIdParam ? nomeCursoFiltrado : nomeDisciplinaFiltrada}
              </strong>
            </span>
            <Link href="/alunos">Limpar filtro</Link>
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
              onClickButton={() => { }}
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
              {alunosFiltrados.map((item) => {
                const evasao = evasoes.find(
                  (e) => e.alunoId === item.aluno.id
                );

                return (
                  <AlunoCard
                    key={item.aluno.id}
                    aluno={item.aluno}
                    curso={item.curso}
                    disciplinas={item.disciplinas}
                    evasao={evasao}
                    onClick={() => router.push(`/alunos/${item.aluno.id}`)}
                  />
                );
              })}
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