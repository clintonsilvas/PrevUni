
"use client";

import { useEffect, useState } from "react";
import { Instituicao } from "@/interfaces/types";
import styles from "./dashboard.module.css";

interface ModalInstituicaoProps {
  fechar: () => void;
  salvo: () => Promise<void>;
  instituicao: Instituicao | null;
}

export default function ModalInstituicao({
  fechar,
  salvo,
  instituicao,
}: ModalInstituicaoProps) {
  const [nome, setNome] = useState("");
  const [sigla, setSigla] = useState("");
  const [cnpj, setCnpj] = useState("");
  const [cidade, setCidade] = useState("");
  const [estado, setEstado] = useState("");
  const [dominio, setDominio] = useState("");
  const [api, setApi] = useState("");
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState("");

  useEffect(() => {
    setNome(instituicao?.nome ?? "");
    setSigla(instituicao?.sigla ?? "");
    setCnpj(instituicao?.cnpj ?? "");
    setCidade(instituicao?.cidade ?? "");
    setEstado(instituicao?.estado ?? "");
    setDominio(instituicao?.dominio ?? "");
    setApi(instituicao?.api ?? "");
  }, [instituicao]);

  async function salvar(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSalvando(true);
    setErro("");

    try {
      const dados = {
        nome,
        sigla,
        cnpj,
        cidade,
        estado,
        dominio,
        api,
      };

      const resposta = await fetch(
        instituicao
          ? `/api/instituicoes/${instituicao.id}`
          : "/api/instituicoes",
        {
          method: instituicao ? "PUT" : "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(dados),
        }
      );

      const json = await resposta.json();

      if (resposta.status === 401) {
        window.location.href = "/login";
        return;
      }

      if (!resposta.ok || !json.success) {
        throw new Error(json.message ?? "Erro ao salvar instituição.");
      }

      await salvo();
    } catch (error) {
      setErro(
        error instanceof Error
          ? error.message
          : "Erro ao salvar instituição."
      );
    } finally {
      setSalvando(false);
    }
  }

  return (
    <div
      className={styles.overlay}
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !salvando) {
          fechar();
        }
      }}
    >
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <div>
            <span className={styles.eyebrow}>
              {instituicao ? "EDITAR INSTITUIÇÃO" : "NOVA INSTITUIÇÃO"}
            </span>

            <h2>
              {instituicao
                ? "Editar instituição"
                : "Adicionar instituição"}
            </h2>

            <p>
              {instituicao
                ? "Atualize os dados da instituição."
                : "Cadastre os dados básicos da instituição."}
            </p>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={fechar}
            disabled={salvando}
            aria-label="Fechar"
          >
            ×
          </button>
        </div>

        <form className={styles.modalForm} onSubmit={salvar}>
          <div className={styles.formGroup}>
            <label htmlFor="nome">Nome da instituição</label>
            <input
              id="nome"
              value={nome}
              onChange={(e) => setNome(e.target.value)}
              required
            />
          </div>

          <div className={styles.formRow}>
            <div className={styles.formGroup}>
              <label htmlFor="sigla">Sigla</label>
              <input
                id="sigla"
                value={sigla}
                onChange={(e) => setSigla(e.target.value)}
                required
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="cnpj">CNPJ</label>
              <input
                id="cnpj"
                value={cnpj}
                onChange={(e) => setCnpj(e.target.value)}
              />
            </div>
          </div>

          <div className={styles.formRow}>
            <div className={styles.formGroup}>
              <label htmlFor="cidade">Cidade</label>
              <input
                id="cidade"
                value={cidade}
                onChange={(e) => setCidade(e.target.value)}
              />
            </div>

            <div className={styles.formGroup}>
              <label htmlFor="estado">Estado</label>
              <select
                id="estado"
                value={estado}
                onChange={(e) => setEstado(e.target.value)}
              >
                <option value="">Selecione</option>
                <option value="MG">Minas Gerais</option>
                <option value="SP">São Paulo</option>
                <option value="RJ">Rio de Janeiro</option>
                <option value="PR">Paraná</option>
              </select>
            </div>
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="dominio">Domínio do PrevUni</label>
            <input
              id="dominio"
              value={dominio}
              onChange={(e) => setDominio(e.target.value)}
              placeholder="prevuni.faculdade.br"
              required
            />
          </div>

          <div className={styles.formGroup}>
            <label htmlFor="api">Endpoint da API</label>
            <input
              id="api"
              type="url"
              value={api}
              onChange={(e) => setApi(e.target.value)}
              placeholder="https://faculdade.br/api/prevuni"
              required
            />
          </div>

          {erro && <p role="alert">{erro}</p>}

          <div className={styles.modalActions}>
            <button
              type="button"
              className={styles.cancelButton}
              onClick={fechar}
              disabled={salvando}
            >
              Cancelar
            </button>

            <button
              type="submit"
              className={styles.saveButton}
              disabled={salvando}
            >
              {salvando
                ? "Salvando..."
                : instituicao
                  ? "Salvar alterações"
                  : "Adicionar instituição"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
