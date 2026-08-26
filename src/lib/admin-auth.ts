import { cookies } from "next/headers";
import { redirect } from "next/navigation";

import { verificarToken } from "@/lib/auth";

export async function obterAdminAutenticado() {

  const cookieStore = await cookies();

  const token = cookieStore.get(
    "prevuni_admin_token"
  )?.value;

  if (!token) {
    redirect("/admin/login");
  }

  const payload = await verificarToken(token);

  if (!payload) {
    redirect("/admin/login");
  }

  if (payload.tipo !== "admin") {
    redirect("/admin/login");
  }

  return payload;
}