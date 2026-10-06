import { cookies } from "next/headers";
import { verificarToken } from "@/lib/auth";

export async function verificarAdminApi() {
  const cookieStore = await cookies();

  const token = cookieStore.get("prevuni_admin_token")?.value;

  if (!token) {
    return false;
  }

  const payload = await verificarToken(token);

  if (!payload) {
    return false;
  }

  if (payload.tipo !== "admin") {
    return false;
  }

  return true;
}