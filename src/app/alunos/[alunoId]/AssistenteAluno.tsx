
"use client";

import { FormEvent, useState } from "react";
import styles from "./AssistenteAluno.module.css";

interface Mensagem {
  tipo: "usuario" | "ia";
  texto: string;
}

interface Props {
  alunoId: string;
}

export function AssistenteAluno({ alunoId }: Props) {
  const [mensagens, setMensagens] = useState<Mensagem[]>([
    {
      tipo: "ia",
      texto:
        "Olá! Posso ajudar a analisar os dados acadêmicos deste aluno. O que deseja saber?",
    },
  ]);

  const [pergunta, setPergunta] = useState("");
  const [carregando, setCarregando] = useState(false);

  async function enviarPergunta(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const texto = pergunta.trim();

    if (!texto || carregando) return;

    setMensagens((anteriores) => [
      ...anteriores,
      { tipo: "usuario", texto },
    ]);

    setPergunta("");
    setCarregando(true);

    try {
      const response = await fetch("/api/gemini", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          alunoId,
          prompt: texto,
        }),
      });

      const resultado = await response.json();

      if (!response.ok || !resultado.sucesso) {
        throw new Error(
          resultado.erro ?? "Não foi possível obter uma resposta."
        );
      }

      setMensagens((anteriores) => [
        ...anteriores,
        {
          tipo: "ia",
          texto: resultado.resposta,
        },
      ]);
    } catch (error) {
      setMensagens((anteriores) => [
        ...anteriores,
        {
          tipo: "ia",
          texto:
            error instanceof Error
              ? error.message
              : "Erro ao comunicar com a IA.",
        },
      ]);
    } finally {
      setCarregando(false);
    }
  }

  return (
    <section className={styles.container}>
      <header className={styles.header}>
        <div>
          <h2>Assistente IA</h2>
          <p>Análise acadêmica do aluno</p>
        </div>
        <span className={styles.status}>Gemini</span>
      </header>

      <div className={styles.mensagens} aria-live="polite">
        {mensagens.map((mensagem, index) => (
          <div
            key={index}
            className={
              mensagem.tipo === "usuario"
                ? styles.mensagemUsuario
                : styles.mensagemIA
            }
          >
            <span className={styles.autor}>
              {mensagem.tipo === "usuario" ? "Você" : "Assistente"}
            </span>
            <p>{mensagem.texto}</p>
          </div>
        ))}

        {carregando && (
          <div className={styles.mensagemIA}>
            <span className={styles.autor}>Assistente</span>
            <p>Analizando os dados...</p>
          </div>
        )}
      </div>

      <form className={styles.formulario} onSubmit={enviarPergunta}>
        <input
          value={pergunta}
          onChange={(event) => setPergunta(event.target.value)}
          placeholder="Pergunte sobre o desempenho do aluno..."
          aria-label="Pergunta para o assistente"
          disabled={carregando}
        />

        <button
          type="submit"
          disabled={carregando || !pergunta.trim()}
        >
          {carregando ? "Aguarde..." : "Enviar"}
        </button>
      </form>
    </section>
  );
}
