namespace InsureYouAI.Dtos
{
    public class GeminiRequestDto
    {
        public List<Content> contents { get; set; }
        public GenerationConfig generationConfig { get; set; }

    }

    public class GenerationConfig
    {
        public double temperature { get; set; }
        // public int maxOutputTokens { get; set; }
    }

    public class Content
    {
        public List<Part> parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}
