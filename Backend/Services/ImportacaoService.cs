using System.Collections.Concurrent;
using Backend.Models;

namespace Backend.Services
{
    public class ImportacaoService
    {
        private readonly UnifenasService _apiService;
        private readonly MongoService _mongoService;

        private static readonly ConcurrentDictionary<string, ImportacaoStatus> _importacaoStatuses = new();

        public ImportacaoService(UnifenasService apiService, MongoService mongoService)
        {
            _apiService = apiService;
            _mongoService = mongoService;
        }

        public ImportacaoStatus IniciarNovaImportacao()
        {
            var importacaoId = Guid.NewGuid().ToString();
            var status = new ImportacaoStatus
            {
                Id = importacaoId,
                Status = "Iniciada",
                Mensagem = "Importação iniciada com sucesso. Aguardando processamento...",
                ProgressoAtual = 0,
                TotalUsuarios = 0,
                Inicio = DateTime.UtcNow
            };
            _importacaoStatuses[importacaoId] = status;
            return status;
        }

        public ImportacaoStatus? GetImportacaoStatus(string importacaoId)
        {
            _importacaoStatuses.TryGetValue(importacaoId, out var status);
            return status;
        }

        public async Task ProcessarUsuariosInBackgroundAsync(string importacaoId)
        {
            if (!_importacaoStatuses.TryGetValue(importacaoId, out var status))
            {
                _importacaoStatuses[importacaoId] = new ImportacaoStatus
                {
                    Id = importacaoId,
                    Status = "Erro Interno",
                    Mensagem = "Status da importação não encontrado."
                };
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Iniciando GetUsuarios().");
                var usuarios = await _apiService.GetUsuariosAsync();
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: {usuarios.Count} usuários obtidos.");

                status.TotalUsuarios = usuarios.Count;
                status.Status = "Em Andamento";
                status.Mensagem = "Processando usuários...";

                int maxParallel = 5;
                var semaphore = new SemaphoreSlim(maxParallel);

                var tasks = usuarios.Select(async (usuario, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Obtendo logs do usuário {usuario.user_id} ({index + 1}/{usuarios.Count})");
                        var logs = await _apiService.GetLogsAsync(usuario.user_id);
                        await _mongoService.SalvarLogsAsync(usuario.user_id, logs);

                        status.ProgressoAtual++;
                        status.Mensagem = $"Processando usuário: {usuario.user_id} ({status.ProgressoAtual}/{status.TotalUsuarios})";
                        Console.WriteLine(status.Mensagem);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                status.Status = "Concluída";
                status.Mensagem = "Importação concluída com sucesso.";
                status.Fim = DateTime.UtcNow;

                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Concluída.");
            }
            catch (Exception ex)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro durante a importação: {ex.Message}";
                status.Fim = DateTime.UtcNow;

                Console.WriteLine($"[{DateTime.Now}] Erro na importação {importacaoId}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public async Task ProcessarUsuariosSomenteComMudancaAsync(string importacaoId)
        {
            if (!_importacaoStatuses.TryGetValue(importacaoId, out var status))
            {
                _importacaoStatuses[importacaoId] = new ImportacaoStatus
                {
                    Id = importacaoId,
                    Status = "Erro Interno",
                    Mensagem = "Status da importação não encontrado."
                };
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Iniciando...");

                var usuariosApi = await _apiService.GetUsuariosAsync();
                var usuariosMongo = await _mongoService.GetUsuariosComUltimoAcessoAsync();

                var usuariosParaImportar = usuariosApi
                    .Where(apiUser =>
                    {
                        var mongoUser = usuariosMongo.FirstOrDefault(m => m.user_id == apiUser.user_id);
                        return mongoUser == null || mongoUser.user_lastaccess != apiUser.user_lastaccess;
                    })
                    .ToList();

                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: {usuariosParaImportar.Count} usuários com alteração.");

                status.TotalUsuarios = usuariosParaImportar.Count;
                status.Status = "Em Andamento";
                status.Mensagem = "Processando usuários com mudanças...";

                int maxParallel = 2;
                var semaphore = new SemaphoreSlim(maxParallel);

                var tasks = usuariosParaImportar.Select(async (usuario, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now}] Processando usuário {usuario.user_id} ({index + 1}/{usuariosParaImportar.Count})");
                        var logs = await _apiService.GetLogsAsync(usuario.user_id);
                        await _mongoService.SalvarLogsAsync(usuario.user_id, logs);

                        status.ProgressoAtual++;
                        status.Mensagem = $"Processando usuário {usuario.user_id} ({status.ProgressoAtual}/{status.TotalUsuarios})";
                        Console.WriteLine(status.Mensagem);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                status.Status = "Concluída";
                status.Mensagem = "Importação concluída com sucesso.";
                status.Fim = DateTime.UtcNow;

                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Concluída com sucesso.");
            }
            catch (Exception ex)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro: {ex.Message}";
                status.Fim = DateTime.UtcNow;

                Console.WriteLine($"[{DateTime.Now}] Erro na importação {importacaoId}: {ex.Message}");
            }
        }
    }
}
