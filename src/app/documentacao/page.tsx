import Link from "next/link";

import styles from "./documentacao.module.css";

export default function Documentacao() {

  return (

    <main className={styles.page}>

      {/* HEADER */}

      <header className={styles.header}>

        <Link
          href="/"
          className={styles.logoLink}
        >

          <img
            src="/logo.svg"
            alt="PrevUni"
            className={styles.logo}
          />

        </Link>


        <nav className={styles.nav}>

          <Link href="/">
            Início
          </Link>

          <Link
            href="#integracao"
            className={styles.active}
          >
            Documentação
          </Link>

          <Link href="#dns">
            Domínio e DNS
          </Link>

          <Link href="/admin/login">
            Administração
          </Link>

        </nav>

      </header>


      {/* HERO */}

      <section className={styles.hero}>

        <div>

          <span className={styles.badge}>
            DOCUMENTAÇÃO DE INTEGRAÇÃO
          </span>

          <h1>
            Integre sua instituição
            <br />
            ao <span>PrevUni.</span>
          </h1>

          <p>
            Consulte o contrato de integração, o formato
            dos dados esperados e as configurações necessárias
            para disponibilizar sua instituição no PrevUni.
          </p>

        </div>

      </section>


      {/* LAYOUT */}

      <div className={styles.documentation}>

        {/* SIDEBAR */}

        <aside className={styles.sidebar}>

          <strong>
            Nesta página
          </strong>

          <a href="#integracao">
            Integração
          </a>

          <a href="#endpoint">
            Endpoint
          </a>

          <a href="#resposta">
            Resposta esperada
          </a>

          <a href="#contrato">
            Contrato de dados
          </a>

          <a href="#regras">
            Regras
          </a>

          <a href="#dns">
            Domínio e DNS
          </a>

          <a href="#exemplo">
            Exemplo completo
          </a>

        </aside>


        {/* CONTENT */}

        <div className={styles.content}>


          {/* INTEGRAÇÃO */}

          <section id="integracao">

            <span className={styles.sectionLabel}>
              01 — INTEGRAÇÃO
            </span>

            <h2>
              Como uma instituição se integra?
            </h2>

            <p>
              O PrevUni utiliza uma integração baseada em API.
              A instituição continua mantendo seus dados em seus
              próprios sistemas e disponibiliza um endpoint para
              que o PrevUni possa consultar as informações
              necessárias.
            </p>

            <p>
              O endpoint deve retornar um objeto contendo
              informações da instituição, coordenadores,
              professores, cursos, disciplinas, alunos e
              matrículas.
            </p>

          </section>


          {/* ENDPOINT */}

          <section id="endpoint">

            <span className={styles.sectionLabel}>
              02 — ENDPOINT
            </span>

            <h2>
              Endpoint de integração
            </h2>

            <p>
              A instituição deve disponibilizar um endpoint
              HTTP acessível pelo PrevUni.
            </p>

            <div className={styles.endpoint}>

              <span className={styles.method}>
                GET
              </span>

              <code>
                https://sua-instituicao.br/api/prevuni
              </code>

            </div>

            <div className={styles.infoBox}>

              <strong>
                Exemplo
              </strong>

              <p>
                Durante o desenvolvimento, você pode utilizar
                um endpoint de teste como:
              </p>

              <code>
                https://prevuni-api.netlify.app/get-data-prevuni
              </code>

            </div>

          </section>


          {/* RESPONSE */}

          <section id="resposta">

            <span className={styles.sectionLabel}>
              03 — RESPOSTA
            </span>

            <h2>
              Resposta esperada
            </h2>

            <p>
              O endpoint deve retornar um JSON seguindo
              exatamente a estrutura abaixo:
            </p>

            <div className={styles.codeBlock}>

              <div className={styles.codeHeader}>

                <span>JSON</span>

                <span>
                  PrevUniData
                </span>

              </div>

              <pre>
{`{
  "instituicao": {
    "id": "unifenas",
    "nome": "Universidade de Exemplo",
    "sigla": "UNI",
    "cnpj": "00.000.000/0001-00",
    "cidade": "Alfenas",
    "estado": "MG",
    "api": "https://faculdade.br/api/prevuni",
    "dominio": "prevuni.faculdade.br"
  },

  "coordenadores": [],

  "professores": [],

  "cursos": [],

  "disciplinas": [],

  "alunos": [],

  "matriculas": []
}`}
              </pre>

            </div>

          </section>


          {/* CONTRATO */}

          <section id="contrato">

            <span className={styles.sectionLabel}>
              04 — CONTRATO
            </span>

            <h2>
              Estrutura dos dados
            </h2>

            <p>
              O PrevUni espera os seguintes objetos dentro
              da resposta da API.
            </p>


            {/* INSTITUICAO */}

            <div className={styles.dataCard}>

              <h3>
                Instituicao
              </h3>

              <p>
                Identifica a instituição que está fornecendo
                os dados.
              </p>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "nome": "string",
  "sigla": "string",
  "cnpj": "string",
  "cidade": "string",
  "estado": "string",
  "api": "string",
  "dominio": "string"
}`}
                </pre>

              </div>

            </div>


            {/* COORDENADOR */}

            <div className={styles.dataCard}>

              <h3>
                Coordenador
              </h3>

              <p>
                Representa os coordenadores acadêmicos
                disponibilizados pela instituição.
              </p>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "instituicaoId": "string",
  "nome": "string",
  "email": "string",
  "matricula": "string"
}`}
                </pre>

              </div>

            </div>


            {/* PROFESSOR */}

            <div className={styles.dataCard}>

              <h3>
                Professor
              </h3>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "instituicaoId": "string",
  "nome": "string",
  "email": "string",
  "matricula": "string"
}`}
                </pre>

              </div>

            </div>


            {/* CURSO */}

            <div className={styles.dataCard}>

              <h3>
                Curso
              </h3>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "nome": "string",
  "quantidadePeriodos": 8,
  "instituicaoId": "string",
  "coordenadorId": "string"
}`}
                </pre>

              </div>

            </div>


            {/* DISCIPLINA */}

            <div className={styles.dataCard}>

              <h3>
                Disciplina
              </h3>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "nome": "string",
  "periodo": 1,
  "cargaHoraria": 80,
  "cursoId": "string",
  "professorId": "string"
}`}
                </pre>

              </div>

            </div>


            {/* ALUNO */}

            <div className={styles.dataCard}>

              <h3>
                Aluno
              </h3>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "nome": "string",
  "cpf": "string",
  "email": "string",
  "dataNascimento": "2000-01-01",
  "cursoId": "string",
  "periodoAtual": 3,
  "status": "ativo",
  "dataIngresso": "2024-02-01"
}`}
                </pre>

              </div>

            </div>


            {/* MATRICULA */}

            <div className={styles.dataCard}>

              <h3>
                Matrícula
              </h3>

              <div className={styles.codeBlockSmall}>

                <pre>
{`{
  "id": "string",
  "alunoId": "string",
  "disciplinaId": "string",
  "semestre": "2026.1",
  "situacao": "matriculado",
  "cargaHorariaCumprida": 40,
  "nota": 8.5
}`}
                </pre>

              </div>

            </div>

          </section>


          {/* REGRAS */}

          <section id="regras">

            <span className={styles.sectionLabel}>
              05 — REGRAS
            </span>

            <h2>
              Regras de relacionamento
            </h2>

            <p>
              Os dados enviados precisam respeitar os
              relacionamentos abaixo.
            </p>

            <ul className={styles.rules}>

              <li>
                Uma instituição pode possuir um ou mais cursos.
              </li>

              <li>
                Uma instituição pode possuir um ou mais coordenadores.
              </li>

              <li>
                Uma instituição pode possuir um ou mais professores.
              </li>

              <li>
                Um curso deve possuir um coordenador.
              </li>

              <li>
                Um curso pode possuir uma ou mais disciplinas.
              </li>

              <li>
                Um curso pode possuir um ou mais alunos.
              </li>

              <li>
                Uma disciplina deve possuir um professor.
              </li>

              <li>
                Uma disciplina pode possuir uma ou mais matrículas.
              </li>

              <li>
                Uma matrícula deve estar associada a um aluno.
              </li>

            </ul>

          </section>


          {/* DNS */}

          <section id="dns">

            <span className={styles.sectionLabel}>
              06 — DOMÍNIO E DNS
            </span>

            <h2>
              Configure o domínio da instituição
            </h2>

            <p>
              Cada instituição possui seu próprio ambiente
              dentro do PrevUni. Para isso, a instituição
              deve configurar um subdomínio apontando para
              a infraestrutura onde o PrevUni está hospedado.
            </p>

            <div className={styles.domainExample}>

              <div>

                <small>
                  Domínio desejado
                </small>

                <strong>
                  prevuni.sua-faculdade.br
                </strong>

              </div>

            </div>


            <h3>
              Exemplo de configuração
            </h3>

            <p>
              O administrador responsável pelo domínio da
              instituição deverá criar o registro DNS
              solicitado pela equipe responsável pelo
              PrevUni.
            </p>


            <div className={styles.dnsTable}>

              <div className={styles.dnsRow}>

                <strong>
                  Tipo
                </strong>

                <strong>
                  Nome
                </strong>

                <strong>
                  Valor
                </strong>

              </div>


              <div className={styles.dnsRow}>

                <code>
                  CNAME
                </code>

                <code>
                  prevuni
                </code>

                <code>
                  seu-destino-do-prevuni
                </code>

              </div>

            </div>


            <div className={styles.warningBox}>

              <strong>
                Importante
              </strong>

              <p>
                O DNS não deve conter senhas, tokens,
                chaves privadas ou outras informações
                secretas. As configurações exatas de DNS
                podem variar conforme a infraestrutura de
                hospedagem utilizada pelo PrevUni.
              </p>

            </div>

          </section>


          {/* EXEMPLO */}

          <section id="exemplo">

            <span className={styles.sectionLabel}>
              07 — EXEMPLO COMPLETO
            </span>

            <h2>
              Exemplo de integração
            </h2>

            <p>
              Abaixo está um exemplo simplificado de uma
              resposta completa que uma instituição poderia
              fornecer.
            </p>

            <div className={styles.codeBlock}>

              <pre>
{`{
  "instituicao": {
    "id": "faculdade-001",
    "nome": "Faculdade Exemplo",
    "sigla": "FAEX",
    "cnpj": "00.000.000/0001-00",
    "cidade": "Alfenas",
    "estado": "MG",
    "api": "https://faculdade.br/api/prevuni",
    "dominio": "prevuni.faculdade.br"
  },

  "coordenadores": [
    {
      "id": "coord-001",
      "instituicaoId": "faculdade-001",
      "nome": "Maria Silva",
      "email": "maria@faculdade.br",
      "matricula": "123456"
    }
  ],

  "professores": [
    {
      "id": "prof-001",
      "instituicaoId": "faculdade-001",
      "nome": "João Souza",
      "email": "joao@faculdade.br",
      "matricula": "654321"
    }
  ],

  "cursos": [
    {
      "id": "curso-001",
      "nome": "Ciência da Computação",
      "quantidadePeriodos": 8,
      "instituicaoId": "faculdade-001",
      "coordenadorId": "coord-001"
    }
  ],

  "disciplinas": [
    {
      "id": "disc-001",
      "nome": "Programação",
      "periodo": 1,
      "cargaHoraria": 80,
      "cursoId": "curso-001",
      "professorId": "prof-001"
    }
  ],

  "alunos": [
    {
      "id": "aluno-001",
      "nome": "Aluno Exemplo",
      "cpf": "00000000000",
      "email": "aluno@faculdade.br",
      "dataNascimento": "2004-05-10",
      "cursoId": "curso-001",
      "periodoAtual": 1,
      "status": "ativo",
      "dataIngresso": "2026-02-01"
    }
  ],

  "matriculas": [
    {
      "id": "mat-001",
      "alunoId": "aluno-001",
      "disciplinaId": "disc-001",
      "semestre": "2026.1",
      "situacao": "matriculado",
      "cargaHorariaCumprida": 20,
      "nota": null
    }
  ]
}`}
              </pre>

            </div>

          </section>


          {/* FINAL */}

          <section className={styles.finalCta}>

            <h2>
              Pronto para integrar sua instituição?
            </h2>

            <p>
              Configure seu endpoint, siga o contrato
              de dados e entre em contato com a equipe
              do PrevUni para configurar seu ambiente.
            </p>

            <Link
              href="/admin/login"
              className={styles.primaryButton}
            >
              Acesso administrativo
            </Link>

          </section>


        </div>

      </div>


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

          <Link href="/">
            Início
          </Link>

          <Link href="#integracao">
            Integração
          </Link>

          <Link href="#dns">
            DNS
          </Link>

        </div>

      </footer>

    </main>

  );

}