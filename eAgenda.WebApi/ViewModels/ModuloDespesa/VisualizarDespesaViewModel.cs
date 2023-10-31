using eAgenda.WebApi.ViewModels.ModuloCategoria;

namespace eAgenda.WebApi.ViewModels.ModuloDespesa
{
    public class VisualizarDespesaViewModel
    {
        public VisualizarDespesaViewModel()
        {
            Categorias = new List<ListarCategoriaViewModel>();
        }

        public Guid Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string FormaPagamento { get; set; }
        public List<ListarCategoriaViewModel> Categorias { get; set; }
    }
}
