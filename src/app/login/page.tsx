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
    return <h1>Instituição não encontrada.</h1>;
  }
  return (
    <div className={styles.container}>
      <div className={styles.left}>
        <div className={styles.textform}>          
          
          <div className={styles.textlogin}>
            <h2>Faça seu Login</h2>
            <p>Insira seus dados abaixo!</p>
          </div>
        </div>
        <LoginForm
          instituicaoId={instituicao.id}
          styles={styles}
        />
        <p className={styles.register}>
          Não possui conta?
          <a href="/cadastro">            {" "}Crie uma conta          </a>
        </p>
      </div>
      <div className={styles.right}></div>
    </div>
  );
}