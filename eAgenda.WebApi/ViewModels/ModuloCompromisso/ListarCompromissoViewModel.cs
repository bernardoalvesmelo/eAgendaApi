namespace eAgenda.WebApi.ViewModels.ModuloCompromisso
{
    public class ListarCompromissoViewModel
    {
        public Guid Id { get; set; }
        public string Assunto { get; set; }
        public DateTime Data { get; set; }
        public string HoraInicio { get; set; }
        public string HoraTermino { get; set; }
    }
}
