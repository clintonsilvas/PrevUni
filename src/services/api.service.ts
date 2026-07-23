import { PrevUniData } from "@/interfaces/types";

export class ApiService {

    async getDados(endpoint: string): Promise<PrevUniData> {
        const response = await fetch(endpoint);
        if (!response.ok) {
            throw new Error("Erro ao buscar dados da instituição.");
        }
        return await response.json();
    }
}