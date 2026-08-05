"use client";

import styles from "./dashboard.module.css";

interface ModalInstituicaoProps {
  fechar: () => void;
}

export default function ModalInstituicao({
  fechar,
}: ModalInstituicaoProps) {

  return (
    <div
      className={styles.overlay}
      onMouseDown={(event) => {

        if (event.target === event.currentTarget) {
          fechar();
        }

      }}
    >

      <div className={styles.modal}>

        <div className={styles.modalHeader}>

          <div>
            <span className={styles.eyebrow}>
              NOVA INSTITUIÇÃO
            </span>

            <h2>
              Adicionar instituição
            </h2>

            <p>
              Cadastre os dados básicos da instituição.
            </p>
          </div>

          <button
            type="button"
            className={styles.closeButton}
            onClick={fechar}
            aria-label="Fechar"
          >
            ×
          </button>

        </div>

        <form className={styles.modalForm}>

          <div className={styles.formGroup}>

            <label htmlFor="nome">
              Nome da instituição
            </label>

            <input
              id="nome"
              name="nome"
              type="text"
              placeholder="Ex.: Centro Universitário..."
              required
            />

          </div>

          <div className={styles.formRow}>

            <div className={styles.formGroup}>

              <label htmlFor="sigla">
                Sigla
              </label>

              <input
                id="sigla"
                name="sigla"
                type="text"
                placeholder="Ex.: UNIFENAS"
                required
              />

            </div>

            <div className={styles.formGroup}>

              <label htmlFor="cnpj">
                CNPJ
              </label>

              <input
                id="cnpj"
                name="cnpj"
                type="text"
                placeholder="00.000.000/0000-00"
              />

            </div>

          </div>

          <div className={styles.formRow}>

            <div className={styles.formGroup}>

              <label htmlFor="cidade">
                Cidade
              </label>

              <input
                id="cidade"
                name="cidade"
                type="text"
                placeholder="Ex.: Alfenas"
              />

            </div>

            <div className={styles.formGroup}>

              <label htmlFor="estado">
                Estado
              </label>

              <select
                id="estado"
                name="estado"
                defaultValue=""
              >
                <option value="" disabled>
                  Selecione
                </option>

                <option value="MG">
                  Minas Gerais
                </option>

                <option value="SP">
                  São Paulo
                </option>

                <option value="RJ">
                  Rio de Janeiro
                </option>

                <option value="PR">
                  Paraná
                </option>

              </select>

            </div>

          </div>

          <div className={styles.formGroup}>

            <label htmlFor="dominio">
              Domínio do PrevUni
            </label>

            <input
              id="dominio"
              name="dominio"
              type="text"
              placeholder="prevuni.faculdade.br"
              required
            />

            <small>
              Exemplo: prevuni.unifenas.br
            </small>

          </div>

          <div className={styles.formGroup}>

            <label htmlFor="api">
              Endpoint da API
            </label>

            <input
              id="api"
              name="api"
              type="url"
              placeholder="https://faculdade.br/api/prevuni"
              required
            />

            <small>
              Endpoint disponibilizado pela instituição.
            </small>

          </div>

          <div className={styles.modalActions}>

            <button
              type="button"
              className={styles.cancelButton}
              onClick={fechar}
            >
              Cancelar
            </button>

            <button
              type="submit"
              className={styles.saveButton}
            >
              Adicionar instituição
            </button>

          </div>

        </form>

      </div>

    </div>
  );
}