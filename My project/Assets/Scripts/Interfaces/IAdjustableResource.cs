// IAdjustableResource.cs
public interface IAdjustableResource
{
    /// <summary>
    /// Called by the Dial when its normalized value changes (0…1).
    /// </summary>
    void ApplyDialValue(float normalized);
}
