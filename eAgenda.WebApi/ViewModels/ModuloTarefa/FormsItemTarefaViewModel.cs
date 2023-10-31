using eAgenda.Dominio.ModuloTarefa;

namespace eAgenda.WebApi.ViewModels.ModuloTarefa
{
    public class FormsItemTarefaViewModel
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public StatusItemTarefa Status { get; set; }
        public bool Concluido { get; set; }
    }
}
