"use client";

import { useState } from "react";

import ModalInstituicao from "./ModalInstituicao";

import styles from "./dashboard.module.css";

interface Instituicao {
  id: string;
  nome: string;
  sigla: string;
  dominio: string;
  api: string;
  cidade: string;
  estado: string;
  ativa: boolean;
}

const instituicoesMock: Instituicao[] = [
  {
    id: "1",
    nome: "Centro Universitário de exemplo",
    sigla: "UNIFENAS",
    dominio: "prevuni.unifenas.br",
    api: "https://prevuni-api.netlify.app/get-data-prevuni",
    cidade: "Alfenas",
    estado: "MG",
    ativa: true,
  },
  {
    id: "2",
    nome: "Universidade Exemplo",
    sigla: "UNIV",
    dominio: "prevuni.univ.br",
    api: "https://universidade.br/api/prevuni",
    cidade: "São Paulo",
    estado: "SP",
    ativa: true,
  },
];

export default function Instituicoes() {

  const [modalAberto, setModalAberto] =
    useState(false);

  return (
    <>

      <div className={styles.toolbar}>

        <div className={styles.total}>
          <strong>
            {instituicoesMock.length}
          </strong>

          <span>
            instituições cadastradas
          </span>
        </div>

        <button
          type="button"
          className={styles.addButton}
          onClick={() => setModalAberto(true)}
        >
          <span>+</span>
          Adicionar instituição
        </button>

      </div>

      <div className={styles.cards}>

        {instituicoesMock.map((instituicao) => (

          <article
            key={instituicao.id}
            className={styles.card}
          >

            <div className={styles.cardTop}>

              <div className={styles.institutionIcon}>
                {instituicao.sigla.substring(0, 2)}
              </div>

              <div className={styles.cardTitle}>

                <h3>
                  {instituicao.nome}
                </h3>

                <span>
                  {instituicao.sigla}
                </span>

              </div>

              <span
                className={
                  instituicao.ativa
                    ? styles.statusActive
                    : styles.statusInactive
                }
              >
                <span className={styles.statusDot} />

                {instituicao.ativa
                  ? "Ativa"
                  : "Inativa"}
              </span>

            </div>

            <div className={styles.info}>

              <div>
                <span>
                  Domínio
                </span>

                <strong>
                  {instituicao.dominio}
                </strong>
              </div>

              <div>
                <span>
                  Localização
                </span>

                <strong>
                  {instituicao.cidade} - {instituicao.estado}
                </strong>
              </div>

            </div>

            <div className={styles.cardBottom}>

              <span className={styles.api}>
                API configurada
              </span>

              <button
                type="button"
                className={styles.manageButton}
              >
                Gerenciar
              </button>

            </div>

          </article>

        ))}

      </div>

      {modalAberto && (
        <ModalInstituicao
          fechar={() => setModalAberto(false)}
        />
      )}

    </>
  );
}