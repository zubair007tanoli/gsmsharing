namespace gsmsharing.Models.APIModels
{
    public class GptRequest
    {
        public string Model { get; set; }
        public List<GptMessage> Messages { get; set; }
    }
}
