"use client";

export default function Dashboard() {

  async function logout() {

    await fetch("/api/logout", {
      method: "POST",
    });

    window.location.href = "/login";
  }


  return (
    <>
      <h1>
        Login realizado com sucesso!
      </h1>

      <button onClick={logout}>
        Sair
      </button>
    </>
  );
}