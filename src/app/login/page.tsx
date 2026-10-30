import { obterDominio } from "@/lib/dominio";
import { InstituicaoService } from "@/services/instituicao.service";
import LoginForm from "./LoginForm";

import styles from "./login.module.css";


export default async function Login() {

  const dominio = await obterDominio();
  const instituicaoService = new InstituicaoService();
  const instituicao =
    await instituicaoService.buscarPorDominio(dominio);

    if (!instituicao) {
    return (
      <div className={styles.errorContainer}>
        <img src="/logo.svg" alt="PrevUni" className={styles.logo} />

        <div className={styles.errorContent}>
          <h1>Instituição não encontrada</h1>

          <p>
            O domínio <strong>{dominio}</strong> não está vinculado
            a nenhuma instituição cadastrada no PrevUni.
          </p>

          <p>
            Verifique se o endereço foi digitado corretamente ou
            entre em contato com a instituição para confirmar o
            endereço de acesso.
          </p>

          <div className={styles.errorActions}>
            <a href="/" className={styles.primaryButton}>
              Ir para o PrevUni
            </a>

            <a href={dominio ? `https://${dominio}/login` : "/login"}
              className={styles.secondaryButton}>
              Tentar novamente
            </a>
          </div>
        </div>
      </div>
    );
  }
  return (
    <div className={styles.container}>
      <div className={styles.left}>
        <div className={styles.textform}>
          <img
            src="/logo.svg"
            alt="PrevUni"
            className={styles.logo}        />
          <h2>{instituicao.nome}</h2>

          <div className={styles.textlogin}>
            <h2>Faça seu Login</h2>
            <p>Insira seus dados abaixo!</p>
          </div>
        </div>
        <LoginForm
          instituicaoId={instituicao.id}
          styles={styles}
        />        
      </div>

      <div className={styles.right}></div>
    </div>
  );
}