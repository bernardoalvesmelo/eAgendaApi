using eAgenda.Aplicacao.ModuloTarefa;
using eAgenda.Dominio.ModuloTarefa;
using eAgenda.Infra.Orm.ModuloTarefa;
using eAgenda.Infra.Orm;
using eAgenda.WebApi.ViewModels.ModuloTarefa;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eAgenda.Dominio.ModuloContato;

namespace eAgenda.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TarefaController : ControllerBase
    {
        private ServicoTarefa servicoTarefa;

        public TarefaController()
        {
            IConfiguration configuracao = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            var connectionString = configuracao.GetConnectionString("SqlServer");

            var builder = new DbContextOptionsBuilder<eAgendaDbContext>();

            builder.UseSqlServer(connectionString);

            var contextoPersistencia = new eAgendaDbContext(builder.Options);

            var repositorioTarefa = new RepositorioTarefaOrm(contextoPersistencia);

            servicoTarefa = new ServicoTarefa(repositorioTarefa, contextoPersistencia);
        }

        [HttpGet]
        public List<ListarTarefaViewModel> SeleciontarTodos(StatusTarefaEnum statusTarefaEnum)
        {
            var tarefas = servicoTarefa.SelecionarTodos(statusTarefaEnum).Value;

            var tarefasViewModel = new List<ListarTarefaViewModel>();

            foreach (var t in tarefas)
            {
                var tarefaViewModel = new ListarTarefaViewModel
                {
                    Id = t.Id,
                    Titulo = t.Titulo,
                    DataCriacao = t.DataCriacao,
                    Prioridade = t.Prioridade.ToString(), 
                    Situacao = t.PercentualConcluido == 100 ? "Concluído" : "Pendente"
                };

                tarefasViewModel.Add(tarefaViewModel);
            }

            return tarefasViewModel;
        }

        [HttpGet("{id}")]
        public FormsTarefaViewModel SeleciontarPorId(Guid id)
        {
            var tarefa = servicoTarefa.SelecionarPorId(id).Value;

            var tarefaViewModel = new FormsTarefaViewModel
            {
                Titulo = tarefa.Titulo,
     
                Prioridade = tarefa.Prioridade,
            };

            foreach(var i in tarefa.Itens)
            {
                var item = new FormsItemTarefaViewModel
                {
                    Id = i.Id,
                    Titulo = i.Titulo,
                    Status = 0,
                    Concluido = i.Concluido,
                };

                tarefaViewModel.Itens.Add(item);
            }

            return tarefaViewModel;
        }

        [HttpGet("visualizacao-completa/{id}")]
        public VisualizarTarefaViewModel SeleciontarPorIdCompleto(Guid id)
        {
            var tarefa = servicoTarefa.SelecionarPorId(id).Value;

            var tarefaViewModel = new VisualizarTarefaViewModel
            {
                Id = tarefa.Id,
                Titulo = tarefa.Titulo,
                DataCriacao = tarefa.DataCriacao,
                DataConclusao = tarefa.DataConclusao,
                QuantidadeItens = tarefa.Itens.Count(),
                PercentualConcluido = tarefa.PercentualConcluido,
                Prioridade = tarefa.Prioridade.ToString(),
                Situacao = tarefa.PercentualConcluido == 100 ? "Concluído" : "Pendente"
            };

            foreach (var i in tarefa.Itens)
            {

                var item = new VisualizarItemTarefaViewModel
                {
                    Titulo = i.Titulo,
                    Situacao = i.Concluido ? "Concluído" : "Pendente",
                };

                tarefaViewModel.Itens.Add(item);
            }

            return tarefaViewModel;
        }

        [HttpPost]
        public string Inserir(FormsTarefaViewModel tarefaViewModel)
        {
            Tarefa tarefa = new Tarefa
            {
                Titulo = tarefaViewModel.Titulo,           
                Prioridade = tarefaViewModel.Prioridade,
            };

            foreach(var i in tarefaViewModel.Itens)
            {
                if(i.Status.Equals(StatusItemTarefa.Adicionado))
                {
                    var item = new ItemTarefa
                    {
                        Titulo = i.Titulo,
                        Concluido = i.Concluido
                    };

                    tarefa.AdicionarItem(item);
                }
            }

            servicoTarefa.AtualizarItens
            (
                tarefa, tarefa.Itens.FindAll(i => i.Concluido),
                tarefa.Itens.FindAll(i => !i.Concluido)
            );

            var resultado = servicoTarefa.Inserir(tarefa);

            if (resultado.IsSuccess)
                return "Tarefa inserida com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpPut("{id}")]
        public string Editar(Guid id, FormsTarefaViewModel tarefaViewModel)
        {
            var resultadoBusca = servicoTarefa.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {

                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var tarefa = resultadoBusca.Value;

            tarefa.Titulo = tarefaViewModel.Titulo;
            tarefa.Prioridade = tarefaViewModel.Prioridade;

            foreach (var i in tarefaViewModel.Itens)
            {
                if(i.Status.Equals(StatusItemTarefa.Adicionado))
                {
                    var item = new ItemTarefa
                    {
                        Titulo = i.Titulo,
                        Concluido = i.Concluido
                    };

                    tarefa.AdicionarItem(item);
                }

                else if(i.Status.Equals(StatusItemTarefa.Removido))
                {
                    tarefa.RemoverItem(i.Id);
                }
            }

            servicoTarefa.AtualizarItens
            (
                tarefa, tarefa.Itens.FindAll(i => i.Concluido),
                tarefa.Itens.FindAll(i => !i.Concluido)
            );

            var resultado = servicoTarefa.Editar(tarefa);

            if (resultado.IsSuccess)
                return "Tarefa editada com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpDelete("{id}")]
        public string Excluir(Guid id)
        {
            var resultadoBusca = servicoTarefa.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {
                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var resultado = servicoTarefa.Excluir(id);

            if (resultado.IsSuccess)
                return "Tarefa excluída com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }
    }
}
