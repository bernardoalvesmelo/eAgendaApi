using eAgenda.Aplicacao.ModuloDespesa;
using eAgenda.Dominio.ModuloDespesa;
using eAgenda.Infra.Orm.ModuloDespesa;
using eAgenda.Infra.Orm;
using eAgenda.WebApi.ViewModels.ModuloDespesa;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eAgenda.WebApi.ViewModels.ModuloCategoria;
using System.Drawing;

namespace eAgenda.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DespesaController : ControllerBase
    {
        private ServicoDespesa servicoDespesa;
        private ServicoCategoria servicoCategoria;

        public DespesaController()
        {
            IConfiguration configuracao = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            var connectionString = configuracao.GetConnectionString("SqlServer");

            var builder = new DbContextOptionsBuilder<eAgendaDbContext>();

            builder.UseSqlServer(connectionString);

            var contextoPersistencia = new eAgendaDbContext(builder.Options);

            var repositorioDespesa = new RepositorioDespesaOrm(contextoPersistencia);

            var repositorioCategoria = new RepositorioCategoriaOrm(contextoPersistencia);

            servicoDespesa = new ServicoDespesa(repositorioDespesa, contextoPersistencia);

            servicoCategoria = new ServicoCategoria(repositorioCategoria, contextoPersistencia);
        }

        [HttpGet]
        public List<ListarDespesaViewModel> SeleciontarTodos()
        {
            var despesas = servicoDespesa.SelecionarTodos().Value;

            var despesasViewModel = new List<ListarDespesaViewModel>();

            foreach (var d in despesas)
            {
                var despesaViewModel = new ListarDespesaViewModel
                {
                    Id = d.Id,
                    Descricao = d.Descricao,
                    Valor = d.Valor,
                    FormaPagamento = d.FormaPagamento.ToString()
                };

                despesasViewModel.Add(despesaViewModel);
            }

            return despesasViewModel;
        }

        [HttpGet("{id}")]
        public FormsDespesaViewModel SeleciontarPorId(Guid id)
        {
            var despesa = servicoDespesa.SelecionarPorId(id).Value;

            var despesaViewModel = new FormsDespesaViewModel
            {
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                FormaPagamento = despesa.FormaPagamento
            };

            foreach(var c in despesa.Categorias)
            {
                despesaViewModel.CategoriasSelecionadas.Add(c.Id);
            }

            return despesaViewModel;
        }

        [HttpGet("visualizacao-completa/{id}")]
        public VisualizarDespesaViewModel SeleciontarPorIdCompleto(Guid id)
        {
            var despesa = servicoDespesa.SelecionarPorId(id).Value;

            var despesaViewModel = new VisualizarDespesaViewModel
            {
                Id = despesa.Id,
                Descricao = despesa.Descricao,
                Valor = despesa.Valor,
                FormaPagamento = despesa.FormaPagamento.ToString()
            };

            foreach (var c in despesa.Categorias)
            {

                var categoriaViewModel = new ListarCategoriaViewModel
                {
                    Id = c.Id,
                    Titulo = c.Titulo
                };

                despesaViewModel.Categorias.Add(categoriaViewModel);
            }

            return despesaViewModel;
        }

        [HttpPost]
        public string Inserir(FormsDespesaViewModel despesaViewModel)
        {
            Despesa despesa = new Despesa
            {
                Descricao = despesaViewModel.Descricao,
                Valor = despesaViewModel.Valor,
                FormaPagamento = despesaViewModel.FormaPagamento
            };

            foreach(var c in despesaViewModel.CategoriasSelecionadas)
            {
                var resultadoBusca = servicoCategoria.SelecionarPorId(c);

                if (resultadoBusca.IsFailed)
                {

                    string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                    return string.Join("\r\n", errosBusca);
                }

                var categoria = resultadoBusca.Value;

                despesa.Categorias.Add(categoria);
            }

            var resultado = servicoDespesa.Inserir(despesa);

            if (resultado.IsSuccess)
                return "Despesa inserida com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpPut("{id}")]
        public string Editar(Guid id, FormsDespesaViewModel despesaViewModel)
        {
            var resultadoBusca = servicoDespesa.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {

                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var despesa = resultadoBusca.Value;

            despesa.Descricao = despesaViewModel.Descricao;
            despesa.Valor = despesaViewModel.Valor;
            despesa.FormaPagamento = despesaViewModel.FormaPagamento;

            despesa.Categorias.Clear();
            foreach (var c in despesaViewModel.CategoriasSelecionadas)
            {
                var resultadoBuscaCategoria = servicoCategoria.SelecionarPorId(c);

                if (resultadoBuscaCategoria.IsFailed)
                {

                    string[] errosBuscaCategoria = resultadoBuscaCategoria.Errors.Select(x => x.Message).ToArray();

                    return string.Join("\r\n", errosBuscaCategoria);
                }

                var categoria = resultadoBuscaCategoria.Value;

                despesa.Categorias.Add(categoria);
            }

            var resultado = servicoDespesa.Editar(despesa);

            if (resultado.IsSuccess)
                return "Despesa editada com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpDelete("{id}")]
        public string Excluir(Guid id)
        {
            var resultadoBusca = servicoDespesa.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {

                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var resultado = servicoDespesa.Excluir(id);

            if (resultado.IsSuccess)
                return "Despesa excluída com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }
    }
}
