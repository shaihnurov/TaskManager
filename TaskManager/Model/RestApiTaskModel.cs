
namespace TaskManager.Model
{
    public class RestApiTaskModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool Completed { get; set; }
    }
}
