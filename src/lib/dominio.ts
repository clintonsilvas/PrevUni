import { headers } from "next/headers";

export async function obterDominio() {
  const cabecalhos = await headers();

  console.log("host:", cabecalhos.get("host"));
  console.log(
    "x-forwarded-host:",
    cabecalhos.get("x-forwarded-host")
  );

  const host =
    cabecalhos.get("x-forwarded-host") ??
    cabecalhos.get("host") ??
    "";

  return host.split(",")[0].trim().split(":")[0].toLowerCase();
}
