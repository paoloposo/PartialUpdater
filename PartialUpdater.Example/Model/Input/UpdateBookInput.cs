namespace PartialUpdater.Example.Model.Input;

public class UpdateBookInput
{
    public string? DifferentlyNamedTitle { get; set; }
    public string? Author { get; set; }
    public bool? Increment { get; set; }
}