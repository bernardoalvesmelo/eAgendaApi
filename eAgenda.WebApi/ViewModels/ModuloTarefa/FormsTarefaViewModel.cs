using eAgenda.Dominio.ModuloTarefa;
namespace eAgenda.WebApi.ViewModels.ModuloTarefa
{
    public class FormsTarefaViewModel
    {
        public FormsTarefaViewModel() 
        {
            Itens = new List<FormsItemTarefaViewModel>();
        }

        public string Titulo { get; set; }
        public PrioridadeTarefaEnum Prioridade { get; set; }
        public List<FormsItemTarefaViewModel> Itens { get; set; }
    }
}
