using eAgenda.Aplicacao.ModuloDespesa;
using eAgenda.Dominio.ModuloDespesa;
using eAgenda.Infra.Orm.ModuloDespesa;
using eAgenda.Infra.Orm;
using eAgenda.WebApi.ViewModels.ModuloCategoria;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eAgenda.WebApi.ViewModels.ModuloDespesa;

namespace eAgenda.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private ServicoCategoria servicoCategoria;

        public CategoriaController()
        {
            IConfiguration configuracao = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            var connectionString = configuracao.GetConnectionString("SqlServer");

            var builder = new DbContextOptionsBuilder<eAgendaDbContext>();

            builder.UseSqlServer(connectionString);

            var contextoPersistencia = new eAgendaDbContext(builder.Options);

            var repositorioCategoria = new RepositorioCategoriaOrm(contextoPersistencia);

            servicoCategoria = new ServicoCategoria(repositorioCategoria, contextoPersistencia);
        }

        [HttpGet]
        public List<ListarCategoriaViewModel> SeleciontarTodos()
        {
            var categorias = servicoCategoria.SelecionarTodos().Value;

            var categoriasViewModel = new List<ListarCategoriaViewModel>();

            foreach (var c in categorias)
            {
                var categoriaViewModel = new ListarCategoriaViewModel
                {
                    Id = c.Id,
                    Titulo = c.Titulo
                };

                categoriasViewModel.Add(categoriaViewModel);
            }

            return categoriasViewModel;
        }

        [HttpGet("{id}")]
        public FormsCategoriaViewModel SeleciontarPorId(Guid id)
        {
            var categoria = servicoCategoria.SelecionarPorId(id).Value;

            var categoriaViewModel = new FormsCategoriaViewModel
            {
                Titulo = categoria.Titulo
            };

            return categoriaViewModel;
        }

        [HttpGet("visualizacao-completa/{id}")]
        public VisualizarCategoriaViewModel SeleciontarPorIdCompleto(Guid id)
        {
            var categoria = servicoCategoria.SelecionarPorId(id).Value;

            var categoriaViewModel = new VisualizarCategoriaViewModel
            {
                Id = categoria.Id,
                Titulo = categoria.Titulo
            };

            foreach (var d in categoria.Despesas)
            {

                var despesaViewModel = new ListarDespesaViewModel
                {
                    Id = d.Id,
                    Descricao = d.Descricao,
                    Valor = d.Valor,
                    FormaPagamento = nameof(d.FormaPagamento)
                };

                categoriaViewModel.Despesas.Add(despesaViewModel);
            }

            return categoriaViewModel;
        }

        [HttpPost]
        public string Inserir(FormsCategoriaViewModel categoriaViewModel)
        {
            Categoria categoria = new Categoria
            {
                Titulo = categoriaViewModel.Titulo
            };

            var resultado = servicoCategoria.Inserir(categoria);

            if (resultado.IsSuccess)
                return "Categoria inserida com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpPut("{id}")]
        public string Editar(Guid id, FormsCategoriaViewModel categoriaViewModel)
        {
            var resultadoBusca = servicoCategoria.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {

                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var categoria = resultadoBusca.Value;

            categoria.Titulo = categoriaViewModel.Titulo;

            var resultado = servicoCategoria.Editar(categoria);

            if (resultado.IsSuccess)
                return "Categoria editada com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }

        [HttpDelete("{id}")]
        public string Excluir(Guid id)
        {
            var resultadoBusca = servicoCategoria.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
            {

                string[] errosBusca = resultadoBusca.Errors.Select(x => x.Message).ToArray();

                return string.Join("\r\n", errosBusca);
            }

            var resultado = servicoCategoria.Excluir(id);

            if (resultado.IsSuccess)
                return "Categoria excluída com sucesso";

            string[] erros = resultado.Errors.Select(x => x.Message).ToArray();

            return string.Join("\r\n", erros);
        }
    }
}
