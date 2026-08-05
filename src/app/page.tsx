import Link from "next/link";
import styles from "./home.module.css";

export default function Home() {
  return (
    <main className={styles.page}>    
      <header className={styles.header}>
        <Link href="/" className={styles.logoLink}>
          <img
            src="/logo.svg"
            alt="PrevUni"
            className={styles.logo}
          />
        </Link>
        <nav className={styles.nav}>
          <Link href="#sobre">
            Sobre
          </Link>
          <Link href="#integracao">
            Integração
          </Link>
          <Link href="/documentacao">
            Documentação
          </Link>
          <Link
            href="/admin/login"
            className={styles.adminLink}
          >
            Administração
          </Link>
        </nav>
      </header>     

      <section className={styles.hero}>
        <div className={styles.heroContent}>
          <span className={styles.badge}>
            PREVENÇÃO DE EVASÃO NO ENSINO SUPERIOR
          </span>
          <h1>
            Antecipe a evasão.
            <br />
            <span>Transforme dados em ação.</span>
          </h1>

          <p>
            O PrevUni ajuda instituições de ensino superior
            a identificar alunos com risco de evasão e
            transformar dados acadêmicos em decisões mais
            estratégicas.
          </p>

          <div className={styles.heroButtons}>
            <Link
              href="#sobre"
              className={styles.primaryButton}
            >
              Conheça o PrevUni
            </Link>
            <Link
              href="/documentacao"
              className={styles.secondaryButton}
            >
              Ver documentação
            </Link>
          </div>
        </div>

        <div className={styles.heroVisual}>
          <div className={styles.dashboardCard}>
            <div className={styles.cardHeader}>
              <span>
                Monitoramento acadêmico
              </span>
              <span className={styles.status}>
                ● Atualizado
              </span>
            </div>

            <div className={styles.metric}>
              <div>
                <small>
                  Alunos monitorados
                </small>
                <strong>
                  2.481
                </strong>
              </div>

              <div>
                <small>
                  Risco de evasão
                </small>

                <strong>
                  12,4%
                </strong>
              </div>
            </div>


            <div className={styles.riskList}>
              <div>
                <span className={styles.dotHigh}></span>
                <span>
                  Alto risco
                </span>
                <strong>
                  47
                </strong>
              </div>
              <div>
                <span className={styles.dotMedium}></span>
                <span>
                  Médio risco
                </span>
                <strong>
                  126
                </strong>
              </div>

              <div>
                <span className={styles.dotLow}></span>
                <span>
                  Baixo risco
                </span>
                <strong>
                  2.308
                </strong>
              </div>
            </div>
          </div>
        </div>
      </section>


      {/* SOBRE */}

      <section
        id="sobre"
        className={styles.section}
      >
        <div className={styles.sectionTitle}>

          <span>
            O PREVUNI
          </span>

          <h2>
            Dados acadêmicos que ajudam
            a prevenir a evasão.
          </h2>

          <p>
            O PrevUni centraliza informações acadêmicas
            fornecidas pela instituição e apresenta aos
            gestores uma visão mais clara dos alunos que
            precisam de atenção.
          </p>

        </div>


        <div className={styles.features}>

          <article className={styles.feature}>

            <div className={styles.icon}>
              01
            </div>

            <h3>
              Integração
            </h3>

            <p>
              A instituição disponibiliza seus dados
              acadêmicos através de uma API seguindo
              o contrato definido pelo PrevUni.
            </p>

          </article>


          <article className={styles.feature}>

            <div className={styles.icon}>
              02
            </div>

            <h3>
              Organização
            </h3>

            <p>
              Os dados são sincronizados e organizados
              para que as informações possam ser
              consultadas pelo sistema.
            </p>

          </article>


          <article className={styles.feature}>

            <div className={styles.icon}>
              03
            </div>

            <h3>
              Prevenção
            </h3>

            <p>
              O sistema permite identificar situações
              que podem indicar risco de evasão e
              apoiar a tomada de decisão.
            </p>

          </article>

        </div>

      </section>


      {/* INTEGRAÇÃO */}

      <section
        id="integracao"
        className={styles.integration}
      >

        <div className={styles.integrationText}>

          <span>
            INTEGRAÇÃO
          </span>

          <h2>
            Sua instituição fornece os dados.
            O PrevUni faz o restante.
          </h2>

          <p>
            Para utilizar o PrevUni, a instituição
            disponibiliza um endpoint que retorna
            os dados necessários seguindo nosso
            contrato de integração.
          </p>

          <Link
            href="/documentacao"
            className={styles.primaryButton}
          >
            Ver contrato da API
          </Link>

        </div>


        <div className={styles.codeCard}>

          <div className={styles.codeTop}>

            <span></span>
            <span></span>
            <span></span>

          </div>

          <pre>
{`GET /get-data-prevuni

{
  "instituicao": {
    "id": "unifenas",
    "nome": "Universidade...",
    "sigla": "UNIFENAS"
  },

  "coordenadores": [
    {
      "id": "coord-001",
      "nome": "Coordenador",
      "email": "coordenador@faculdade.edu"
    }
  ],

  "professores": [...],

  "cursos": [...],

  "disciplinas": [...],

  "alunos": [...],

  "matriculas": [...]
}`}
          </pre>

        </div>

      </section>


      {/* FLUXO */}

      <section className={styles.flowSection}>

        <div className={styles.sectionTitle}>

          <span>
            ARQUITETURA
          </span>

          <h2>
            Uma integração simples.
          </h2>

        </div>


        <div className={styles.flow}>

          <div className={styles.flowItem}>

            <strong>
              01
            </strong>

            <h3>
              Instituição
            </h3>

            <p>
              Mantém seus dados acadêmicos
              em seus próprios sistemas.
            </p>

          </div>


          <div className={styles.arrow}>
            →
          </div>


          <div className={styles.flowItem}>

            <strong>
              02
            </strong>

            <h3>
              API
            </h3>

            <p>
              Disponibiliza os dados através
              do endpoint definido.
            </p>

          </div>


          <div className={styles.arrow}>
            →
          </div>


          <div className={styles.flowItem}>

            <strong>
              03
            </strong>

            <h3>
              PrevUni
            </h3>

            <p>
              Sincroniza, organiza e disponibiliza
              as informações para os gestores.
            </p>

          </div>

        </div>

      </section>


    {/* CTA */}

    <section className={styles.cta}>      
      <div className={styles.ctatxt}>
        <span>
          MULTI-TENANT
        </span>

        <h2>
          Cada instituição, seu próprio ambiente.
        </h2>
          <p>
          O PrevUni utiliza uma arquitetura multi-tenant,
          permitindo que cada instituição tenha seu próprio
          domínio e ambiente de acesso, mantendo seus dados
          organizados e isolados.
        </p>    

        <p>
          A instituição acessa o PrevUni através de seu próprio
          endereço, enquanto a plataforma identifica automaticamente
          a qual instituição aquele ambiente pertence.
        </p>
      </div>

      
      <div className={styles.domainExample}>
        <span>PrevUni</span>
        <strong>prevuni.sua-faculdade.br</strong>
      </div>
    </section>


      {/* FOOTER */}

      <footer className={styles.footer}>
        <div>
          <img
            src="/logo.svg"
            alt="PrevUni"
          />
          <p>
            Prevenção inteligente da evasão
            no ensino superior.
          </p>
        </div>


        <div className={styles.footerLinks}>
          <Link href="/documentacao">
            Documentação
          </Link>
          <Link href="/admin/login">
            Administração
          </Link>
        </div>
      </footer>
    </main>
  );
}