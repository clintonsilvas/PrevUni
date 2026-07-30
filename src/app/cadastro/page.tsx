import CadastroForm from "./CadastroForm";

import styles from "../login/login.module.css";


export default async function Login() {  
  return (
    <div className={`${styles.container} ${styles.inverso}`}>             
      <div className={styles.left}>
        <div className={styles.textform}>
          <div className={styles.textlogin}>
            <h2>Seja bem-vindo</h2>
            <p>Insira seus dados abaixo!</p>
          </div>
        </div>
        <CadastroForm
          styles={styles}
        />
        <p className={styles.register}>
          Já possui uma conta? 
          <a href="/login">Entrar</a>
        </p>
      </div>   
      <div className={styles.right}>
      </div>         
    </div>
  );
}