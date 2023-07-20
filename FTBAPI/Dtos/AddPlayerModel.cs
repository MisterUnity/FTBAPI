using FTBAPI.Validations;
using System.ComponentModel.DataAnnotations;

namespace FTBAPI.Dtos
{
    public class AddPlayerModel
    {
        //[NonSpace, MaxLength(10)]
        public string name { get; set; } = null!;
        //[NonSpace, MaxLength(1)]
        public string gender { get; set; } = null!;
        //[NonSpace, MaxLength(10)]
        public string height { get; set; } = null!;
        //[NonSpace, MaxLength(10)]
        public string weight { get; set; } = null!;
        //[NonSpace, MaxLength(10)]
        public string? position { get; set; }
        //[NonSpace, MaxLength(500)]
        public FormFileCollection? photo { get; set; }
        //[NonSpace, MaxLength(600)]
        public string? description { get; set; }
        //[NonSpace, MaxLength(10)]
        public string? nextPosition { get; set; }
        public int? seniority { get; set; }
        //[NonSpace, MaxLength(30)]
        public string? brithday { get; set; }
    }
}
