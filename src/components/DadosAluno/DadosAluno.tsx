"use client";

import { useEffect, useState } from "react";
import { Aluno } from "@/interfaces/types";
import styles from "./DadosAluno.module.css";

interface Props {
  alunoId: string;
}

export function DadosAluno({ alunoId }: Props) {
  const [aluno, setAluno] = useState<Aluno | null>(null);
  const [carregando, setCarregando] = useState(true);

  useEffect(() => {
    async function buscarDadosAluno() {
      try {
        setCarregando(true);

        const resposta = await fetch(`/api/alunos/${alunoId}`);

        if (!resposta.ok) {
          setAluno(null);
          return;
        }

        setAluno(await resposta.json());
      } catch {
        setAluno(null);
      } finally {
        setCarregando(false);
      }
    }

    buscarDadosAluno();
  }, [alunoId]);

  if (carregando) {
    return <div className={styles.estado}>Carregando dados do aluno...</div>;
  }

  if (!aluno) {
    return <div className={styles.estado}>Não foi possível carregar os dados do aluno.</div>;
  }

  const formatarData = (data?: string | Date | null) => {
    if (!data) return "Não informado";

    const dataFormatada = new Date(data);

    if (Number.isNaN(dataFormatada.getTime())) return "Data inválida";

    return dataFormatada.toLocaleDateString("pt-BR", {
      timeZone: "UTC",
    });
  };

  const campos = [
    { titulo: "ID do aluno", valor: aluno.id },
    { titulo: "E-mail", valor: aluno.email },
    { titulo: "CPF", valor: aluno.cpf },
    { titulo: "Curso", valor: aluno.cursoId },
    { titulo: "Data de nascimento", valor: formatarData(aluno.dataNascimento) },
    { titulo: "Data de ingresso", valor: formatarData(aluno.dataIngresso) },
    { titulo: "Período atual", valor: aluno.periodoAtual },
  ];

  return (
    <section className={styles.container}>
      <div className={styles.cabecalho}>
        <div className={styles.avatar}>
          {aluno.nome?.charAt(0).toUpperCase() ?? "A"}
        </div>

        <div className={styles.identificacao}>
          <h2>{aluno.nome}</h2>
          <p>{aluno.email}</p>
          <span className={styles.id}>ID: {aluno.id}</span>
        </div>

        <span
          className={`${styles.status} ${
            aluno.status === "ativo" ? styles.ativo : styles.inativo
          }`}
        >
          {aluno.status}
        </span>
      </div>

      <div className={styles.divisor} />

      <div className={styles.secaoTitulo}>
        <h3>Dados pessoais e acadêmicos</h3>
        <p>Informações cadastrais do estudante</p>
      </div>

      <div className={styles.grade}>
        {campos.map((campo) => (
          <div className={styles.campo} key={campo.titulo}>
            <span>{campo.titulo}</span>
            <strong>{campo.valor ?? "Não informado"}</strong>
          </div>
        ))}
      </div>
    </section>
  );
}