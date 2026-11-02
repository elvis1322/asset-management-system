namespace Application.Models
{
    public static class GenderHelper
    {
        public static string? NormalizeGender(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            return input.Trim().ToUpperInvariant() switch
            {
                "M" or "MALE" => "Male",
                "F" or "FEMALE" => "Female",
                _ => null
            };
        }
    }
} 
