namespace InsureYouAI.Dtos
{
    public class GeminiResponseDto
    {
        public List<Candidate> candidates { get; set; }
    }

    public class Candidate
    {
        public GeminiContent content { get; set; }
    }

    public class GeminiContent
    {
        public List<GeminiPart> parts { get; set; }
    }

    public class GeminiPart
    {
        public string text { get; set; }
    }

}
