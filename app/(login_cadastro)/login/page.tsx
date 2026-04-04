"use client";
import { useState, useEffect } from "react";
import Image from "next/image";
import "./login.style.css";
import InputText from "../../components/inputText/inputText.component";
import Button from "../../components/button/button.component";
import useIsMobile from "../../utils/isMobile";

function Login() {
  const [emailAdded, setEmail] = useState("");
  const [passwordAdded, setPassword] = useState("");
  const [EntrarClick, setEntrarClick] = useState(false);
  const isMobile = useIsMobile();

  function ClickEntrar() {
    alert("click");
    setEntrarClick(false);
  }

  return (
    <main className="body-container">
      <div className="form-container">
        <Image
          width={160}
          height={32}
          className="logo-svg"
          src="./logo.svg"
          alt="logoPrevUni"
        ></Image>

        <div className="header-container">
          <h1>Faça seu Login</h1>
          <p>Insira seus dados abaixo!</p>
        </div>
        <div className="inputs-container">
          <InputText
            name="e-mail"
            placeholder="Insira aqui seu e-mail"
            showLabel={true}
            labelText="E-mail:"
            typeInput="email"
            onAddContent={setEmail}
          />
          <InputText
            name="password"
            placeholder="Insira aqui sua senha"
            showLabel={true}
            labelText="Senha:"
            typeInput="password"
            onAddContent={setPassword}
          />
        </div>
        <div className="submitContainer">
          <Button
            content="Entrar"
            style="primary"
            onClickButton={() => ClickEntrar()}
          />

          <Button
            content="Entrar com o Google"
            onClickButton={setEntrarClick}
            style="secondary"
          />
          <span className="createAccountLabel">
            Não possui uma conta? {isMobile ? <br /> : null}
            <a href="/cadastro">Criar uma conta</a>
          </span>
        </div>
      </div>
      <div className="image-container"></div>
    </main>
  );
}
export default Login;
