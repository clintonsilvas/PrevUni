"use client";

import { useEffect, useState } from "react";
import { Instituicao } from "@/interfaces/types";
import styles from "./dashboard.module.css";

interface ModalIntegracaoProps {
  fechar: () => void;
  instituicao: Instituicao | null ;
}

export default function ModalIntegracao({
  fechar,
  instituicao,
}: ModalIntegracaoProps) {

  const [api, setApi] = useState("");
  const [erro, setErro] = useState("");

  const [verificando, setVerificando] = useState(false);
  const [sincronizando, setSincronizando] = useState(false);

  const [apiValida, setApiValida] = useState<boolean | null>(null);
  const [mensagem, setMensagem] = useState("");

  useEffect(() => {
    setApi(instituicao?.api ?? "");
    setApiValida(null);
    setMensagem("");
    setErro("");
  }, [instituicao]);

  async function verificaApi() {
    if (!instituicao) return;

    setVerificando(true);
    setErro("");
    setApiValida(null);
    setMensagem("");

    try {
      const resposta = await fetch(
        `/api/instituicoes/verificaApi/${instituicao.id}`
      );

      const json = await resposta.json();

      if (resposta.status === 401) {
        window.location.href = "/login";
        return;
      }

      if (!resposta.ok || !json.success) {
        throw new Error(
          json.message ?? "Erro ao verificar API."
        );
      }

      setApiValida(json.data.valida);
      setMensagem(json.data.mensagem);

    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Erro ao verificar API."
      );
    } finally {
      setVerificando(false);
    }
  }

  async function sincronizar() {
    if (!instituicao) return;

    setSincronizando(true);
    setErro("");
    setMensagem("Sincronizando dados...");

    try {
      const resposta = await fetch(
        `/api/sync/${instituicao.id}`,
        {
          method: "POST",
        }
      );

      const json = await resposta.json();

      if (resposta.status === 401) {
        window.location.href = "/login";
        return;
      }

      if (!resposta.ok || !json.success) {
        throw new Error(
          json.message ?? "Erro ao sincronizar dados."
        );
      }

      setMensagem(
        "Dados sincronizados com sucesso."
      );

    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Erro ao sincronizar dados."
      );
    } finally {
      setSincronizando(false);
    }
  }

  return (
    <div
      className={styles.overlay}
      onMouseDown={(event) => {
        if (
          event.target === event.currentTarget &&
          !verificando &&
          !sincronizando
        ) {
          fechar();
        }
      }}
    >
      <div className={styles.modal}>

        <div className={styles.modalHeader}>
          <div>
            <span className={styles.eyebrow}>
              INTEGRAÇÃO
            </span>

            <h2>
              {instituicao?.nome}
            </h2>

            <p>
              Verifique a API antes de sincronizar os dados.
            </p>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={fechar}
            disabled={verificando || sincronizando}
            aria-label="Fechar"
          >
            ×
          </button>
        </div>

        <div className={styles.modalForm}>

          <div className={styles.formGroup}>
            <label>
              API cadastrada
            </label>

            <input
              value={api}
              readOnly
            />
          </div>

          <button
            type="button"
            className={styles.cancelButton}
            onClick={verificaApi}
            disabled={verificando || sincronizando}
          >
            {verificando
              ? "Verificando..."
              : "Verificar API"}
          </button>

          {mensagem && (
            <p>
              {mensagem}
            </p>
          )}

          {erro && (
            <p>
              {erro}
            </p>
          )}

          {apiValida === false && (
            <div> 
              <a
                href="/documentacao"
                target="_blank"
                rel="noopener noreferrer"
              >
                Ver documentação da API
              </a>
            </div>
          )}

          {apiValida === true && (
            <div>             

              <button
                type="button"
                className={styles.saveButton}
                onClick={sincronizar}
                disabled={sincronizando}
              >
                {sincronizando
                  ? "Sincronizando..."
                  : "Sincronizar dados"}
              </button>

            </div>
          )}

          <div className={styles.modalActions}>
            <button
              type="button"
              className={styles.cancelButton}
              onClick={fechar}
              disabled={verificando || sincronizando}
            >
              Fechar
            </button>
          </div>

        </div>
      </div>
    </div>
  );
}