"use client";

import { useMemo, useState } from "react";
import "./alunos.styles.css"

import {
  LupaIcon,
  SearchButtonIcon,
  ChevronDownIcon,
} from "@/components/icons/icons";

type Ordenacao =
  | "az"
  | "mais-disciplinas";

const OPCOES_ORDENACAO: {
  valor: Ordenacao;
  label: string;
}[] = [
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

interface Props {
  alunos: AlunoCompleto[];
}

export default function AlunosClient({ alunos }: Props) {
  const [busca, setBusca] = useState("");
  const [ordenacao, setOrdenacao] =
    useState<Ordenacao>("az");

  const alunosFiltrados = useMemo(() => {
    const termo = busca.toLowerCase().trim();

    const filtrados = alunos.filter(({ aluno, curso }) => {
      if (!termo) return true;

      return (
        aluno.nome.toLowerCase().includes(termo) ||
        aluno.email.toLowerCase().includes(termo) ||
        curso?.nome.toLowerCase().includes(termo)
      );
    });

    return [...filtrados].sort((a, b) => {
      if (ordenacao === "az") {
        return a.aluno.nome.localeCompare(
          b.aluno.nome,
          "pt-BR"
        );
      }

      if (ordenacao === "mais-disciplinas") {
        return (
          b.disciplinas.length -
          a.disciplinas.length
        );
      }

      return 0;
    });
  }, [alunos, busca, ordenacao]);

  return (
    <main className="alunos-page">

      <div className="alunos-title-row">
        <h1 className="alunos-title">
          Alunos
        </h1>
      </div>

      {/* Busca */}
      <div className="alunos-search">

        <div className="alunos-search-input">
          <LupaIcon size={20} />

          <input
            value={busca}
            onChange={(e) =>
              setBusca(e.target.value)
            }
            placeholder="O que você procura?"
          />
        </div>

        <div className="alunos-search-button">
          <button
            type="button"
            onClick={() => {}}
          >
            Pesquisar
          </button>
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
          {alunosFiltrados.length === 1
            ? "Aluno"
            : "Alunos"}
        </span>

        {/* Desktop */}
        <div className="alunos-sort-desktop">
          <span>Ordenar por:</span>

          {OPCOES_ORDENACAO.map((opcao) => (
            <button
              key={opcao.valor}
              onClick={() =>
                setOrdenacao(opcao.valor)
              }
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
              onChange={(e) =>
                setOrdenacao(
                  e.target.value as Ordenacao
                )
              }
            >
              {OPCOES_ORDENACAO.map((opcao) => (
                <option
                  key={opcao.valor}
                  value={opcao.valor}
                >
                  {opcao.label}
                </option>
              ))}
            </select>

            <ChevronDownIcon size={14} />
          </div>
        </label>
      </div>

      {/* Lista */}
      <div className="alunos-grid">

        {alunosFiltrados.map(
          ({ aluno, curso, disciplinas }) => (
            <div
              key={aluno.id}
              className="aluno-card"
            >
              <div className="aluno-card-icon">
                👤
              </div>

              <div className="aluno-card-info">

                <h2>{aluno.nome}</h2>

                <p>{aluno.email}</p>

                <span>
                  {curso?.nome ??
                    "Curso não informado"}
                </span>

                <span>
                  {disciplinas.length}{" "}
                  {disciplinas.length === 1
                    ? "disciplina"
                    : "disciplinas"}
                </span>

              </div>
            </div>
          )
        )}

      </div>

      {alunosFiltrados.length === 0 && (
        <p className="alunos-empty">
          Nenhum aluno encontrado para essa busca.
        </p>
      )}

    </main>
  );
}