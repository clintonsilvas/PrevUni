import { headers } from "next/headers";

export async function obterDominio() {
  const host = (await headers()).get("host") ?? "";

  return host.split(":")[0];
}