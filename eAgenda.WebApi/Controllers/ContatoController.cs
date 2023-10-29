using eAgenda.Aplicacao.ModuloContato;
using eAgenda.Dominio.ModuloContato;
using eAgenda.Infra.Orm;
using eAgenda.Infra.Orm.ModuloContato;
using eAgenda.WebApi.ViewModels.ModuloCompromisso;
using eAgenda.WebApi.ViewModels.ModuloContato;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eAgenda.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContatoController : ControllerBase
    {
        private ServicoContato servicoContato;

        public ContatoController()
        {
            IConfiguration configuracao = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            var connectionString = configuracao.GetConnectionString("SqlServer");

            var builder = new DbContextOptionsBuilder<eAgendaDbContext>();

            builder.UseSqlServer(connectionString);

            var contextoPersistencia = new eAgendaDbContext(builder.Options);

            var repositorioContato = new RepositorioContatoOrm(contextoPersistencia);

            servicoContato = new ServicoContato(repositorioContato, contextoPersistencia);
        }

        [HttpGet]
        public List<ListarContatoViewModel> SeleciontarTodos(StatusFavoritoEnum statusFavoritoEnum)
        {
            var contatos = servicoContato.SelecionarTodos(statusFavoritoEnum).Value;

            var contatosViewModel = new List<ListarContatoViewModel>();

            foreach (var c in contatos)
            {
                var contatoViewModel = new ListarContatoViewModel
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Cargo = c.Cargo,
                    Empresa = c.Empresa,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    Favorito = c.Favorito,
                };

                contatosViewModel.Add(contatoViewModel);
            }

            return contatosViewModel;
        }

        [HttpGet("visualizacao-completa/{id}")]
        public FormsContatoViewModel SeleciontarPorId(string id)
        {
            var contato = servicoContato.SelecionarPorId(Guid.Parse(id)).Value;

            var contatoViewModel = new FormsContatoViewModel
            {
                Nome = contato.Nome,
                Cargo = contato.Cargo,
                Empresa = contato.Empresa,
                Email = contato.Email,
                Telefone = contato.Telefone,
                Favorito = contato.Favorito
            };

            return contatoViewModel;
        }

        [HttpGet("visualizacao-completa/{id}")]
        public VisualizarContatoViewModel SeleciontarPorIdCompleto(string id)
        {
            var contato = servicoContato.SelecionarPorId(Guid.Parse(id)).Value;

            var contatoViewModel = new VisualizarContatoViewModel
            {
                Id = contato.Id,
                Nome = contato.Nome,
                Cargo = contato.Cargo,
                Empresa = contato.Empresa,
                Email = contato.Email,
                Telefone = contato.Telefone,
                Favorito = contato.Favorito
            };

            foreach (var c in contato.Compromissos)
            {

                var compromissoViewModel = new ListarCompromissoViewModel
                {
                    Id = c.Id,
                    Assunto = c.Assunto,
                    Data = c.Data,
                    HoraInicio = c.HoraInicio.ToString(@"hh\:mm\:ss"),
                    HoraTermino = c.HoraTermino.ToString(@"hh\:mm\:ss")
                };

                contatoViewModel.ListarCompromissosViewModel.Add(compromissoViewModel);
            }

            return contatoViewModel;
        }

        [HttpPost]
        public FormsContatoViewModel Inserir(FormsContatoViewModel contatoViewModel)
        {
            Contato contato = new Contato
            {
                Nome = contatoViewModel.Nome,
                Email = contatoViewModel.Email,
                Telefone = contatoViewModel.Telefone,
                Cargo = contatoViewModel.Cargo,
                Empresa = contatoViewModel.Empresa,
                Favorito = contatoViewModel.Favorito
            };

            servicoContato.Inserir(contato);

            return contatoViewModel;
        }

        [HttpPut("{id}")]
        public FormsContatoViewModel Editar(Guid id, FormsContatoViewModel contatoViewModel)
        {
            var contato = servicoContato.SelecionarPorId(id).Value;

            contato.Nome = contatoViewModel.Nome;
            contato.Email = contatoViewModel.Email;
            contato.Telefone = contatoViewModel.Telefone;
            contato.Cargo = contatoViewModel.Cargo;
            contato.Empresa = contatoViewModel.Empresa;
            contato.Favorito = contatoViewModel.Favorito;

            servicoContato.Editar(contato);

            return contatoViewModel;
        }

        [HttpPut("favoritos/{id}")]
        public void MudarFavorito(Guid id)
        {
            var contato = servicoContato.SelecionarPorId(id).Value;

            servicoContato.ConfigurarFavoritos(contato);

            return;
        }

        [HttpDelete("{id}")]
        public void Excluir(Guid id)
        {
            servicoContato.Excluir(id);
            
            return;
        }
    }
}
